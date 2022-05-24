using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GenesisBugTracker.Models
{
    public class TicketStatus
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Status Name")]
        public string? Name { get; set; }
    }
}
