namespace Mini_ServiceDesk_API.Models
{
    public class ErrorResponse
    {
        public required ErrorInfo Error { get; set; }
    }

    public class ErrorInfo
    {
        public required string Code { get; set; }
        public required string Message { get; set; }
        public object[] Details { get; set; } = [];
        public required string TraceId { get; set; }
    }
}
