using GenesisBugTracker.Data;
using GenesisBugTracker.Models;
using GenesisBugTracker.Models.Enums;
using GenesisBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GenesisBugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;

        public BTProjectService(ApplicationDbContext context, 
                                IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
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

        #region Add Project Manager Async
        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            BTUser currentPM = await GetProjectManagerAsync(projectId);

            if (currentPM != null)
            {
                try
                {
                    await RemoveProjectManagerAsync(projectId);
                }
                catch (Exception)
                {

                    throw;
                }
            }
            try
            {
                return await AddUserToProjectAsync(userId, projectId);
            }
            catch (Exception)
            {

                throw;
            }
            
        }
        #endregion

        #region Add User To Project Async
        public async Task<bool> AddUserToProjectAsync(string userId, int projectId)
        {
            BTUser? bTUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (bTUser != null)
            {
                Project? project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
                if (!await IsUserOnProjectAsync(userId, projectId))
                {
                    try
                    {
                        project!.Members!.Add(bTUser);
                        await _context.SaveChangesAsync();
                        return true;
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Archive Project Async
        public async Task ArchiveProjectAsync(Project project)
        {
            try
            {
                project.Archived = true;
                //_context.Update(project);
                //await _context.SaveChangesAsync();
                await UpdateProjectAsync(project);

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

        #region Get Project By Id Async
        public async Task<Project> GetProjectByIdAsync(int projectId, int companyId)
        {
            try
            {
                Project project = await _context.Projects.Where(p => p.Id == projectId && p.CompanyId == companyId)
                                                         .Include(p => p.Company)
                                                         .Include(p => p.ProjectPriority)
                                                         .Include(p => p.Members)
                                                         .Include(p => p.Tickets)
                                                            .ThenInclude(t => t.TicketPriority)
                                                         .Include(p => p.Tickets)
                                                            .ThenInclude(t => t.TicketStatus)
                                                         .Include(p => p.Tickets)
                                                            .ThenInclude(t => t.TicketType)
                                                         .Include(p => p.Tickets)
                                                            .ThenInclude(t => t.DeveloperUser)
                                                         .Include(p => p.Tickets)
                                                            .ThenInclude(t => t.SubmitterUser)
                                                         .FirstAsync();
                return project;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get Project Manager Async
        public async Task<BTUser> GetProjectManagerAsync(int projectId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members)
                                                    .FirstOrDefaultAsync(p => p.Id == projectId);
                foreach (BTUser member in project?.Members!)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        return member;
                    }
                }

                return null!;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Is User On Project Async
        public async Task<bool> IsUserOnProjectAsync(string userId, int projectId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members)
                                                          .FirstOrDefaultAsync(p => p.Id == projectId);
                
                bool result = false;
                
                if (project != null)
                {
                    result = project.Members.Any(m => m.Id == userId);
                }

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members)
                                                          .FirstOrDefaultAsync(p => p.Id == projectId);
                foreach (BTUser member in project?.Members!)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        await RemoveUserFromProjectAsync(member.Id, projectId);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Remove User From Project Async
        public async Task<bool> RemoveUserFromProjectAsync(string userId, int projectId)
        {
            try
            {
                BTUser? bTUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                Project? project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

                if (await IsUserOnProjectAsync(userId, projectId))
                {
                    project?.Members.Remove(bTUser!);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
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
    }
}
