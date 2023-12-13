using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using TurboTicketsMVC.Data;
using TurboTicketsMVC.Extensions;
using TurboTicketsMVC.Models;
using TurboTicketsMVC.Models.ChartModels;
using TurboTicketsMVC.Models.Enums;
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

		[Authorize]
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

		public async Task<JsonResult> TicketsDevelopersChart()
		{
            TicketsDevelopersData ticketsDevelopersData = new();
            List<TicketsDevelopersBar> barData = new();

            IEnumerable<Project> projects = await _projectService.GetProjectsByCompanyIdAsync(_companyId);

            //Bar One
            TicketsDevelopersBar barOne = new()
            {
                X = projects.Select(p => p.Name).ToArray(),
                Y = projects.SelectMany(p => p.Tickets)
                            .GroupBy(t => t.ProjectId)
                            .Select(g => g.Count())
                            .ToArray(),
                Name = "Tickets",
                Type = "bar"
            };

            //Bar Two
            TicketsDevelopersBar barTwo = new()
            {
                X = projects.Select(p => p.Name).ToArray(),
                Y = projects.Select(async p => (await _projectService.GetProjectMembersByRoleAsync(p.Id, nameof(TTRoles.Developer),_companyId)).Count()).Select(c => c.Result).ToArray(),

                Name = "Developers",
                Type = "bar"
            };

            barData.Add(barOne);
            barData.Add(barTwo);

            ticketsDevelopersData.Data = barData;

            return Json(ticketsDevelopersData);
        }

		public IActionResult Landing()
		{
			return View();
		}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}