using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using TurboTicketsMVC.Models.Enums;

namespace TurboTicketsMVC.Models
{
    public class Notification
    {
        private DateTimeOffset _createdDate;

        public int Id { get; set; }
        //foreign keys
        public int? ProjectId { get; set; }
        public int? TicketId { get; set; }

        [Required]
        public string? SenderId { get; set; }

        [Required]
        public string? RecipientId { get; set; }

        //public int NotificationTypeId { get; set; }
        public TTNotificationTypes NotificationType { get; set; }
        //foreign keys end

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Title { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Message { get; set; }

        public DateTimeOffset CreatedDate { get { return _createdDate; } set { _createdDate = value.ToUniversalTime(); } }

        public bool HasBeenViewed { get; set; }

        //navigation
        //public virtual NotificationType? NotificationType { get; set; }
        public virtual Ticket? Ticket { get; set; }
        public virtual Project? Project { get; set; }
        public virtual TTUser? Sender { get; set; }
        public virtual TTUser? Recipient { get; set; }
    }
}
