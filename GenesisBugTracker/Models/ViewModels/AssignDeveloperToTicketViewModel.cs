using Microsoft.AspNetCore.Mvc.Rendering;

namespace GenesisBugTracker.Models.ViewModels
{
    public class AssignDeveloperToTicketViewModel
    {
        public Ticket? Ticket { get; set; }
        public string DevId { get; set; }
        public SelectList? DevList { get; set; }
    }
}
