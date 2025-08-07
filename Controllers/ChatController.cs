using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using TicketingSystem.Data;
using TicketingSystem.Helpers;
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

        // --- THE FIX ---
        // The [HttpGet] attribute has been completely REMOVED.
        // This allows the default convention-based routing to find this action.
        public async Task<IActionResult> Room(int id) // The parameter name "id" must match the default route pattern.
        {
            // In a real app, get this from authentication (e.g., User.Claims)
            var currentUserId = 2;

            // The ticketId is passed in the 'id' parameter from the URL
            var ticketId = id;

            var ticket = await _ticketRepo.GetByIdAsync(ticketId);
            if (ticket == null) return NotFound();

            var messages = await _chatRepo.GetConversationAsync(ticketId, currentUserId);
            var users = await _chatRepo.GetUsersForTicketAsync(ticketId);

            var userViewModels = users.Select(u => new ChatUserViewModel
            {
                UserId = u.UserId,
                Name = u.Name,
                Role = u.Role,
                IsOnline = u.IsOnline,
                Initials = AvatarHelper.GetInitials(u.Name),
                AvatarBackgroundColor = AvatarHelper.GetBackgroundColor(u.Name)
            }).ToDictionary(u => u.UserId);

            var viewModel = new ChatRoomViewModel
            {
                TicketId = ticketId,
                TicketTitle = ticket.Title,
                InitialMessages = messages.ToList(),
                CurrentUserId = currentUserId,
                AllUsers = userViewModels
            };

            return View(viewModel);
        }
    }
}