using TicketingSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TicketingSystem.Data
{
    public interface IChatRepository
    {
        Task<IEnumerable<ChatMessage>> GetConversationAsync(int ticketId, int currentUserId);
        Task<ChatMessage?> SaveMessageAsync(ChatMessage message);
        Task AddOrUpdateConnectionAsync(string connectionId, int userId);
        Task RemoveConnectionAsync(string connectionId);
        Task<IEnumerable<string>> GetUserConnectionsAsync(int userId);
        Task<IEnumerable<ChatUserViewModel>> GetUsersForTicketAsync(int ticketId);
    }
}