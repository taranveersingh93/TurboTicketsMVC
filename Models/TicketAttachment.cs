using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurboTicketsMVC.Models
{
    public class TicketAttachment
    {
        private DateTimeOffset _createdDate;
        public int Id { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Description { get; set; }
        public DateTimeOffset CreatedDate { get { return _createdDate; } set { _createdDate = value.ToUniversalTime(); } }

        //foreign keys
        public int TicketId { get; set; }

        [Required]
        public string? TTUserId { get; set; }

        [NotMapped]
        public IFormFile? ImageFormFile { get; set; }
        public byte[]? ImageFileData { get; set; }
        public string? ImageFileType { get; set; }

        //navigation
        public virtual Ticket? Ticket { get; set; }
        public virtual TTUser? TTUser { get; set; }

    }
}
