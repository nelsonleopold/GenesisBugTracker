using GenesisBugTracker.Data;
using GenesisBugTracker.Models;
using GenesisBugTracker.Models.Enums;
using GenesisBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GenesisBugTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;
        private readonly IBTProjectService _projectService;

        public BTTicketService(ApplicationDbContext context,
                               IBTRolesService rolesService,
                               IBTProjectService projectService)
        {
            _context = context;
            _rolesService = rolesService;
            _projectService = projectService;
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

        #region Add Ticket Attachment Async
        public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
        {
            try
            {
                await _context.AddAsync(ticketAttachment);
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
                                                            .Where(t => t.Archived == false)
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

        #region Get Ticket As No Tracking Async
        public async Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId)
        {
            try
            {
                return await _context.Tickets.Include(t => t.DeveloperUser)
                                             .Include(t => t.Project)
                                             .Include(t => t.TicketPriority)
                                             .Include(t => t.TicketStatus)
                                             .Include(t => t.TicketType)
                                             .AsNoTracking()
                                             .FirstOrDefaultAsync(t => t.Id == ticketId);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        public async Task<TicketAttachment> GetTicketAttachmentsByIdAsync(int ticketAttachmentId)
        {
            try
            {
                TicketAttachment? ticketAttachment = await _context.TicketAttachments.Include(t => t.User)
                                                                                    .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
                return ticketAttachment!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #region Get Archived Ticket By Id Async
        public async Task<Ticket> GetArchivedTicketByIdAsync(int ticketId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets.Where(t => t.Archived == true)
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
                                                       .FirstOrDefaultAsync(t => t.Id == ticketId);
                return ticket;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get Ticket By Id Async
        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets.Include(t => t.DeveloperUser)
                                                       .Include(t => t.Project)
                                                       .Include(t => t.SubmitterUser)
                                                       .Include(t => t.TicketPriority)
                                                       .Include(t => t.TicketStatus)
                                                       .Include(t => t.TicketType)
                                                       .Include(t => t.Comments)
                                                       .Include(t => t.Attachments)
                                                       .Include(t => t.Notifications)
                                                       .Include(t => t.TicketHistories)
                                                       .FirstOrDefaultAsync(t => t.Id == ticketId);
                return ticket;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get Tickets By User Id
        public async Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId)
        {
            BTUser? btUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            List<Ticket>? tickets = new();

            try
            {
                if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.Admin)))
                {
                    tickets = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId))
                                                    .SelectMany(p => p.Tickets!).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.Developer)))
                {
                    tickets = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId))
                                                    .SelectMany(p => p.Tickets!)
                                                    .Where(t => t.DeveloperUserId == userId || t.SubmitterUserId == userId).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.Submitter)))
                {
                    tickets = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId))
                                                    .SelectMany(t => t.Tickets!).Where(t => t.SubmitterUserId == userId).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.ProjectManager)))
                {
                    List<Ticket>? projectTickets = (await _projectService.GetUserProjectsAsync(userId)).SelectMany(t => t.Tickets!).ToList();
                    List<Ticket>? submittedTickets = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId))
                                                    .SelectMany(p => p.Tickets!).Where(t => t.SubmitterUserId == userId).ToList();

                    tickets = projectTickets.Concat(submittedTickets).ToList();
                }

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get Unassigned Tickets Async
        public async Task<List<Ticket>> GetUnassignedTicketsAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = await GetAllTicketsByCompanyIdAsync(companyId);
                List<Ticket> result = new();

                foreach (Ticket ticket in tickets)
                {
                    if (ticket.DeveloperUserId == null)
                    {
                        result.Add(ticket);
                    }
                }

                return result;

            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Restore Ticket Async
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
        #endregion

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
