using Mini_ServiceDesk_API.Models.Enum;

namespace Mini_ServiceDesk_API.Models
{
    public class AddTicketDto
    {
        public required string Title { get; set; }

        public string? Description { get; set; }

        public TicketStatus Status { get; set; }

        public TicketPriority Priority { get; set; }
        public string? Assignee { get; set; }
    }
}
