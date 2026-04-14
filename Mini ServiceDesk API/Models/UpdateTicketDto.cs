using Mini_ServiceDesk_API.Models.Enum;

namespace Mini_ServiceDesk_API.Models
{
    public class UpdateTicketDto
    {
        // Partial fields - only set the properties you want to update
        public string? Title { get; set; }

        public TicketStatus? Status { get; set; }

        public TicketPriority? Priority { get; set; }

        public string? Assignee { get; set; }
    }
}
