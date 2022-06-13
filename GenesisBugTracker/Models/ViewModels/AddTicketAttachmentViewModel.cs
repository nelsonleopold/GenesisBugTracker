namespace GenesisBugTracker.Models.ViewModels
{
    public class AddTicketAttachmentViewModel
    {
        public Ticket? Ticket { get; set; }
        public TicketAttachment? TicketAttachment { get; set; }
        public int TicketId { get; set; }
        public string? UserId { get; set; }
        public IFormFile? FormFile { get; set; }

    }
}
