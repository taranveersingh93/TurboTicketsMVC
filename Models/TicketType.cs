using System.ComponentModel.DataAnnotations;

namespace TurboTicketsMVC.Models
{
    public class TicketType
    {
        public int Id { get; set; }

        [Display(Name = "Ticket Type")]
        public string? Name { get; set; }
    }
}
