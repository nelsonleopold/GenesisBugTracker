using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GenesisBugTracker.Data;
using GenesisBugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using GenesisBugTracker.Services.Interfaces;
using GenesisBugTracker.Extensions;
using GenesisBugTracker.Models.ViewModels;
using GenesisBugTracker.Models.Enums;

namespace GenesisBugTracker.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTFileService _fileService;

        public ProjectsController(ApplicationDbContext context,
                                  UserManager<BTUser> userManager,
                                  IBTProjectService projectService,
                                  IBTRolesService rolesService,
                                  IBTFileService fileService)
        {
            _context = context;
            _userManager = userManager;
            _projectService = projectService;
            _rolesService = rolesService;
            _fileService = fileService;
        }

        //// GET: Projects
        //[Authorize]
        //public async Task<IActionResult> Index()
        //{
        //    int companyId = User.Identity!.GetCompanyId();

        //    List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

        //    return View(projects);
        //}

        // GET: Projects/All
        public async Task<IActionResult> AllProjects()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

            return View(projects);
        }

        // GET: Projects/My
        public async Task<IActionResult> MyProjects()
        {
            string userId = _userManager.GetUserId(User);

            List<Project> projects = await _projectService.GetUserProjectsAsync(userId);

            return View(projects);
        }

        public async Task<IActionResult> UnassignedProjects()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Project> unassignedProjects = await _projectService.GetUnassignedProjectsAsync(companyId);

            return View(unassignedProjects);
        }

        // GET: Projects/AssignProjectMembers
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AssignProjectMembers(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                ProjectMembersViewModel model = new();
                int companyId = User.Identity!.GetCompanyId();
                model.Project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
                List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);
                List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId);

                List<BTUser> teamMembers = developers.Concat(submitters).ToList();
                List<string> projectMembers = model.Project.Members.Select(m => m.Id).ToList();

                model.UsersList = new MultiSelectList(teamMembers, "Id", "FullName", projectMembers);

                return View(model);
            }
            catch (Exception)
            {

                throw;
            }
        }

        // POST: Projects/AssignProjectMembers
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignProjectMembers(ProjectMembersViewModel model)
        {
            try
            {
                if (model.SelectedUsers != null)
                {
                    List<string> memberIds = (await _projectService.GetAllProjectMembersExceptPMAsync(model.Project!.Id)).Select(m => m.Id).ToList();

                    // Remove current members
                    foreach (string member in memberIds)
                    {
                        await _projectService.RemoveUserFromProjectAsync(member, model.Project.Id);
                    }
                    // Add selected members
                    foreach (string member in model.SelectedUsers)
                    {
                        await _projectService.AddUserToProjectAsync(member, model.Project.Id);
                    }

                    return RedirectToAction(nameof(Details), new { id = model.Project!.Id });
                }

                return RedirectToAction(nameof(AssignProjectMembers), new { id = model.Project!.Id });
            }
            catch (Exception)
            {

                throw;
            }
        }

        // GET: Projects/ArchivedProjects
        [Authorize]
        public async Task<IActionResult> ArchivedProjects()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = await _projectService.GetAllArchivedProjectsAsync(companyId);

            return View(projects);
        }

        // GET: Projects/AssignProjectManager
        [Authorize]
        public async Task<IActionResult> AssignProjectManager(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }
            AssignProjectManagerToProjectViewModel model = new();
            int companyId = User.Identity!.GetCompanyId();
            Project project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            model.Project = project;

            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName");
            return View(model);
        }

        // POST: Projects/AssignProjectManager
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignProjectManager(AssignProjectManagerToProjectViewModel model)
        {
            if (model.Project == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    model.Project.StartDate = DateTime.SpecifyKind(model.Project.StartDate, DateTimeKind.Utc);
                    model.Project.EndDate = DateTime.SpecifyKind(model.Project.EndDate, DateTimeKind.Utc);

                    // Use custom service methods
                    await _projectService.UpdateProjectAsync(model.Project);

                    // Allow Admin to edit ProjectManager
                    if (!string.IsNullOrEmpty(model.PMId))
                    {
                        await _projectService.AddProjectManagerAsync(model.PMId, model.Project.Id);
                    }


                }
                catch (Exception)
                {

                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            int companyId = User.Identity!.GetCompanyId();
            Project project = await _projectService.GetProjectByIdAsync(model.Project.Id, companyId);

            model.Project = project;

            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName");
            return View(model);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }
            int companyId = User.Identity!.GetCompanyId();

            Project project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        public async Task<IActionResult> Create()
        {
            AddProjectWithPMViewModel model = new();
            int companyId = User.Identity!.GetCompanyId();

            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName");
            model.PriorityList = new SelectList(_context.ProjectPriorities, "Id", "Name");
            return View(model);
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddProjectWithPMViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Image code
                if (model.Project?.ImageFormFile != null)
                {
                    model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                    model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                    model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;
                }

                // Use custom service methods
                model.Project!.CompanyId = User.Identity!.GetCompanyId();
                model.Project.Created = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

                model.Project.StartDate = DateTime.SpecifyKind(model.Project.StartDate, DateTimeKind.Utc);
                model.Project.EndDate = DateTime.SpecifyKind(model.Project.EndDate, DateTimeKind.Utc);

                await _projectService.AddNewProjectAsync(model.Project);

                // TODO: Allow Admin to add ProjectManager
                if (!string.IsNullOrEmpty(model.PMId))
                {
                    await _projectService.AddProjectManagerAsync(model.PMId, model.Project.Id);
                }
                return RedirectToAction(nameof(Index));
            }
            int companyId = User.Identity!.GetCompanyId();

            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName");
            model.PriorityList = new SelectList(_context.ProjectPriorities, "Id", "Name");
            return View(model.Project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }
            AddProjectWithPMViewModel model = new();
            int companyId = User.Identity!.GetCompanyId();
            Project project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            model.Project = project;

            // Get PM if one is assigned
            BTUser projectManager = await _projectService.GetProjectManagerAsync(project.Id);

            if (projectManager != null)
            {
                model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName", projectManager.Id);
            }
            else
            {
                model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName");
            }
            model.PriorityList = new SelectList(_context.ProjectPriorities, "Id", "Name");

            if (project == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AddProjectWithPMViewModel model)
        {
            if (model.Project == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Image code
                    if (model.Project?.ImageFormFile != null)
                    {
                        model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                        model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;
                    }

                    // Date code
                    model.Project!.Created = DateTime.SpecifyKind(model.Project.Created!.Value, DateTimeKind.Utc);
                    model.Project.StartDate = DateTime.SpecifyKind(model.Project.StartDate, DateTimeKind.Utc);
                    model.Project.EndDate = DateTime.SpecifyKind(model.Project.EndDate, DateTimeKind.Utc);

                    // Use custom service methods
                    await _projectService.UpdateProjectAsync(model.Project);

                    // Allow Admin to edit ProjectManager
                    if (!string.IsNullOrEmpty(model.PMId))
                    {
                        await _projectService.AddProjectManagerAsync(model.PMId, model.Project.Id);
                    }

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(model.Project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            int companyId = User.Identity!.GetCompanyId();
            Project project = await _projectService.GetProjectByIdAsync(model.Project.Id, companyId);
            model.Project = project;

            // Get PM if one is assigned
            BTUser projectManager = await _projectService.GetProjectManagerAsync(project.Id);

            if (projectManager == null)
            {
                model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName", projectManager);
            }
            else
            {
                model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName");
            }
            model.PriorityList = new SelectList(_context.ProjectPriorities, "Id", "Name");
            return View(model.Project);
        }

        // GET: Projects/Archive/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }
            int companyId = User.Identity!.GetCompanyId();

            Project project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                await _projectService.ArchiveProjectAsync(project);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Restore/5
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }
            int companyId = User.Identity!.GetCompanyId();

            Project project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Archive/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreProject(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }
            Project? project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                await _projectService.RestoreProjectAsync(project);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
