document.addEventListener("DOMContentLoaded", function () {
    const chatContainer = document.querySelector('.chat-wrapper');
    if (!chatContainer) return;

    const ticketId = parseInt(chatContainer.dataset.ticketId);
    const currentUserId = parseInt(chatContainer.dataset.currentUserId);

    const messageInput = document.getElementById('message-input');
    const sendButton = document.getElementById('send-button');
    const messageList = document.getElementById('message-list');
    const userList = document.getElementById('user-list');
    const chatHeader = document.getElementById('chat-with-header');
    const typingIndicator = document.getElementById('typing-indicator');

    let activeChatUserId = null; // null for group chat
    let typingTimeout;

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(`/chathub?userId=${currentUserId}`)
        .withAutomaticReconnect()
        .build();

    function appendMessage(message) {
        const isCurrentUser = message.senderId === currentUserId;
        const messageOwnerClass = isCurrentUser ? 'current-user' : 'other-user';
        const messageTypeClass = message.recipientId ? 'private' : 'public';

        const row = document.createElement('div');
        row.className = `message-row ${messageOwnerClass} ${messageTypeClass}`;
        row.dataset.senderId = message.senderId;
        row.dataset.recipientId = message.recipientId || 0;

        let senderHtml = '';
        if (!isCurrentUser) {
            senderHtml = `<div class="message-sender">${escapeHtml(message.senderName)}</div>`;
        }

        row.innerHTML = `
            <div class="message-content">
                ${senderHtml}
                <div class="message-bubble">${escapeHtml(message.content)}</div>
                <div class="message-timestamp">${new Date(message.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</div>
            </div>`;

        messageList.appendChild(row);

        // Filter visibility after appending
        filterMessages();
        messageList.scrollTop = messageList.scrollHeight;
    }

    function escapeHtml(unsafe) {
        return unsafe
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    function filterMessages() {
        document.querySelectorAll('.message-row').forEach(row => {
            const senderId = parseInt(row.dataset.senderId);
            const recipientId = parseInt(row.dataset.recipientId);
            let show = false;
            if (activeChatUserId === null) {
                show = recipientId === 0; // Group chat messages
            } else {
                show = (senderId === currentUserId && recipientId === activeChatUserId) ||
                    (senderId === activeChatUserId && recipientId === currentUserId);
            }
            row.style.display = show ? 'flex' : 'none';
        });
        messageList.scrollTop = messageList.scrollHeight;
    }

    function switchChat(targetUserId, targetUserName) {
        activeChatUserId = targetUserId;
        document.querySelectorAll('#user-list li').forEach(li => li.classList.remove('active'));
        const activeLi = targetUserId ? document.querySelector(`li[data-user-id='${targetUserId}']`) : document.getElementById('group-chat-tab');
        if (activeLi) activeLi.classList.add('active');

        if (targetUserId) {
            chatHeader.textContent = `Private Chat with ${targetUserName}`;
            messageInput.placeholder = `Your private message to ${targetUserName}...`;
        } else {
            chatHeader.textContent = `Group Chat`;
            messageInput.placeholder = 'Type your message for the group...';
        }
        filterMessages();
    }

    sendButton.addEventListener('click', () => {
        const message = messageInput.value.trim();
        if (message) {
            const method = activeChatUserId ? "SendPrivateMessage" : "SendPublicMessage";
            const args = activeChatUserId ? [ticketId, activeChatUserId, message] : [ticketId, message];
            connection.invoke(method, ...args).catch(err => console.error(err));
            messageInput.value = '';
            messageInput.focus();
        }
    });

    messageInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            sendButton.click();
        }
    });

    messageInput.addEventListener('input', () => {
        connection.invoke("SendTypingIndicator", ticketId, activeChatUserId).catch(err => console.error(err));
    });

    userList.addEventListener('click', (e) => {
        const targetLi = e.target.closest('li');
        if (targetLi && !targetLi.classList.contains('active')) {
            const userIdStr = targetLi.dataset.userId;
            const userId = userIdStr === 'null' ? null : parseInt(userIdStr);
            const userName = targetLi.textContent.trim().split('(')[0];
            switchChat(userId, userName);
        }
    });

    connection.on("ReceivePublicMessage", appendMessage);
    connection.on("ReceivePrivateMessage", appendMessage);

    connection.on("ReceiveTypingIndicator", (fromUserName, recipientId) => {
        const isForMyPrivateChat = recipientId === currentUserId && activeChatUserId === parseInt(fromUserName.split(' ')[0]);
        const isForMyGroup = recipientId === null && activeChatUserId === null;

        if (isForMyPrivateChat || isForMyGroup) {
            typingIndicator.textContent = `${fromUserName} is typing...`;
            clearTimeout(typingTimeout);
            typingTimeout = setTimeout(() => { typingIndicator.textContent = ""; }, 3000);
        }
    });

    connection.on("UserStatusChanged", (userId, isOnline) => {
        const statusDot = document.getElementById(`status-dot-${userId}`);
        if (statusDot) {
            statusDot.className = `online-status-dot ${isOnline ? 'online' : 'offline'}`;
        }
    });

    async function start() {
        try {
            await connection.start();
            console.log("SignalR Connected.");
            await connection.invoke("JoinTicketRoom", ticketId.toString());
            switchChat(null, 'Group'); // Default to group chat
        } catch (err) {
            console.error(err);
            setTimeout(start, 5000);
        }
    }

    start();
});
