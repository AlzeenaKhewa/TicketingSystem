// TicketingSystem/Models/Ticket.cs
using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Please enter a subject for your request.")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a topic for your request.")]
        public string Topic { get; set; } = string.Empty;

        [Display(Name = "Account Number (Optional)")]
        [StringLength(50, ErrorMessage = "Account number cannot exceed 50 characters.")]
        public string? AccountNumber { get; set; } // Nullable string

        [Required(ErrorMessage = "Please describe your issue in detail.")]
        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // This property is populated by the repository's JOIN query.
        [Required(ErrorMessage = "Please enter your full name.")]
        [Display(Name = "Your Full Name")]
        [StringLength(150)]
        public string CustomerName { get; set; } = string.Empty;
    }
}