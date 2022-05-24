using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GenesisBugTracker.Models
{
    public class TicketType
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Type Name")]
        public string? Name { get; set; }
    }
}
