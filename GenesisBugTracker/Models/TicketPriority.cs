using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GenesisBugTracker.Models
{
    public class TicketPriority
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Priority Name")]
        public string? Name { get; set; }
    }
}
