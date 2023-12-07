namespace TurboTicketsMVC.Models.ViewModels
{

		public class DashboardViewModel
		{
			public Company? Company { get; set; }
			public IEnumerable<Project>? Projects { get; set; }
			public IEnumerable<Ticket>? Tickets { get; set; }
			public IEnumerable<TTUser>? Members { get; set; }
		}
	
}
