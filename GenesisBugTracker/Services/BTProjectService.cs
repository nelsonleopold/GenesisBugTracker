using GenesisBugTracker.Data;
using GenesisBugTracker.Models;
using GenesisBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GenesisBugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;

        public BTProjectService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Add New Project Async
        public async Task AddNewProjectAsync(Project project)
        {
            try
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Archive Project Async
        public async Task ArchiveProjectAsync(Project project)
        {
            try
            {
                _context.Update(project.Archived == true);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion


        #region Get All Projects By CompanyId Async
        public async Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Project> projects = new();

                projects = await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == false)
                                                    .Include(p => p.Members)
                                                    .Include(p => p.Tickets)
                                                      .ThenInclude(t => t.Comments)
                                                    .Include(p => p.Tickets)
                                                      .ThenInclude(t => t.Attachments)
                                                    .Include(p => p.Tickets)
                                                      .ThenInclude(t => t.TicketHistories)
                                                    .Include(p => p.Tickets)
                                                      .ThenInclude(t => t.Notifications)
                                                    .Include(p => p.Tickets)
                                                      .ThenInclude(t => t.DeveloperUser)
                                                    .Include(p => p.Tickets)
                                                      .ThenInclude(t => t.SubmitterUser)
                                                    .Include(p => p.Tickets)
                                                      .ThenInclude(t => t.TicketStatus)
                                                    .Include(p => p.Tickets)
                                                      .ThenInclude(t => t.TicketPriority)
                                                    .Include(p => p.Tickets)
                                                      .ThenInclude(t => t.TicketType)
                                                    .Include(p => p.ProjectPriority)
                                                    .ToListAsync();

                return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Update Project Async
        public async Task UpdateProjectAsync(Project project)
        {
            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get Project By Id Async
        public async Task<Project> GetProjectByIdAsync(int projectId)
        {
            try
            {
                Project project = await _context.Projects.Where(p => p.Id == projectId)
                                                         .Include(p => p.Company)
                                                         .Include(p => p.ProjectPriority)
                                                         .Include(p => p.Members)
                                                         .Include(p => p.Tickets)
                                                         .FirstAsync();
                return project;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
    }
}
