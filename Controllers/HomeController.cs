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

        [Authorize]
        public IActionResult Dashboard()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction(nameof(GenericError));
            }
        }

        [Authorize]
        public IActionResult GetStarted()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction(nameof(GenericError));
            }
        }

        public IActionResult Landing()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction(nameof(GenericError));
            }
        }

        public IActionResult AccessDeniedError()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction(nameof(GenericError));
            }
        }

        public IActionResult GenericError()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction(nameof(NotFoundError));
            }
        }
        public IActionResult NotFoundError()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction(nameof(GenericError));
            }
        }
        
        public async Task<IActionResult> MarkAllNotificationsRead()
        {
            try
            {
                await _notificationService.MarkAllNotificationsRead(_userId);
                return RedirectToAction("Dashboard", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction(nameof(GenericError));
            }
        }

        public async Task<IActionResult> MarkAllNotificationsUnread()
        {
            try
            {
                await _notificationService.MarkAllNotificationsUnread(_userId);
                return RedirectToAction("Dashboard", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction(nameof(GenericError));
            }
        }
    }
}