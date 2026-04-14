using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Mini_ServiceDesk_API.Data;
using Mini_ServiceDesk_API.Models;
using Mini_ServiceDesk_API.Models.Enum;
using Mini_ServiceDesk_API.Models.Entities;

namespace Mini_ServiceDesk_API.Controllers
{
    // localhost:xxxx/api/tickets
    [Route("api/[controller]")]
    [ApiController]
    [Mini_ServiceDesk_API.Auth.ApiKeyAuth]
    public class TicketsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        public TicketsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetAllTickets(
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

            var query = dbContext.Tickets.AsQueryable();

            // Filtering
            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }
            if (priority.HasValue)
            {
                query = query.Where(t => t.Priority == priority.Value);
            }
            if (!string.IsNullOrWhiteSpace(assignee))
            {
                query = query.Where(t => t.Assignee != null && t.Assignee.Contains(assignee));
            }

            // Sorting
            var sort = (sortBy ?? "createdAt").ToLowerInvariant();
            var ord = (order ?? "asc").ToLowerInvariant();
            if (sort == "priority")
            {
                query = ord == "desc" ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority);
            }
            else // createdAt (default)
            {
                query = ord == "desc" ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt);
            }

            // Pagination
            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var result = new
            {
                page,
                pageSize,
                totalCount,
                totalPages,
                items
            };

            return Ok(result);
        }

        // Add Ticket
        [HttpPost]
        public IActionResult AddTicket(Models.AddTicketDto addTicketDto)
        {
            var ticketEntity = new Ticket()
            {
                Title = addTicketDto.Title,
                Description = addTicketDto.Description,
                Status = addTicketDto.Status,
                Priority = addTicketDto.Priority,
                Assignee = addTicketDto.Assignee
            };
            dbContext.Tickets.Add(ticketEntity);
            dbContext.SaveChanges();
            return Ok(ticketEntity);
        }

        // Get Ticket By ID
        [HttpGet]
        [Route("{id:guid}")]
        public IActionResult GetTicketsById(Guid id)
        {
            var ticket = dbContext.Tickets.Find(id);

            if (ticket is null)
            {
                return NotFound();
            }
            return Ok(ticket);
        }

        [HttpPatch]
        [Route("{id:guid}")]
        [Mini_ServiceDesk_API.Auth.ApiKeyAuth(Role = "agent")]
        public IActionResult UpdateTicket(Guid id, UpdateTicketDto updateTicketDto)
        {
            var ticket = dbContext.Tickets.Find(id);

            if (ticket is null)
            {
                return NotFound();
            }

            // Title
            if (!string.IsNullOrWhiteSpace(updateTicketDto.Title))
            {
                ticket.Title = updateTicketDto.Title;
            }

            // Assignee
            if (updateTicketDto.Assignee is not null)
            {
                ticket.Assignee = updateTicketDto.Assignee;
            }

            // Priority
            if (updateTicketDto.Priority.HasValue)
            {
                ticket.Priority = updateTicketDto.Priority.Value;
            }

            // Status with validation
            if (updateTicketDto.Status.HasValue)
            {
                var newStatus = updateTicketDto.Status.Value;
                if (!IsValidStatusTransition(ticket.Status, newStatus))
                {
                    return BadRequest("invalid status transitions");
                }
                ticket.Status = newStatus;
            }

            dbContext.SaveChanges();
            return Ok(ticket);
        }

        private bool IsValidStatusTransition(TicketStatus current, TicketStatus next)
        {
            // allowed transitions:
            // OPEN -> IN_PROGRESS
            // IN_PROGRESS -> RESOLVED
            // RESOLVED -> CLOSED
            // IN_PROGRESS -> CLOSED (allow closing directly from in-progress)
            if (current == next) return true;

            return (current, next) switch
            {
                (TicketStatus.OPEN, TicketStatus.IN_PROGRESS) => true,
                (TicketStatus.IN_PROGRESS, TicketStatus.RESOLVED) => true,
                (TicketStatus.RESOLVED, TicketStatus.CLOSED) => true,
                (TicketStatus.IN_PROGRESS, TicketStatus.CLOSED) => true,
                _ => false
            };
        }

        [HttpDelete]
        [Route("{id:guid}")]
        [Mini_ServiceDesk_API.Auth.ApiKeyAuth(Role = "agent")]
        public IActionResult DeleteTicket(Guid id)
        {
            var ticket = dbContext.Tickets.Find(id);
            if (ticket is null)
            {
                return NotFound();
            }
            dbContext.Tickets.Remove(ticket);
            dbContext.SaveChanges();

            return Ok();
        }
    }
}
