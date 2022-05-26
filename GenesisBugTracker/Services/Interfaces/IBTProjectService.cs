using GenesisBugTracker.Models;

namespace GenesisBugTracker.Services.Interfaces
{
    public interface IBTProjectService
    {
        public Task AddNewProjectAsync(Project project);
        public Task ArchiveProjectAsync(Project project);
        public Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId);
        public Task UpdateProjectAsync(Project project);
        public Task<Project> GetProjectByIdAsync(int projectId);
    }
}
