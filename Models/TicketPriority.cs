using System.ComponentModel.DataAnnotations;

namespace TurboTicketsMVC.Models
{
    public class TicketPriority
    {
        public int Id { get; set; }

        [Display(Name = "Ticket Priority")]
        public string? Name { get; set; }
    }
}
