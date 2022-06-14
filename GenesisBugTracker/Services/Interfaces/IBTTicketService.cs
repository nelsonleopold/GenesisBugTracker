using GenesisBugTracker.Models;

namespace GenesisBugTracker.Services.Interfaces
{
    public interface IBTTicketService
    {
        public Task AddNewTicketAsync(Ticket ticket);
        public Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment);
        public Task ArchiveTicketAsync(Ticket ticket);
        public Task<List<Ticket>> GetAllArchivedTicketsAsync(int companyId);
        public Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int companyId);
        public Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId);
        public Task<TicketAttachment> GetTicketAttachmentsByIdAsync(int ticketAttachmentId);
        public Task<Ticket> GetTicketByIdAsync(int ticketId);
        public Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId);
        public Task<List<Ticket>> GetUnassignedTicketsAsync(int companyId);
        public Task RestoreTicketAsync(Ticket ticket);
        public Task UpdateTicketAsync(Ticket ticket);
        
    }
}
