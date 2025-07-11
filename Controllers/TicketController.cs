// TicketingSystem/Controllers/TicketsController.cs
using Microsoft.AspNetCore.Mvc;
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
            // In a real app, get this from user authentication
            var customerId = 1;

            // Pass the success message from TempData to the ViewBag
            // ViewBag is used here because the message's lifetime is just for this one render.
            ViewBag.SuccessMessage = TempData["SuccessMessage"];

            var viewModel = new TicketPageViewModel
            {
                ExistingTickets = (await _ticketRepository.GetTicketsByCustomerIdAsync(customerId)).ToList(),
                NewTicket = new Ticket()
            };

            return View(viewModel);
        }

        // In TicketsController.cs, update the Create action again to use the new return value

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketPageViewModel model)
        {
            if (ModelState.IsValid)
            {
                var customerId = 1;

                var ticketToCreate = model.NewTicket;
                ticketToCreate.CustomerId = customerId;
                ticketToCreate.Status = "Open";
                ticketToCreate.CreatedAt = DateTime.UtcNow;

                // --- CAPTURE THE NEW ID ---
                var newTicketId = await _ticketRepository.CreateTicketAsync(ticketToCreate);

                // --- USE THE ID IN THE MESSAGE ---
                TempData["SuccessMessage"] = $"Your request (ID: TCK-{newTicketId:D6}) has been submitted successfully!";

                return RedirectToAction(nameof(Index));
            }

            // ... (rest of the method is the same)
            var customerIdForReload = 1;
            model.ExistingTickets = (await _ticketRepository.GetTicketsByCustomerIdAsync(customerIdForReload)).ToList();
            return View("Index", model);
        }
    }
}
