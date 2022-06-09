using GenesisBugTracker.Extensions;
using GenesisBugTracker.Models;
using GenesisBugTracker.Models.Enums;
using GenesisBugTracker.Models.ViewModels;
using GenesisBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GenesisBugTracker.Controllers
{
    [Authorize(Roles = nameof(BTRoles.Admin))]
    public class UserRolesController : Controller
    {
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTRolesService _rolesService;

        public UserRolesController(IBTCompanyInfoService companyInfoService, 
                                   IBTRolesService rolesService)
        {
            _companyInfoService = companyInfoService;
            _rolesService = rolesService;
        }

        // GET: UserRoles
        public async Task<IActionResult> ManageUserRoles()
        {
            List<ManageUserRolesViewModel> model = new();
            int companyId = User.Identity!.GetCompanyId();
            List<BTUser> bTUsers = await _companyInfoService.GetAllMembersAsync(companyId);

            foreach (BTUser bTUser in bTUsers)
            {
                ManageUserRolesViewModel viewModel = new();
                viewModel.BTUser = bTUser;
                IEnumerable<string> currentRoles = await _rolesService.GetUserRolesAsync(bTUser);
                viewModel.Roles = new MultiSelectList(await _rolesService.GetBTRolesAsync(), "Name", "Name", currentRoles);

                model.Add(viewModel);
            }

            return View(model);
        }

        // POST: UserRoles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel member)
        {
            int companyId = User.Identity!.GetCompanyId();
            BTUser? bTUser = (await _companyInfoService.GetAllMembersAsync(companyId)).FirstOrDefault(u => u.Id == member.BTUser!.Id);
            IEnumerable<string> currentRoles = await _rolesService.GetUserRolesAsync(bTUser!);
            string? selectedUserRole = member.SelectedRoles!.FirstOrDefault();

            if (!string.IsNullOrEmpty(selectedUserRole))
            {
                if (await _rolesService.RemoveUserFromRolesAsync(bTUser!, currentRoles))
                {
                    await _rolesService.AddUserToRoleAsync(bTUser!, selectedUserRole);
                }               
            }
            else
            {
                return RedirectToAction(nameof(ManageUserRoles));
            }

            return RedirectToAction("CompanyMembers", "Companies", new { id = companyId});
        }
    }
}
