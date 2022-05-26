using GenesisBugTracker.Data;
using GenesisBugTracker.Models;
using GenesisBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GenesisBugTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;

        public BTTicketService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int companyId)
        {
            List<Ticket> tickets = await _context.Projects.Include(p => p.Tickets)
                                                          .Where(p => p.CompanyId == companyId)
                                                          .SelectMany(p => p.Tickets)
                                                            .Include(t => t.Attachments)
                                                            .Include(t => t.Comments)
                                                            .Include(t => t.DeveloperUser)
                                                            .Include(t => t.TicketHistories)
                                                            .Include(t => t.SubmitterUser)
                                                            .Include(t => t.TicketPriority)
                                                            .Include(t => t.TicketStatus)
                                                            .Include(t => t.TicketType)
                                                            .Include(t => t.Project)
                                                          .ToListAsync();
                           
            return tickets;
        }
    }
}
