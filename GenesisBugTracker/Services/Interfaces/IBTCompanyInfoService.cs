using GenesisBugTracker.Models;

namespace GenesisBugTracker.Services.Interfaces
{
    public interface IBTCompanyInfoService
    {
        public Task<List<BTUser>> GetAllMembersAsync(int companyId);
    }
}
