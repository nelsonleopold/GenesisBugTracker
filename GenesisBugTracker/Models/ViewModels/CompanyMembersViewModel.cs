using GenesisBugTracker.Models.Enums;

namespace GenesisBugTracker.Models.ViewModels
{
    public class CompanyMembersViewModel
    {
        public Company? Company { get; set; }
        public BTUser? BTUser { get; set; }
        public List<string>? Roles { get; set; }

    }
}
