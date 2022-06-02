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

        #region Add New Ticket Async
        public async Task AddNewTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Archive Ticket Async
        public async Task ArchiveTicketAsync(Ticket ticket)
        {
            ticket.Archived = true;
            await UpdateTicketAsync(ticket);
        }
        #endregion

        #region Get All Archived Tickets Async
        public async Task<List<Ticket>> GetAllArchivedTicketsAsync(int companyId)
        {
            try
            {
                List<Ticket> archivedTickets = await _context.Projects.Include(p => p.Tickets)
                                                         .Where(p => p.CompanyId == companyId)
                                                         .SelectMany(p => p.Tickets)
                                                            .Where(t => t.Archived == true)
                                                         .ToListAsync();
                return archivedTickets;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get All Tickets By CompanyId Async
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
        #endregion

        #region Get Ticket By Id Async
        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            try
            {
                Ticket ticket = await _context.Tickets.Where(t => t.Id == ticketId)
                                              .Include(t => t.DeveloperUser)
                                              .Include(t => t.Project)
                                              .Include(t => t.SubmitterUser)
                                              .Include(t => t.TicketPriority)
                                              .Include(t => t.TicketStatus)
                                              .Include(t => t.TicketType)
                                              .Include(t => t.Comments)
                                              .Include(t => t.Attachments)
                                              .Include(t => t.Notifications)
                                              .Include(t => t.TicketHistories)
                                              .FirstAsync();
                return ticket;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        public async Task RestoreTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Archived = false;
                await UpdateTicketAsync(ticket);
            }
            catch (Exception)
            {

                throw;
            }
        }

        #region Update Ticket Async
        public async Task UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
      
    }
}
