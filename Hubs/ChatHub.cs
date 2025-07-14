using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;
using TicketingSystem.Data;
using TicketingSystem.Models;

namespace TicketingSystem.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatRepository _chatRepo;

        public ChatHub(IChatRepository chatRepo)
        {
            _chatRepo = chatRepo;
        }

        private int GetCurrentUserId()
        {
            // In a real app, get this from claims after authentication.
            // For this demo, we'll read it from the query string passed by the client.
            var httpContext = Context.GetHttpContext();
            if (httpContext != null && httpContext.Request.Query.TryGetValue("userId", out var userIdStr))
            {
                if (int.TryParse(userIdStr, out var userId))
                {
                    return userId;
                }
            }
            return 1; // Fallback for safety
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            await _chatRepo.AddOrUpdateConnectionAsync(Context.ConnectionId, userId);
            await Clients.All.SendAsync("UserStatusChanged", userId, true); // Notify everyone user is online
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();
            await _chatRepo.RemoveConnectionAsync(Context.ConnectionId);
            // Check if user has any other connections before marking them offline
            var connections = await _chatRepo.GetUserConnectionsAsync(userId);
            if (!connections.Any())
            {
                await Clients.All.SendAsync("UserStatusChanged", userId, false); // Notify everyone user is offline
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinTicketRoom(string ticketId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
        }

        public async Task SendPublicMessage(int ticketId, string message)
        {
            var chatMessage = new ChatMessage { Content = message, TicketId = ticketId, SenderId = GetCurrentUserId(), RecipientId = null, Timestamp = DateTime.UtcNow };
            var savedMessage = await _chatRepo.SaveMessageAsync(chatMessage);
            if (savedMessage != null)
                await Clients.Group($"ticket-{ticketId}").SendAsync("ReceivePublicMessage", savedMessage);
        }

        public async Task SendPrivateMessage(int ticketId, int recipientId, string message)
        {
            var senderId = GetCurrentUserId();
            var chatMessage = new ChatMessage { Content = message, TicketId = ticketId, SenderId = senderId, RecipientId = recipientId, Timestamp = DateTime.UtcNow };
            var savedMessage = await _chatRepo.SaveMessageAsync(chatMessage);
            if (savedMessage != null)
            {
                var senderConnections = await _chatRepo.GetUserConnectionsAsync(senderId);
                var recipientConnections = await _chatRepo.GetUserConnectionsAsync(recipientId);
                var connectionsToNotify = senderConnections.Concat(recipientConnections).Distinct().ToList();
                await Clients.Clients(connectionsToNotify).SendAsync("ReceivePrivateMessage", savedMessage);
            }
        }

        public async Task SendTypingIndicator(int ticketId, int? recipientId)
        {
            var users = await _chatRepo.GetUsersForTicketAsync(ticketId);
            var senderName = users.FirstOrDefault(u => u.UserId == GetCurrentUserId())?.Name ?? "A user";

            if (recipientId.HasValue)
            {
                var recipientConnections = await _chatRepo.GetUserConnectionsAsync(recipientId.Value);
                await Clients.Clients(recipientConnections).SendAsync("ReceiveTypingIndicator", senderName, GetCurrentUserId());
            }
            else
            {
                await Clients.GroupExcept($"ticket-{ticketId}", Context.ConnectionId).SendAsync("ReceiveTypingIndicator", senderName, null);
            }
        }
    }
}