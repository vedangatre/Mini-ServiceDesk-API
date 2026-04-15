using Microsoft.EntityFrameworkCore;
using Mini_ServiceDesk_API.Models;
using Mini_ServiceDesk_API.Models.Entities;

namespace Mini_ServiceDesk_API.Data
{
    public class TicketRepository : ITicketRepository
    {
        private readonly ApplicationDbContext dbContext;
        public TicketRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Add(Ticket ticketEntity)
        {
            dbContext.Tickets.Add(ticketEntity);
            await dbContext.SaveChangesAsync();
        }

        public async Task<Ticket> GetById(Guid id)
        {
            return await dbContext.Tickets.FindAsync(id);
        }

        public async Task Remove(Ticket ticket)
        {
            dbContext.Tickets.Remove(ticket);
            await dbContext.SaveChangesAsync();
        }

        public async Task<Ticket> Update(Ticket ticket)
        {
            // dbContext is already tracking the ticket instance passed from service/controller
            await dbContext.SaveChangesAsync();
            return ticket;
        }

        public async Task<PagedResult<Ticket>> GetAll(int page, int pageSize, Mini_ServiceDesk_API.Models.Enum.TicketStatus? status, Mini_ServiceDesk_API.Models.Enum.TicketPriority? priority, string? assignee, string? sortBy, string? order)
        {
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
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<Ticket>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items
            };
        }
    }
}
