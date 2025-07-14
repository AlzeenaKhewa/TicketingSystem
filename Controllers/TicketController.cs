using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // --- Added for SelectListItem
using TicketingSystem.Data;
using TicketingSystem.Models;

namespace TicketingSystem.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketsController(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public async Task<IActionResult> Index()
        {
            // --- OPTIMIZATION START ---
            // In a real app, you would get the customer ID from the authenticated user.
            // For now, we'll keep it, but this is the place to change it.
            // var customerId = GetCurrentUserId(); e.g. User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customerId = 1;
            // --- OPTIMIZATION END ---

            ViewBag.SuccessMessage = TempData["SuccessMessage"];

            var viewModel = new TicketPageViewModel
            {
                ExistingTickets = (await _ticketRepository.GetTicketsByCustomerIdAsync(customerId)).ToList(),
                NewTicket = new Ticket(),
                // --- OPTIMIZATION START ---
                // Populate the dropdown options from the controller, not the view.
                TopicOptions = GetTopicOptions()
                // --- OPTIMIZATION END ---
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketPageViewModel model)
        {
            if (ModelState.IsValid)
            {
                // --- OPTIMIZATION: Get customerId from a secure source, not hardcoded. ---
                var customerId = 1;

                var ticketToCreate = model.NewTicket;
                ticketToCreate.CustomerId = customerId;
                ticketToCreate.Status = "Open"; // Default status
                ticketToCreate.CreatedAt = DateTime.UtcNow;

                var newTicketId = await _ticketRepository.CreateTicketAsync(ticketToCreate);
                TempData["SuccessMessage"] = $"Your request (ID: TCK-{newTicketId:D6}) has been submitted successfully!";
                return RedirectToAction(nameof(Index));
            }

            // --- OPTIMIZATION START ---
            // If the model is invalid, we must reload the necessary data for the view.
            var customerIdForReload = 1;
            model.ExistingTickets = (await _ticketRepository.GetTicketsByCustomerIdAsync(customerIdForReload)).ToList();
            model.TopicOptions = GetTopicOptions(); // Repopulate dropdown options on failure
            return View("Index", model);
            // --- OPTIMIZATION END ---
        }

        // --- OPTIMIZATION START ---
        // Helper method to provide a single source for topic options.
        private List<SelectListItem> GetTopicOptions()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Transaction Inquiry", Text = "Transaction Inquiry" },
                new SelectListItem { Value = "Card Services", Text = "Card Services" },
                new SelectListItem { Value = "Account Security", Text = "Account Security" },
                new SelectListItem { Value = "Payments & Transfers", Text = "Payments & Transfers" },
                new SelectListItem { Value = "Technical Issue", Text = "Technical Issue" },
                new SelectListItem { Value = "General Feedback", Text = "General Feedback" }
            };
        }
        // --- OPTIMIZATION END ---
    }
}