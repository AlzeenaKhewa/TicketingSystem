using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using TicketingSystem.Data;
using TicketingSystem.Models;

namespace TicketingSystem.Controllers
{
    public class ChatController : Controller
    {
        private readonly IChatRepository _chatRepo;
        private readonly ITicketRepository _ticketRepo;

        public ChatController(IChatRepository chatRepo, ITicketRepository ticketRepo)
        {
            _chatRepo = chatRepo;
            _ticketRepo = ticketRepo;
        }

        // THIS IS THE CORRECTED ROUTE ATTRIBUTE AND METHOD SIGNATURE
        [HttpGet("tickets/{ticketId}/chat")]
        public async Task<IActionResult> Room(int ticketId)
        {
            // In a real app, get this from authentication (e.g., User.Claims)
            var currentUserId = 1;

            var ticket = await _ticketRepo.GetByIdAsync(ticketId);
            if (ticket == null) return NotFound();

            var messages = await _chatRepo.GetConversationAsync(ticketId, currentUserId);
            var users = await _chatRepo.GetUsersForTicketAsync(ticketId);

            var viewModel = new ChatRoomViewModel
            {
                TicketId = ticketId,
                TicketTitle = ticket.Title,
                InitialMessages = messages.ToList(),
                CurrentUserId = currentUserId,
                AllUsers = users.ToList()
            };

            return View(viewModel);
        }
    }
}