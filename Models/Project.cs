using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TurboTicketsMVC.Models.Enums;

namespace TurboTicketsMVC.Models
{
    public class Project
    {
        private DateTimeOffset _createdDate;
        private DateTimeOffset _startDate;
        private DateTimeOffset _endDate;

        public int Id { get; set; }
        //foreign key
        public int CompanyId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Name { get; set; }

        [Required]
        [StringLength(2000, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Description { get; set; }

        public DateTimeOffset CreatedDate { get { return _createdDate; } set { _createdDate = value.ToUniversalTime(); } }
        public DateTimeOffset StartDate { get { return _startDate; } set { _startDate = value.ToUniversalTime(); } }
        public DateTimeOffset EndDate { get { return _endDate; } set { _endDate = value.ToUniversalTime(); } }


        //public int ProjectPriorityId { get; set; }
        public TTProjectPriorities ProjectPriority {  get; set; }

        [NotMapped]
        public IFormFile? ImageFormFile { get; set; }

        public byte[]? ImageFileData { get; set; }
        public string? ImageFileType { get; set; }
        public bool Archived { get; set; }

        //navigation
        public virtual Company? Company { get; set; }
        //public virtual ProjectPriority? ProjectPriority { get; set; }
        public virtual ICollection<TTUser> Members { get; set; } = new HashSet<TTUser>();
        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();

    }
}
