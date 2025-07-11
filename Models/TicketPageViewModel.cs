// TicketingSystem/Models/TicketPageViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace TicketingSystem.Models
{
    public class TicketPageViewModel
    {
        public List<Ticket> ExistingTickets { get; set; } = new List<Ticket>();
        public Ticket NewTicket { get; set; } = new Ticket();

        // --- ADD THIS NEW PROPERTY ---
        /// <summary>
        /// Holds the list of options for the Topic dropdown in the view.
        /// </summary>
        public List<SelectListItem> TopicOptions { get; set; } = new List<SelectListItem>();
    }
}