using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Mini_ServiceDesk_API.Auth;

namespace Mini_ServiceDesk_API.Controllers.v2
{
    [Route("api/v{version:apiVersion}/tickets")]
    [ApiController]
    [ApiVersion("2.0")]
    [ApiKeyAuth]
    public class TicketsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAllTickets()
        {
            return Ok(new { message = "Get all tickets v2" });
        }

        [HttpPost]
        public IActionResult AddTicket()
        {
            return Ok(new { message = "Add ticket v2" });
        }

        [HttpGet("{id:guid}")]
        public IActionResult GetTicketsById(Guid id)
        {
            return Ok(new { message = "Get ticket by id v2", id });
        }

        [HttpPatch("{id:guid}")]
        public IActionResult UpdateTicket(Guid id)
        {
            return Ok(new { message = "Update ticket v2", id });
        }

        [HttpDelete("{id:guid}")]
        public IActionResult DeleteTicket(Guid id)
        {
            return Ok(new { message = "Delete ticket v2", id });
        }
    }
}
