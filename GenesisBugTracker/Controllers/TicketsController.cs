using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GenesisBugTracker.Data;
using GenesisBugTracker.Models;
using GenesisBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using GenesisBugTracker.Extensions;
using GenesisBugTracker.Models.Enums;
using GenesisBugTracker.Models.ViewModels;

namespace GenesisBugTracker.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;
        private readonly IBTProjectService _projectService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTFileService _fileService;
        private readonly IBTTicketHistoryService _ticketHistoryService;
        private readonly IBTNotificationService _notificationService;
        private readonly IBTLookupService _lookupService;

        public TicketsController(ApplicationDbContext context,
                                 IBTTicketService ticketService,
                                 UserManager<BTUser> userManager,
                                 IBTProjectService projectService,
                                 IBTRolesService rolesService,
                                 IBTTicketHistoryService ticketHistoryService,
                                 IBTNotificationService notificationService,
                                 IBTLookupService lookupService,
                                 IBTFileService fileService)
        {
            _context = context;
            _ticketService = ticketService;
            _userManager = userManager;
            _projectService = projectService;
            _rolesService = rolesService;
            _ticketHistoryService = ticketHistoryService;
            _notificationService = notificationService;
            _lookupService = lookupService;
            _fileService = fileService;
        }

        // GET: Tickets
        [Authorize]
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyIdAsync(companyId);


            return View(tickets);
        }

        // GET: Tickets/AllTickets
        [Authorize]
        public async Task<IActionResult> AllTickets()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyIdAsync(companyId);

            return View(tickets);
        }

        // GET: Tickets/MyTickets
        [Authorize]
        public async Task<IActionResult> MyTickets()
        {
            int companyId = User.Identity!.GetCompanyId();
            string userId = _userManager.GetUserId(User);

            List<Ticket> tickets = await _ticketService.GetTicketsByUserIdAsync(userId, companyId);

            return View(tickets);
        }

        // GET: Tickets/ArchivedTickets
        [Authorize]
        public async Task<IActionResult> ArchivedTickets()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Ticket> tickets = await _ticketService.GetAllArchivedTicketsAsync(companyId);


            return View(tickets);
        }

        // GET: Tickets/UnassignedTickets
        public async Task<IActionResult> UnassignedTickets()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Ticket> tickets = await _ticketService.GetUnassignedTicketsAsync(companyId);

            return View(tickets);
        }

        // GET: Tickets/AssignDevelopers
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AssignDeveloper(int? id)
        {
            try
            {
                if (id == null || _context.Tickets == null)
                {
                    return NotFound();
                }

                AssignDeveloperToTicketViewModel model = new();
                int companyId = User.Identity!.GetCompanyId();
                Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

                model.Ticket = ticket;

                model.DevList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId), "Id", "FullName");
                return View(model);
            }
            catch (Exception)
            {

                throw;
            }
        }

        // POST: Tickets/AssignDeveloper
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDeveloper(AssignDeveloperToTicketViewModel model)
        {
            if (model.Ticket == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                BTUser btUser = await _userManager.GetUserAsync(User);

                Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(model.Ticket!.Id);
                try
                {
                    model.Ticket.Created = DateTime.SpecifyKind(model.Ticket.Created, DateTimeKind.Utc);
                    model.Ticket.Updated = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

                    if (!string.IsNullOrEmpty(model.DevId))
                    {
                        model.Ticket.DeveloperUserId = model.DevId;
                        await _ticketService.UpdateTicketAsync(model.Ticket);
                    }
                }
                catch (Exception)
                {

                    throw;
                }

                //newTicket
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(model.Ticket.Id);
                // Add History
                await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, btUser.Id);
                //Send Notifications
                //Notify Developer
                if (model.Ticket.DeveloperUserId != null)
                {
                    Notification devNotification = new()
                    {
                        TicketId = model.Ticket.Id,
                        NotificationTypeId = (await _lookupService.LookupNotificationTypeIdAsync(nameof(BTNotificationTypes.Ticket))).Value,
                        Title = "Ticket Updated",
                        Message = $"Ticket: {model.Ticket.Title}, was updated by {btUser.FullName}",
                        Created = DateTime.UtcNow,
                        SenderId = btUser.Id,
                        RecipientId = model.Ticket.DeveloperUserId
                    };
                    await _notificationService.AddNotificationAsync(devNotification);
                    await _notificationService.SendEmailNotificationAsync(devNotification, "Ticket Updated");

                    return RedirectToAction("Details", "Tickets", new { model.Ticket.Id });
                }
            }

            int companyId = User.Identity!.GetCompanyId();
            Ticket ticket = await _ticketService.GetTicketByIdAsync(model.Ticket.Id);

            model.Ticket = ticket;

            model.DevList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId), "Id", "FullName");
            return View(model);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            AddTicketAttachmentViewModel model = new();
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);
            model.Ticket = ticket;
            model.UserId = _userManager.GetUserId(User);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(model);
        }

        //POST: Tickets/AddTicketAttachment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment(AddTicketAttachmentViewModel model)
        {
            string statusMessage;

            if (model.FormFile == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                TicketAttachment ticketAttachment = new();
                model.TicketAttachment = ticketAttachment;
                model.TicketAttachment.TicketId = model.TicketId;
                
                model.TicketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(model.FormFile);
                model.TicketAttachment.FileName = model.FormFile.FileName;
                model.TicketAttachment.FileContentType = model.FormFile.ContentType;

                model.TicketAttachment.Created = DateTime.UtcNow;
                model.TicketAttachment.UserId = _userManager.GetUserId(User);

                await _ticketService.AddTicketAttachmentAsync(model.TicketAttachment!);
                statusMessage = "Success: New attachment added to Ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = model.TicketAttachment!.TicketId, message = statusMessage });
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity!.GetCompanyId();
            string userId = _userManager.GetUserId(User);

            if (User.IsInRole(nameof(BTRoles.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyIdAsync(companyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(userId), "Id", "Name");
            }

            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketPriorityId,TicketTypeId")] Ticket ticket)
        {
            ModelState.Remove("SubmitterUserId");
            int companyId = User.Identity!.GetCompanyId();
            string userId = _userManager.GetUserId(User);
            if (ModelState.IsValid)
            {
                BTUser btUser = await _userManager.GetUserAsync(User);
                ticket.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                ticket.SubmitterUserId = _userManager.GetUserId(User);
                ticket.TicketStatusId = (await _context.TicketStatuses.FirstOrDefaultAsync(t => t.Name == "New"))!.Id;

                await _ticketService.AddNewTicketAsync(ticket);

                // TODO: Add Ticket History
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                await _ticketHistoryService.AddHistoryAsync(null!, newTicket, ticket.SubmitterUserId);

                // TODO: Add Ticket Notification
                BTUser projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);
                Notification notification = new()
                {
                    NotificationTypeId = (await _lookupService.LookupNotificationTypeIdAsync(nameof(BTNotificationTypes.Ticket))).Value,
                    TicketId = ticket.Id,
                    Title = "New Ticket Added",
                    Message = $"New Ticket: {ticket.Title}, was created by {btUser.FullName}",
                    Created = DateTime.UtcNow,
                    SenderId = btUser.Id,
                    RecipientId = projectManager?.Id
                };

                await _notificationService.AddNotificationAsync(notification);
                if (projectManager != null)
                {
                    await _notificationService.SendEmailNotificationAsync(notification, $"New Ticket Added For Project: {newTicket.Project!.Name}");
                }
                else
                {
                    await _notificationService.SendEmailNotificationsByRoleAsync(notification, companyId, nameof(BTRoles.Admin));
                }

                await _ticketService.AddNewTicketAsync(ticket);
                return RedirectToAction(nameof(Index));
            }



            if (User.IsInRole(nameof(BTRoles.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyIdAsync(companyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(userId), "Id", "Name");
            }

            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            //var ticket = await _context.Tickets.FindAsync(id);
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }
            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            //ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketPriorityId,TicketStatusId,TicketTypeId,SubmitterUserId,DeveloperUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                string userId = _userManager.GetUserId(User);
                Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);

                try
                {
                    ticket.Created = DateTime.SpecifyKind(ticket.Created, DateTimeKind.Utc);
                    ticket.Updated = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

                    // Use custom service methods
                    await _ticketService.UpdateTicketAsync(ticket);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // TODO: Add History
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, userId);

                return RedirectToAction(nameof(Index));
            }

            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);

            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Archive/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }
            Ticket ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                await _ticketService.ArchiveTicketAsync(ticket);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Tickets/Restore/5
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreTicket(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }

            Ticket? ticket = await _context.Tickets.FindAsync(id);

            if (ticket != null)
            {
                await _ticketService.RestoreTicketAsync(ticket);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
