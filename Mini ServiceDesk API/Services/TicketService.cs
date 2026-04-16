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

        public TicketService(ITicketRepository ticketRepository)
        {
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

        

        public async Task RemoveTicket(Ticket ticket)
        {
            await ticketRepository.Remove(ticket);
        }

        public async Task<Ticket> UpdateTicket(Ticket ticket, UpdateTicketDto updateTicketDto)
        {
            
            if (!string.IsNullOrWhiteSpace(updateTicketDto.Title))
            {
                ticket.Title = updateTicketDto.Title;
            }

            if (updateTicketDto.Assignee is not null)
            {
                ticket.Assignee = updateTicketDto.Assignee;
            }

            if (updateTicketDto.Priority.HasValue)
            {
                ticket.Priority = updateTicketDto.Priority.Value;
            }

            if (updateTicketDto.Status.HasValue)
            {
                var newStatus = updateTicketDto.Status.Value;
                if (!IsValidStatusTransition(ticket.Status, newStatus))
                {
                    throw new InvalidOperationException("Invalid status transitions");
                }
                ticket.Status = newStatus;
            }

            return await ticketRepository.Update(ticket);
        }

        private bool IsValidStatusTransition(TicketStatus current, TicketStatus next)
        {
           
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
