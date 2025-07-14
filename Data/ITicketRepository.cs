using TicketingSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TicketingSystem.Data
{
    public interface ITicketRepository
    {
        Task<IEnumerable<Ticket>> GetAllTicketsAsync();
        Task<IEnumerable<Ticket>> GetTicketsByCustomerIdAsync(int customerId);
        Task<int> CreateTicketAsync(Ticket ticket);
        Task<Ticket?> GetByIdAsync(int ticketId); // Use one consistent, nullable method
    }
}