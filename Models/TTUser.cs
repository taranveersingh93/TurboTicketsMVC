using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TurboTicketsMVC.Models
{
    public class TTUser: IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and upto {1} characters long.", MinimumLength = 2)]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and upto {1} characters long.", MinimumLength = 2)]

        public string? LastName { get; set; }

        [NotMapped]
        public string? FullName { get { return $"{FirstName} {LastName}"; } }

        [NotMapped]
        public IFormFile? ImageFormFile { get; set; }

        public byte[]? ImageFileData { get; set; }
        public string? ImageFileType { get; set; }
        //foreign key
        public int CompanyId { get; set; }

        //navigation
        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
        public virtual Company? Company { get; set; }
    }
}
