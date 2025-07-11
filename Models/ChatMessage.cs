// TicketingSystem/Models/ChatMessage.cs
using Dapper.Contrib.Extensions; // Dapper mapping if you use it

// Map snake_case from DB to PascalCase in C#
[Table("chat_messages")]
public class ChatMessage
{
    [Key]
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsAdminMessage { get; set; }
    public DateTime SentAt { get; set; }
}