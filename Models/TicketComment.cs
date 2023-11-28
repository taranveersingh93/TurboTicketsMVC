using System.ComponentModel.DataAnnotations;

namespace TurboTicketsMVC.Models
{
    public class TicketComment
    {
        private DateTimeOffset _createdDate;
        public int Id { get; set; }

        [Required]
        [StringLength(2000, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Comment { get; set; }

        public DateTimeOffset CreatedDate { get { return _createdDate; } set { _createdDate = value.ToUniversalTime(); } }

        //foreign keys
        public int TicketId { get; set; }
        
        [Required]
        public string? UserId { get; set; }

        //navigation
        public virtual Ticket? Ticket { get; set; }
        public virtual TTUser? User { get; set; }
    }
}
