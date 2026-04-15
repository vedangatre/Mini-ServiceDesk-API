using Microsoft.EntityFrameworkCore;
using Mini_ServiceDesk_API.Data;
using Mini_ServiceDesk_API.Models;
using Mini_ServiceDesk_API.Models.Entities;
using Mini_ServiceDesk_API.Models.Enum;

namespace Mini_ServiceDesk_API.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository ticketRepository;
        private readonly ApplicationDbContext dbContext;

        public TicketService(ApplicationDbContext dbContext, ITicketRepository ticketRepository)
        {
            this.dbContext = dbContext;
            this.ticketRepository = ticketRepository;
        }

        public async Task<Ticket> CreateTicket(AddTicketDto addTicketDto)
        {
            var ticketEntity = new Ticket
            {
                Title = addTicketDto.Title,
                Description = addTicketDto.Description,
                Status = addTicketDto.Status,
                Priority = addTicketDto.Priority,
                Assignee = addTicketDto.Assignee
            };
            await ticketRepository.Add(ticketEntity);
            return ticketEntity; 
        }

        public async Task<Ticket?> FindTicketById(Guid id)
        {
            return await ticketRepository.GetById(id);
        }

        public async Task<PagedResult<Ticket>> GetAllTickets(int page, int pageSize, TicketStatus? status, TicketPriority? priority, string? assignee, string? sortBy, string? order)
        {
            return await ticketRepository.GetAll(page, pageSize, status, priority, assignee, sortBy, order);
        }

        //private IQueryable<Ticket> ApplyFilters(IQueryable<Ticket> query, TicketStatus? status, TicketPriority? priority, string? assignee)
        //{
        //    if (status.HasValue)
        //    {
        //        query = query.Where(t => t.Status == status.Value);
        //    }
        //    if (priority.HasValue)
        //    {
        //        query = query.Where(t => t.Priority == priority.Value);
        //    }
        //    if (!string.IsNullOrWhiteSpace(assignee))
        //    {
        //        query = query.Where(t => t.Assignee != null && t.Assignee.Contains(assignee));
        //    }
        //    return query;
        //}

        //private IQueryable<Ticket> ApplySorting(IQueryable<Ticket> query, string? sortBy, string? order)
        //{
        //    var sort = (sortBy ?? "createdAt").ToLowerInvariant();
        //    var ord = (order ?? "asc").ToLowerInvariant();

        //    if (sort == "priority")
        //    {
        //        return ord == "desc" ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority);
        //    }

        //    // Default: sort by CreatedAt
        //    return ord == "desc" ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt);
        //}

        public async Task RemoveTicket(Ticket ticket)
        {
            await ticketRepository.Remove(ticket);
        }

        public async Task<Ticket> UpdateTicket(Ticket ticket, UpdateTicketDto updateTicketDto)
        {
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
                    throw new InvalidOperationException("invalid status transitions");
                }
                ticket.Status = newStatus;
            }

            return await ticketRepository.Update(ticket);
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
    }
}
