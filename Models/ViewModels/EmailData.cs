using System.ComponentModel.DataAnnotations;

namespace TurboTicketsMVC.Models.ViewModels
{

    public class EmailData
    {
        [Required]
        public string? EmailAddress { get; set; }
        [Required]
        public string? EmailSubject { get; set; }
        [Required]
        public string? EmailBody { get; set; }
        [Required]
        public string? Recipient { get; set; }

    }
}
