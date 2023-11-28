using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using TurboTicketsMVC.Models.Enums;

namespace TurboTicketsMVC.Models
{
    public class Ticket
    {
        private DateTimeOffset _createdDate;
        private DateTimeOffset? _updatedDate;

        public int Id { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Title { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Description { get; set; }

        public DateTimeOffset CreatedDate { get { return _createdDate; } set { _createdDate = value.ToUniversalTime(); } }
        public DateTimeOffset? UpdatedDate
        {
            get => _updatedDate; set
            {
                if (value.HasValue)
                {
                    _updatedDate = value.Value.ToUniversalTime();
                }
            }
        }

        public bool Archived { get; set; }
        public bool ArchivedByProject { get; set; }

        //foreign keys
        public int ProjectId { get; set; }
        //public int TicketTypeId { get; set; }
        //public int TicketStatusId { get; set; }
        //public int TicketPriorityId { get; set; }
        public TTTicketTypes TicketType { get; set; }
        public TTTicketStatuses TicketStatus { get; set; }
        public TTTicketPriorities TicketPriority { get; set; }

        public string? DeveloperUserId { get; set; }
        [Required]
        public string? SubmitterUserId { get; set; }

        //navigation
        public virtual Project? Project { get; set; }
        //public virtual TicketPriority? TicketPriority { get; set; }
        //public virtual TicketType? TicketType { get; set; }
        //public virtual TicketStatus? TicketStatus { get; set; }
        public virtual TTUser? DeveloperUser { get; set; }
        public virtual TTUser? SubmitterUser { get; set; }
        
        public virtual ICollection<TicketComment> Comments { get; set; } = new HashSet<TicketComment>();
        public virtual ICollection<TicketAttachment> Attachments { get; set; } = new HashSet<TicketAttachment>();
        public virtual ICollection<TicketHistory> History { get; set; } = new HashSet<TicketHistory>();
    }
}
