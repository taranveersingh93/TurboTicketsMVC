using System.ComponentModel.DataAnnotations;

namespace TurboTicketsMVC.Models
{
    public class TicketStatus
    {
        public int Id { get; set; }


        [Display(Name = "Ticket Status")]
        public string? Name { get; set; }
    }
}
