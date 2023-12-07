using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TurboTicketsMVC.Data;
using TurboTicketsMVC.Models;
using TurboTicketsMVC.Models.ViewModels;
using TurboTicketsMVC.Services.Interfaces;

namespace TurboTicketsMVC.Controllers
{
    public class HomeController : TTBaseController
    {
        private readonly ILogger<HomeController> _logger;
		private readonly ApplicationDbContext _context;
		private readonly UserManager<TTUser> _userManager;
		private readonly ITTTicketService _ticketService;
		private readonly ITTProjectService _projectService;
		private readonly ITTCompanyService _companyService;
		private readonly ITTFileService _fileService;
		private readonly ITTTicketHistoryService _ticketHistoryService;
		private readonly ITTNotificationService _notificationService;

		public HomeController(ApplicationDbContext context,
								 UserManager<TTUser> userManager,
								 ITTTicketService ticketService,
								 ITTCompanyService companyService,
								 ITTProjectService projectService,
								 ITTFileService fileService,
								 ITTTicketHistoryService ticketHistoryService,
								 ITTNotificationService notificationService,
								 ILogger<HomeController> logger)
		{
			_context = context;
			_userManager = userManager;
			_ticketService = ticketService;
			_companyService = companyService;
			_projectService = projectService;
			_fileService = fileService;
			_ticketHistoryService = ticketHistoryService;
			_notificationService = notificationService;
			_logger = logger;
		}

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
			DashboardViewModel dashboardViewModel = new()
			{
				Company = await _companyService.GetCompanyInfoAsync(_companyId),
				Projects = await _projectService.GetProjectsByCompanyIdAsync(_companyId),
				Tickets = await _ticketService.GetAllTicketsByCompanyIdAsync(_companyId),
				Members = await _companyService.GetMembersAsync(_companyId)
			};

            return View(dashboardViewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}