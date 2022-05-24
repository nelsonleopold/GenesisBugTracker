using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GenesisBugTracker.Models
{
    public class ProjectPriority
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Project Priority Name")]
        public string? Name { get; set; }
    }
}
