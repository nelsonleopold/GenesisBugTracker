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
using Microsoft.AspNetCore.DataProtection;
using GenesisBugTracker.Extensions;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authorization;

namespace GenesisBugTracker.Controllers
{
    public class InvitesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _projectService;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTInviteService _inviteService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IDataProtector _dataProtector;
        private readonly IEmailSender _emailService;

        public InvitesController(ApplicationDbContext context,
                                 IBTProjectService projectService,
                                 IBTCompanyInfoService companyInfoService,
                                 IBTInviteService inviteService,
                                 UserManager<BTUser> userManager,
                                 IDataProtectionProvider dataProtectionProvider,
                                 IEmailSender emailService)
        {
            _context = context;
            _projectService = projectService;
            _companyInfoService = companyInfoService;
            _inviteService = inviteService;
            _userManager = userManager;
            _dataProtector = dataProtectionProvider.CreateProtector("GenesisBugTrackerPurpose");
            _emailService = emailService;
        }

        // GET: Invites
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity!.GetCompanyId();
            var applicationDbContext = _context.Invites.Where(i => i.CompanyId == companyId)
                                                       .Include(i => i.Company)
                                                       .Include(i => i.Invitee)
                                                       .Include(i => i.Invitor)
                                                       .Include(i => i.Project);
                                                       

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Invites/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Invites == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .Include(i => i.Project)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        // GET: Invites/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity!.GetCompanyId();
            List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);


            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");
            return View();
        }

        // POST: Invites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProjectId,InviteeEmail,InviteeFirstName,InviteeLastName,Message")] Invite invite)
        {
            ModelState.Remove("InvitorId");
            int companyId = User.Identity!.GetCompanyId();

            if (ModelState.IsValid)
            {
                try
                {
                    Guid guid = Guid.NewGuid();

                    string token = _dataProtector.Protect(guid.ToString());
                    string email = _dataProtector.Protect(invite.InviteeEmail!);
                    string company = _dataProtector.Protect(companyId.ToString());

                    string? callbackUrl = Url.Action("ProcessInvite", "Invites", new { token, email, company }, protocol: Request.Scheme);
                    string body = $@"{invite.Message} <br />
                              Please join my Company. <br />
                              Click the following link to join our team. <br />
                              <a href=""{callbackUrl}"">COLLABORATE</a>";
                    string? destination = invite.InviteeEmail;
                    Company btCompany = await _companyInfoService.GetCompanyInfoById(companyId);
                    string subject = $" Genesis Tracker: {btCompany.Name} Invite";

                    await _emailService.SendEmailAsync(destination, subject, body);

                    invite.CompanyToken = guid;
                    invite.CompanyId = companyId;
                    invite.InviteDate = DateTime.UtcNow;
                    invite.InvitorId = _userManager.GetUserId(User);
                    invite.IsValid = true;

                    await _inviteService.AddNewInviteAsync(invite);

                    return RedirectToAction("Dashboard", "Home");
                }
                catch (Exception)
                {

                    throw;
                }
            }

            List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);
            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");

            return View(invite);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ProcessInvite(string token, string email, string company)
        {
            if (token == null)
            {
                return NotFound();
            }

            Guid companyToken = Guid.Parse(_dataProtector.Unprotect(token));
            string inviteeEmail = _dataProtector.Unprotect(email);
            int companyId = int.Parse(_dataProtector.Unprotect(company));

            try
            {
                Invite invite = await _inviteService.GetInviteAsync(companyToken, inviteeEmail, companyId);

                if (invite != null)
                {
                    return View(invite);
                }
                return NotFound();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
