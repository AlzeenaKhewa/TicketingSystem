using System.Collections.Generic;

namespace TicketingSystem.Models
{
    /// <summary>
    /// Represents a user within the chat UI, including display properties.
    /// </summary>
    public class ChatUserViewModel
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsOnline { get; set; }

        // --- NEW PROPERTIES FOR UI ---
        public string Initials { get; set; } = string.Empty;
        public string AvatarBackgroundColor { get; set; } = "#333";
    }

    /// <summary>
    /// The main view model for the Chat Room page.
    /// </summary>
    public class ChatRoomViewModel
    {
        public int TicketId { get; set; }
        public string TicketTitle { get; set; } = string.Empty;
        public List<ChatMessage> InitialMessages { get; set; } = new();
        public int CurrentUserId { get; set; }

        // A dictionary is more efficient for looking up user details in the view/JS.
        public Dictionary<int, ChatUserViewModel> AllUsers { get; set; } = new();
    }
}