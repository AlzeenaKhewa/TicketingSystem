document.addEventListener("DOMContentLoaded", function () {
    // --- 1. SETUP & GRAB DOM ELEMENTS ---
    const chatWrapper = document.querySelector('.chat-wrapper');
    if (!chatWrapper) return;

    const ticketId = parseInt(chatWrapper.dataset.ticketId);
    const currentUserId = parseInt(chatWrapper.dataset.currentUserId);

    const messageInput = document.getElementById('message-input');
    const sendButton = document.getElementById('send-button');
    const messageList = document.getElementById('message-list');
    const userList = document.getElementById('user-list');
    const chatHeader = document.getElementById('chat-with-header');

    let activeChatUserId = null;

    // --- 2. CONFIGURE AND START SIGNALR CONNECTION ---
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chathub?userId=${currentUserId}") // This is correct!
        .withAutomaticReconnect()
        .build();

    async function start() {
        try {
            await connection.start();
            console.log("SignalR Connected.");
            await connection.invoke("JoinTicketRoom", ticketId.toString());
            console.log(`Client joined group for ticket ${ticketId}.`);
            switchChatView(null, 'Public Chat');
        } catch (err) {
            console.error("SignalR Connection Error: ", err);
            setTimeout(start, 5000);
        }
    }

    // --- 3. DEFINE CLIENT-SIDE HANDLERS ---
    connection.on("ReceivePublicMessage", (message) => {
        console.log("Public message received:", message);
        appendMessage(message);
    });

    connection.on("ReceivePrivateMessage", (message) => {
        console.log("Private message received:", message);
        appendMessage(message);
    });

    connection.on("UserStatusChanged", (userId, isOnline) => {
        const statusDot = document.getElementById(`status-dot-${userId}`);
        if (statusDot) {
            statusDot.className = `online-status-dot ${isOnline ? 'online' : 'offline'}`;
        }
    });

    // --- 4. UI HELPER FUNCTIONS ---
    function appendMessage(message) {
        const isCurrentUser = message.senderId === currentUserId;
        const messageOwnerClass = isCurrentUser ? 'current-user' : 'other-user';
        const messageTypeClass = message.recipientId ? 'private' : 'public';

        const row = document.createElement('div');
        row.className = `message-row ${messageOwnerClass} ${messageTypeClass}`;
        row.dataset.senderId = message.senderId;
        row.dataset.recipientId = message.recipientId || 0;

        let senderHtml = !isCurrentUser
            ? `<div class="message-sender">${escapeHtml(message.senderName)}</div>`
            : '';

        row.innerHTML = `
            <div class="message-content">
                ${senderHtml}
                <div class="message-bubble">${escapeHtml(message.content)}</div>
                <div class="message-timestamp">${new Date(message.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</div>
            </div>`;

        messageList.appendChild(row);
        filterAndScrollMessages();
    }

    function filterAndScrollMessages() {
        document.querySelectorAll('.message-row').forEach(row => {
            const senderId = parseInt(row.dataset.senderId);
            const recipientId = parseInt(row.dataset.recipientId);
            let show = false;

            if (activeChatUserId === null) {
                show = recipientId === 0;
            } else {
                show =
                    (senderId === currentUserId && recipientId === activeChatUserId) ||
                    (senderId === activeChatUserId && recipientId === currentUserId);
            }
            row.style.display = show ? 'flex' : 'none';
        });

        messageList.scrollTop = messageList.scrollHeight;
    }

    function switchChatView(targetUserId, targetUserName) {
        activeChatUserId = targetUserId;
        document.querySelectorAll('#user-list li').forEach(li => li.classList.remove('active'));
        const activeLi = targetUserId
            ? document.querySelector(`li[data-user-id='${targetUserId}']`)
            : document.getElementById('group-chat-tab');
        if (activeLi) activeLi.classList.add('active');

        chatHeader.textContent = targetUserName;
        messageInput.placeholder = `Message ${targetUserName}...`;
        filterAndScrollMessages();
    }

    function escapeHtml(unsafe) {
        return unsafe
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    // --- 5. BIND UI EVENT LISTENERS ---
    sendButton.addEventListener('click', () => {
        const message = messageInput.value.trim();
        if (message) {
            if (activeChatUserId) {
                connection.invoke("SendPrivateMessage", ticketId, activeChatUserId, message).catch(err => console.error(err));
            } else {
                connection.invoke("SendPublicMessage", ticketId, message).catch(err => console.error(err));
            }
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

    userList.addEventListener('click', (e) => {
        const targetLi = e.target.closest('li');
        if (targetLi && !targetLi.classList.contains('active')) {
            const userIdStr = targetLi.dataset.userId;
            const userId = userIdStr === 'null' ? null : parseInt(userIdStr);
            const userName = targetLi.textContent.trim().split('(')[0].trim();
            switchChatView(userId, userName);
        }
    });

    // --- 6. START THE APPLICATION ---
    start();
});
