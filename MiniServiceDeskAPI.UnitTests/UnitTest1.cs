using Mini_ServiceDesk_API.Data;
using Mini_ServiceDesk_API.Models;
using Mini_ServiceDesk_API.Models.Entities;
using Mini_ServiceDesk_API.Models.Enum;
using Mini_ServiceDesk_API.Services;
using Moq;
namespace MiniServiceDeskAPI.UnitTests
{
    public class Tests
    {
        private ITicketService _ticketService;
        private Mock<ITicketRepository> _ticketRepositoryMock;

        [SetUp]
        public void Setup()
        {
            _ticketRepositoryMock = new Mock<ITicketRepository>();
            _ticketService = new TicketService(_ticketRepositoryMock.Object);
        }

        [Test]
        public void CreateTicket_WhenPassedTicketDetails_VerifiesAddTicketOnce()
        {

            var ticketDto = new AddTicketDto
            {
                Assignee = "John Doe",
                Title = "Test Ticket",
                Description = "This is a test ticket.",
                Status = TicketStatus.OPEN,
                Priority = TicketPriority.MEDIUM
            };

            _ticketService.CreateTicket(ticketDto).Wait();

            _ticketRepositoryMock.Verify(
                r => r.Add(
                    It.Is<Ticket>(ticket => 
                    ticket.Title.Contains(ticketDto.Title) &&
                    ticket.Description.Contains(ticketDto.Description) &&
                    ticket.Status == ticketDto.Status &&
                    ticket.Priority == ticketDto.Priority &&
                    ticket.Assignee.Contains(ticketDto.Assignee)
                    )), Times.Once);
        }

        [Test]
        public void FindTicketById_WhenPassedId_VerifiesGetByIdIsCalledOnce()
        {
            var ticketId = Guid.NewGuid();

            _ticketService.FindTicketById(ticketId).Wait();

            _ticketRepositoryMock.Verify(
                r => r.GetById(It.Is<Guid>(id => id == ticketId)), Times.Once);
        }

        [Test]
        public void RemoveTicket_WhenPassedTicket_VerifiesRemoveIsCalledOnce()
        { 
            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                Assignee = "John Doe",
                Title = "Test Ticket",
                Description = "This is a test ticket.",
                Status = TicketStatus.OPEN,
                Priority = TicketPriority.MEDIUM
            };

            _ticketService.RemoveTicket(ticket).Wait();

            _ticketRepositoryMock.Verify(
                r => r.Remove(
                    It.Is<Ticket>(t => t == ticket)), 
                    Times.Once);
        }

        [Test]
        public void UpdateTicket_WhenPassedTicketAndUpdateDto_VerifiesUpdateIsCalledOnce()
        {

            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                Assignee = "John Doe",
                Title = "Test Ticket",
                Description = "This is a test ticket.",
                Status = TicketStatus.OPEN,
                Priority = TicketPriority.MEDIUM
            };

            var updateticketDto = new UpdateTicketDto
            {
                Assignee = "John Doe",
                Title = "Test Ticket",
                Status = TicketStatus.OPEN,
                Priority = TicketPriority.MEDIUM
            };

            _ticketService.UpdateTicket(ticket, updateticketDto).Wait();

            _ticketRepositoryMock.Verify(
                r => r.Update(It.Is<Ticket>(ticket =>
                ticket.Title.Contains(updateticketDto.Title) &&
                ticket.Status == updateticketDto.Status &&
                ticket.Priority == updateticketDto.Priority &&
                ticket.Assignee.Contains(updateticketDto.Assignee)
                )), Times.Once);
        }

        [Test]
        public void GetAllTickets_WhenCalled_VerifiesGetAllIsCalledOnce()
        {
            int page = 1;
            int pageSize = 10;
            TicketStatus Status = TicketStatus.OPEN;
            TicketPriority Priority = TicketPriority.MEDIUM;
            string Assignee = "John Doe";
            string sortBy = "createdAt";
            string order = "asc";

            _ticketService.GetAllTickets(page, pageSize, Status, Priority, Assignee, sortBy, order).Wait();

            _ticketRepositoryMock.Verify(
                r => r.GetAll(
                It.Is<int>(p => p == page),
                It.Is<int>(ps => ps == pageSize),
                It.Is<TicketStatus>(s => s == Status),
                It.Is<TicketPriority>(p => p == Priority),
                It.Is<string>(a => a == Assignee),
                It.Is<string>(s => s == sortBy),
                It.Is<string>(o => o == order)), Times.Once);
        }
    }
}
