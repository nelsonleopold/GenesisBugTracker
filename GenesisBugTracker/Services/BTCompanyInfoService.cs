using GenesisBugTracker.Data;
using GenesisBugTracker.Models;
using GenesisBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GenesisBugTracker.Services
{
    public class BTCompanyInfoService : IBTCompanyInfoService
    {
        private readonly ApplicationDbContext _context;

        public BTCompanyInfoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BTUser>> GetAllMembersAsync(int companyId)
        {
            try
            {
                List<BTUser> members = new();

                members = await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync();

                return members;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
