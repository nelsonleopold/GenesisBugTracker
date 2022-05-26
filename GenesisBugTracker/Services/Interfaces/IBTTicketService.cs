using GenesisBugTracker.Models;

namespace GenesisBugTracker.Services.Interfaces
{
    public interface IBTTicketService
    {
        public Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int companyId);
    }
}
