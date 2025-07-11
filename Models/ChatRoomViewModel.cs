using TicketingSystem.Models;

namespace TicketingSystem.Models
{
    /// <summary>
    /// Holds all the data required for rendering the dedicated chat room page.
    /// </summary>
    public class ChatRoomViewModel
    {
        public Ticket TicketDetails { get; set; } = new Ticket();
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}