using Mini_ServiceDesk_API.Models;
using Mini_ServiceDesk_API.Models.Entities;
using Mini_ServiceDesk_API.Models.Enum;

namespace Mini_ServiceDesk_API.Services
{
    public interface ITicketService
    {
        public Task<Ticket> CreateTicket(AddTicketDto addTicketDto);
        public Task<Ticket?> FindTicketById(Guid id);
        public Task RemoveTicket(Ticket ticket);
        public Task<Ticket> UpdateTicket(Ticket ticket, UpdateTicketDto updateTicketDto);

        public Task<PagedResult<Ticket>> GetAllTickets(int page, int pageSize, TicketStatus? status, TicketPriority? priority, string? assignee, string? sortBy, string? order);
    }
}
