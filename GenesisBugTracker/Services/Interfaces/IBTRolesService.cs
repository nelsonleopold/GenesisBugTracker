using GenesisBugTracker.Models;

namespace GenesisBugTracker.Services.Interfaces
{
    public interface IBTRolesService
    {
        public Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId);
        public Task<bool> IsUserInRoleAsync(BTUser user, string roleName);
        
    }
}
