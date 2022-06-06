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
                await UpdateProjectAsync(project);

                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = true;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
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

        #region Get All Project Members Except PM Async
        public async Task<List<BTUser>> GetAllProjectMembersExceptPMAsync(int projectId)
        {
            try
            {
                List<BTUser> developers = await GetAllProjectMembersByRoleAsync(projectId, nameof(BTRoles.Developer));
                List<BTUser> submitters = await GetAllProjectMembersByRoleAsync(projectId, nameof(BTRoles.Submitter));
                List<BTUser> admins = await GetAllProjectMembersByRoleAsync(projectId, nameof(BTRoles.Admin));

                List<BTUser> teamMembers = developers.Concat(submitters).Concat(admins).ToList();

                return teamMembers;

            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get All Archived Projects Async
        public async Task<List<Project>> GetAllArchivedProjectsAsync(int companyId)
        {
            try
            {
                List<Project> archivedProjects = new();

                archivedProjects = await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == true)
                                                          .ToListAsync();
                return archivedProjects;
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

        #region Get All Project Members By Role Async
        public async Task<List<BTUser>> GetAllProjectMembersByRoleAsync(int projectId, string roleName)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members)
                                                    .FirstOrDefaultAsync(p => p.Id == projectId);

                List<BTUser> members = new();

                foreach (BTUser user in project?.Members!)
                {
                    if (await _rolesService.IsUserInRoleAsync(user, roleName))
                    {
                        members.Add(user);
                    }
                }
                return members;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get Unassigned Projects Async
        public async Task<List<Project>> GetUnassignedProjectsAsync(int companyId)
        {
            List<Project> result = new();
            List<Project> projects = new();
            try
            {
                projects = await _context.Projects
                                         .Include(p => p.ProjectPriority)
                                         .Where(p => p.CompanyId == companyId).ToListAsync();
                foreach (Project project in projects)
                {
                    if ((await GetAllProjectMembersByRoleAsync(project.Id, nameof(BTRoles.ProjectManager))).Count == 0)
                    {
                        result.Add(project);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }
        #endregion

        #region Get User Projects Async
        public async Task<List<Project>> GetUserProjectsAsync(string userId)
        {
            try
            {
                List<Project>? projects = (await _context.Users.Include(u => u.Projects)
                                                                    .ThenInclude(p => p.Company)
                                                               .Include(u => u.Projects)
                                                                    .ThenInclude(p => p.Members)
                                                               .Include(u => u.Projects)
                                                                    .ThenInclude(p => p.Tickets)
                                                               .Include(u => u.Projects)
                                                                    .ThenInclude(p => p.Tickets)
                                                                        .ThenInclude(t => t.DeveloperUser)
                                                               .Include(u => u.Projects)
                                                                    .ThenInclude(p => p.Tickets)
                                                                        .ThenInclude(t => t.SubmitterUser)
                                                               .Include(u => u.Projects)
                                                                    .ThenInclude(p => p.Tickets)
                                                                        .ThenInclude(t => t.TicketPriority)
                                                               .Include(u => u.Projects)
                                                                    .ThenInclude(p => p.Tickets)
                                                                        .ThenInclude(t => t.TicketStatus)
                                                               .Include(u => u.Projects)
                                                                    .ThenInclude(p => p.Tickets)
                                                                        .ThenInclude(t => t.TicketType)
                                                               .FirstOrDefaultAsync(u => u.Id == userId))?
                                                               .Projects!.ToList();
                return projects!;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get Users Not On Project Async
        public async Task<List<BTUser>> GetUsersNotOnProjectAsync(int projectId, int companyId)
        {
            try
            {
                List<BTUser> users = await _context.Users.Where(u => u.Projects!.All(p => p.Id != projectId) && u.CompanyId == companyId).ToListAsync();
                return users;
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
        #endregion

        #region Remove Project Manager Async
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

        #region Restore Project Async
        public async Task RestoreProjectAsync(Project project)
        {
            try
            {
                project.Archived = false;
                await UpdateProjectAsync(project);
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
