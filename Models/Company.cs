using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurboTicketsMVC.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "TCompany Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Name { get; set; }

        [Display(Name = "Company Description")]
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Description { get; set; }

        [NotMapped]
        public IFormFile? ImageFormFile { get; set; }

        public byte[]? ImageFormData { get; set; }
        public string? ImageFormType { get; set; }
        //navigation
        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
        public virtual ICollection<TTUser> Members { get; set; } = new HashSet<TTUser>();
        public virtual ICollection<Invite> Invites { get; set; } = new HashSet<Invite>();
    }
}
