using System.Collections.Generic;

namespace TicketingSystem.Models
{
    public class ChatUserViewModel
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
    }

    public class ChatRoomViewModel
    {
        public int TicketId { get; set; }
        public string TicketTitle { get; set; } = string.Empty;
        public List<ChatMessage> InitialMessages { get; set; } = new();
        public int CurrentUserId { get; set; }
        public List<ChatUserViewModel> AllUsers { get; set; } = new();
    }
}