using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GenesisBugTracker.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Ticket Title")]
        public string? Title { get; set; }

        [Required]
        [StringLength(2000)]
        [DisplayName("Ticket Description")]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date Created")]
        public DateTime Created { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date Updated")]
        public DateTime? Updated { get; set; }

        public bool Archived { get; set; }

        [DisplayName("Archived by Project")]
        public bool ArchivedByProject { get; set; }

        public int ProjectId { get; set; }

        public int TicketPriorityId { get; set; }

        public int TicketStatusId { get; set; }

        public int TicketTypeId { get; set; }

        // Foreign Key
        [Required]
        public string? SubmitterUserId { get; set; }

        // Foreign Key
        public string? DeveloperUserId { get; set; }


        // Navigational properties
        public virtual Project? Project { get; set; }

        [DisplayName("Priority")]
        public virtual TicketPriority? TicketPriority { get; set; }

        [DisplayName("Status")]
        public virtual TicketStatus? TicketStatus { get; set; }

        [DisplayName("Type")]
        public virtual TicketType? TicketType { get; set; }

        [DisplayName("Submitted By")]
        public virtual BTUser? SubmitterUser { get; set; }

        [DisplayName("Developer")]
        public virtual BTUser? DeveloperUser { get; set; }

        // Navigational Collections
        public virtual ICollection<TicketComment> Comments { get; set; } = new HashSet<TicketComment>();

        public virtual ICollection<TicketAttachment> Attachments { get; set; } = new HashSet<TicketAttachment>();

        public virtual ICollection<Notification> Notifications { get; set; } = new HashSet<Notification>();

        public virtual ICollection<TicketHistory> TicketHistories { get; set; } = new HashSet<TicketHistory>();
    }
}
