using Mini_ServiceDesk_API.Models;
using Mini_ServiceDesk_API.Models.Entities;
using Mini_ServiceDesk_API.Models.Enum;

namespace Mini_ServiceDesk_API.Data
{
    public interface ITicketRepository
    {
        public Task<Ticket> GetById(Guid id);
        public Task Add(Ticket ticket);

        public Task Remove(Ticket ticket);

        // Persist changes for an updated ticket entity
        public Task<Ticket> Update(Ticket ticket);

        // Query with filtering/sorting/pagination
        public Task<PagedResult<Ticket>> GetAll(int page, int pageSize, TicketStatus? status, TicketPriority? priority, string? assignee, string? sortBy, string? order);
    }
}
