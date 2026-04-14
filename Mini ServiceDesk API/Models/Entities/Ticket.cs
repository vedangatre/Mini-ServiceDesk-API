using Mini_ServiceDesk_API.Models.Enum;

namespace Mini_ServiceDesk_API.Models.Entities
{
    public class Ticket
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }

        public string? Description { get; set; }

        public TicketStatus Status { get; set; }

        public TicketPriority Priority { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string? Assignee { get; set; }
    }
}
