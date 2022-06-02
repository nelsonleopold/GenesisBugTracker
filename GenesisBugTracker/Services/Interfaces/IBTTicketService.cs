using GenesisBugTracker.Models;

namespace GenesisBugTracker.Services.Interfaces
{
    public interface IBTTicketService
    {
        public Task AddNewTicketAsync(Ticket ticket);
        public Task ArchiveTicketAsync(Ticket ticket);
        public Task<List<Ticket>> GetAllArchivedTicketsAsync(int companyId);
        public Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int companyId);
        public Task<Ticket> GetTicketByIdAsync(int ticketId);
        public Task RestoreTicketAsync(Ticket ticket);
        public Task UpdateTicketAsync(Ticket ticket);
        
    }
}
