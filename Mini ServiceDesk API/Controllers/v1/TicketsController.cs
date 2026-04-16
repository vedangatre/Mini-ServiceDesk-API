using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Mini_ServiceDesk_API.Data;
using Mini_ServiceDesk_API.Models;
using Mini_ServiceDesk_API.Models.Enum;
using Mini_ServiceDesk_API.Services;

namespace Mini_ServiceDesk_API.Controllers.v1
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Auth.ApiKeyAuth]
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

        [HttpPost]
        public async Task<IActionResult> AddTicket([FromBody] AddTicketDto addTicketDto)
        {
            var ticketEntity = await ticketService.CreateTicket(addTicketDto);
            return CreatedAtAction(nameof(GetTicketsById), new { id = ticketEntity.Id }, ticketEntity);
        }


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
        [Auth.ApiKeyAuth(Role = "agent")]
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
        [Auth.ApiKeyAuth(Role = "agent")]
        public async Task<IActionResult> DeleteTicket(Guid id)
        {
            var ticket = await ticketService.FindTicketById(id);
            if (ticket is null)
            {
                return NotFound();
            }
            await ticketService.RemoveTicket(ticket);
            return NoContent();
        }
    }
}
