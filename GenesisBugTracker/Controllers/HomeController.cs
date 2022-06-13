using GenesisBugTracker.Extensions;
using GenesisBugTracker.Models;
using GenesisBugTracker.Models.ViewModels;
using GenesisBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GenesisBugTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTProjectService _projectService;

        public HomeController(ILogger<HomeController> logger,
                              UserManager<BTUser> userManager,
                              IBTCompanyInfoService companyInfoService,
                              IBTTicketService ticketService,
                              IBTProjectService projectService)
        {
            _logger = logger;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
            _ticketService = ticketService;
            _projectService = projectService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            if (User.Identity!.IsAuthenticated)
            {
                BTUser user = await _userManager.GetUserAsync(User);
                int companyId = User.Identity.GetCompanyId();
                Company company = await _companyInfoService.GetCompanyInfoById(companyId);
                List<BTUser> members = await _companyInfoService.GetAllMembersAsync(companyId);
                List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyIdAsync(companyId);
                List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

                var model = new DashboardViewModel()
                {
                    Company = company,
                    Tickets = tickets,
                    Projects = projects,
                    Members = members,
                };
                return View(model);
            }
            else
            {
                return View();
            }
        }

        public IActionResult LandingPage()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}