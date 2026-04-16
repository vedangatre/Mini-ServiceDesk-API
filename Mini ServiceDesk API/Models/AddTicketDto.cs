using System.ComponentModel.DataAnnotations;
using Mini_ServiceDesk_API.Models.Enum;

namespace Mini_ServiceDesk_API.Models
{
    public class AddTicketDto
    {
        [Required]
        public required string Title { get; set; }

        public string? Description { get; set; }

        [EnumDataType(typeof(TicketStatus))]
        public TicketStatus Status { get; set; }

        [EnumDataType(typeof(TicketPriority))]
        public TicketPriority Priority { get; set; }
        public string? Assignee { get; set; }
    }
}
