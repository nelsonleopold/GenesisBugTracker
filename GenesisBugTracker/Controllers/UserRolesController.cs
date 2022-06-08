using GenesisBugTracker.Extensions;
using GenesisBugTracker.Models;
using GenesisBugTracker.Models.Enums;
using GenesisBugTracker.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenesisBugTracker.Controllers
{
    [Authorize(nameof(BTRoles.Admin))]
    public class UserRolesController : Controller
    {
        // GET: UserRoles
        public async Task<IActionResult> ManageUserRoles()
        {
            List<ManageUserRolesViewModel> model = new();
            int companyId = User.Identity.GetCompanyId();
            List<BTUser> bTUsers = _

            return View();
        }

        // POST: UserRoles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(string member)
        {
            return View();
        }
    }
}
