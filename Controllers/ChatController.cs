// TicketingSystem/Controllers/ChatController.cs
using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Data;

namespace TicketingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;

        public ChatController(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        [HttpGet("{ticketId}")]
        public async Task<IActionResult> GetMessages(int ticketId)
        {
            var messages = await _ticketRepository.GetChatMessagesAsync(ticketId);
            return Ok(messages);
        }
    }
}