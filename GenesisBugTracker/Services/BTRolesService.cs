using GenesisBugTracker.Data;
using GenesisBugTracker.Models;
using GenesisBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace GenesisBugTracker.Services
{
    public class BTRolesService : IBTRolesService
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<BTUser> _userManager;

        public BTRolesService(ApplicationDbContext context,
                              RoleManager<IdentityRole> roleManager,
                              UserManager<BTUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId)
        {
            try
            {
                List<BTUser> bTUsers = (await _userManager.GetUsersInRoleAsync(roleName)).ToList();
                List<BTUser> results = bTUsers.Where(u => u.CompanyId == companyId).ToList();

                return results;
            }
            catch (Exception)
            {

                throw;
            }
        }


        #region
        public async Task<bool> IsUserInRoleAsync(BTUser user, string roleName)
        {
            try
            {
                bool result = await _userManager.IsInRoleAsync(user, roleName);
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
    }
}
