using System.ComponentModel.DataAnnotations;

namespace TurboTicketsMVC.Models
{
    public class TicketHistory
    {
        private DateTimeOffset _createdDate;
        public int Id { get; set; }
        //foreign key
        public int TicketId { get; set; }

        [Required]
        public string? UserId { get; set; }
        public string? PropertyName { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset CreatedDate { get { return _createdDate; } set { _createdDate = value.ToUniversalTime(); } }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        //nav
        public virtual Ticket? Ticket { get; set; }
        public virtual TTUser? User { get; set; }
    }
}
