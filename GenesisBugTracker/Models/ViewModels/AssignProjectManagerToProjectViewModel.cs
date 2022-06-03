using Microsoft.AspNetCore.Mvc.Rendering;

namespace GenesisBugTracker.Models.ViewModels
{
    public class AssignProjectManagerToProjectViewModel
    {
        public Project? Project { get; set; }
        public string?  PMId { get; set; }
        public SelectList? PMList { get; set; }
        
    }
}
