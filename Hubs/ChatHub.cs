using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;
using TicketingSystem.Data;
using TicketingSystem.Models;
using Microsoft.Extensions.Logging; // Add this for logging

namespace TicketingSystem.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatRepository _chatRepo;
        private readonly ILogger<ChatHub> _logger; // Add a logger

        public ChatHub(IChatRepository chatRepo, ILogger<ChatHub> logger)
        {
            _chatRepo = chatRepo;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            // This logic is confirmed to be working
            var httpContext = Context.GetHttpContext();
            if (httpContext != null && httpContext.Request.Query.TryGetValue("userId", out var userIdStr))
            {
                if (int.TryParse(userIdStr, out var userId))
                {
                    return userId;
                }
            }
            return 1; // Fallback
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation($"--> User {userId} connected with ConnectionId: {Context.ConnectionId}");
            await _chatRepo.AddOrUpdateConnectionAsync(Context.ConnectionId, userId);
            await Clients.All.SendAsync("UserStatusChanged", userId, true);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation($"--> User {userId} disconnected with ConnectionId: {Context.ConnectionId}");
            await _chatRepo.RemoveConnectionAsync(Context.ConnectionId);
            var connections = await _chatRepo.GetUserConnectionsAsync(userId);
            if (!connections.Any())
            {
                await Clients.All.SendAsync("UserStatusChanged", userId, false);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinTicketRoom(string ticketId)
        {
            string groupName = $"ticket-{ticketId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation($"--> Connection {Context.ConnectionId} joined group: {groupName}");
        }

        public async Task SendPublicMessage(int ticketId, string message)
        {
            var senderId = GetCurrentUserId();
            _logger.LogInformation($"--> SendPublicMessage called by User {senderId} for Ticket {ticketId}. Message: '{message}'");

            var chatMessage = new ChatMessage
            {
                Content = message,
                TicketId = ticketId,
                SenderId = senderId,
                RecipientId = null,
                Timestamp = DateTime.UtcNow
            };

            var savedMessage = await _chatRepo.SaveMessageAsync(chatMessage);
            if (savedMessage != null)
            {
                string groupName = $"ticket-{ticketId}";
                _logger.LogInformation($"--> Broadcasting 'ReceivePublicMessage' to group '{groupName}'.");
                await Clients.Group(groupName).SendAsync("ReceivePublicMessage", savedMessage);
            }
            else
            {
                _logger.LogWarning("--> Message was NOT saved to the database. Broadcast cancelled.");
            }
        }

        public async Task SendPrivateMessage(int ticketId, int recipientId, string message)
        {
            var senderId = GetCurrentUserId();
            _logger.LogInformation($"--> SendPrivateMessage called by User {senderId} to User {recipientId} for Ticket {ticketId}.");

            var chatMessage = new ChatMessage
            {
                Content = message,
                TicketId = ticketId,
                SenderId = senderId,
                RecipientId = recipientId,
                Timestamp = DateTime.UtcNow
            };

            var savedMessage = await _chatRepo.SaveMessageAsync(chatMessage);
            if (savedMessage != null)
            {
                var senderConnections = await _chatRepo.GetUserConnectionsAsync(senderId);
                var recipientConnections = await _chatRepo.GetUserConnectionsAsync(recipientId);
                var connectionsToNotify = senderConnections.Concat(recipientConnections).Distinct().ToList();

                _logger.LogInformation($"--> Broadcasting 'ReceivePrivateMessage' to {connectionsToNotify.Count} specific connections.");

                if (connectionsToNotify.Any())
                {
                    await Clients.Clients(connectionsToNotify).SendAsync("ReceivePrivateMessage", savedMessage);
                }
            }
            else
            {
                _logger.LogWarning("--> Private message was NOT saved to the database. Broadcast cancelled.");
            }
        }
    }
}