using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Mini_ServiceDesk_API.Data;
using Mini_ServiceDesk_API.Models;
using Mini_ServiceDesk_API.Models.Enum;
using Mini_ServiceDesk_API.Models.Entities;
using Mini_ServiceDesk_API.Services;

namespace Mini_ServiceDesk_API.Controllers
{
    // localhost:xxxx/api/tickets
    [Route("api/[controller]")]
    [ApiController]
    [Mini_ServiceDesk_API.Auth.ApiKeyAuth]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService ticketService;
        private readonly ApplicationDbContext dbContext;
        public TicketsController(ApplicationDbContext dbContext, ITicketService ticketService)
        {
            this.dbContext = dbContext;
            this.ticketService = ticketService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTickets(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] TicketStatus? status = null,
            [FromQuery] TicketPriority? priority = null,
            [FromQuery] string? assignee = null,
            [FromQuery] string? sortBy = "createdAt",
            [FromQuery] string? order = "asc")
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            pageSize = Math.Min(pageSize, 100);
            var result = await ticketService.GetAllTickets(page, pageSize, status, priority, assignee, sortBy, order);
            return Ok(result);
        }

        // Add Ticket
        [HttpPost]
        public async Task<IActionResult> AddTicket(AddTicketDto addTicketDto)
        {
            var ticketEntity = await ticketService.CreateTicket(addTicketDto);
            return Ok(ticketEntity);
        }

        // Get Ticket By ID
        [HttpGet]
        [Route("{id:guid}")]
        public async Task<IActionResult> GetTicketsById(Guid id)
        {

            var ticket = await ticketService.FindTicketById(id);
            if (ticket is null)
            {
                return NotFound();
            }
            return Ok(ticket);
        }

        [HttpPatch]
        [Route("{id:guid}")]
        [Mini_ServiceDesk_API.Auth.ApiKeyAuth(Role = "agent")]
        public async Task<IActionResult> UpdateTicket(Guid id, UpdateTicketDto updateTicketDto)
        {
            var ticket = await ticketService.FindTicketById(id);
            if (ticket is null)
            {
                return NotFound();
            }
            try
            {
                var updatedTicket = await ticketService.UpdateTicket(ticket, updateTicketDto);
                return Ok(updatedTicket);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

      

        [HttpDelete]
        [Route("{id:guid}")]
        [Mini_ServiceDesk_API.Auth.ApiKeyAuth(Role = "agent")]
        public async Task<IActionResult> DeleteTicket(Guid id)
        {
            var ticket = await ticketService.FindTicketById(id);
            if (ticket is null)
            {
                return NotFound();
            }
            await ticketService.RemoveTicket(ticket);
            return Ok();
        }
    }
}
