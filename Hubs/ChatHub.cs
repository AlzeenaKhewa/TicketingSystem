// TicketingSystem/Hubs/ChatHub.cs
using Microsoft.AspNetCore.SignalR;
using TicketingSystem.Data;
using TicketingSystem.Models;

namespace TicketingSystem.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ITicketRepository _ticketRepository;

        public ChatHub(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        // Method for a client (browser) to join a specific ticket's chat room
        public async Task JoinTicketGroup(string ticketId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
        }

        // Method for a client to leave a group (good practice)
        public async Task LeaveTicketGroup(string ticketId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
        }

        // Method called by a client to send a message
        public async Task SendMessage(string ticketId, string senderName, string message, bool isAdmin)
        {
            // 1. Create the ChatMessage object to save to DB
            var chatMessage = new ChatMessage
            {
                TicketId = int.Parse(ticketId),
                SenderName = senderName,
                Message = message,
                IsAdminMessage = isAdmin,
                SentAt = DateTime.UtcNow
            };

            // 2. Save the message to the database
            await _ticketRepository.CreateChatMessageAsync(chatMessage);

            // 3. Broadcast the message to all clients in the same ticket group
            await Clients.Group($"ticket-{ticketId}").SendAsync("ReceiveMessage", senderName, message, isAdmin, chatMessage.SentAt);
        }
    }
}