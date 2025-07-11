// TicketingSystem/Data/ITicketRepository.cs
using TicketingSystem.Models;

namespace TicketingSystem.Data
{
    public interface ITicketRepository
    {
        Task<IEnumerable<Ticket>> GetTicketsByCustomerIdAsync(int customerId);
        Task<int> CreateTicketAsync(Ticket ticket); // <-- CHANGE: Return the new ID

        // === NEW METHODS FOR CHAT ===
        Task<IEnumerable<ChatMessage>> GetChatMessagesAsync(int ticketId);
        Task CreateChatMessageAsync(ChatMessage message);
    }
}