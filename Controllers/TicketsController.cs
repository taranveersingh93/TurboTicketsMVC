using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TurboTicketsMVC.Data;
using TurboTicketsMVC.Extensions;
using TurboTicketsMVC.Models;
using TurboTicketsMVC.Models.Enums;
using TurboTicketsMVC.Models.ViewModels;
using TurboTicketsMVC.Services;
using TurboTicketsMVC.Services.Interfaces;

namespace TurboTicketsMVC.Controllers
{
    [Authorize]
    public class TicketsController : TTBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<TTUser> _userManager;
        private readonly ITTTicketService _ticketService;
        private readonly ITTProjectService _projectService;
        private readonly ITTCompanyService _companyService;
        private readonly ITTFileService _fileService;
        private readonly ITTTicketHistoryService _ticketHistoryService;
        private readonly ITTNotificationService _notificationService;

        public TicketsController(ApplicationDbContext context,
                                 UserManager<TTUser> userManager,
                                 ITTTicketService ticketService,
                                 ITTCompanyService companyService,
                                 ITTProjectService projectService,
                                 ITTFileService fileService,
                                 ITTTicketHistoryService ticketHistoryService,
                                 ITTNotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _ticketService = ticketService;
            _companyService = companyService;
            _projectService = projectService;
            _fileService = fileService;
            _ticketHistoryService = ticketHistoryService;
            _notificationService = notificationService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity!.GetCompanyId();
            IEnumerable<Ticket> companyTickets = await _ticketService.GetAllTicketsByCompanyIdAsync(companyId);
            return View(companyTickets);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);

            if (ticket == null)
            {
                return NotFound();
            }
            IEnumerable<TTUser> projectDevelopers = await _projectService.GetProjectMembersByRoleAsync(ticket.ProjectId, nameof(TTRoles.Developer), _companyId);
            ViewData["Developers"] = new SelectList(projectDevelopers, "Id", "FullName", ticket.DeveloperUserId);

            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            string name = "Taranveer";
            int companyId = User.Identity!.GetCompanyId();
            IEnumerable<Project> companyProjects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);
            IEnumerable<TTUser> companyUsers = await _companyService.GetMembersAsync(companyId);
            ViewData["DeveloperUsers"] = new SelectList(companyUsers, "Id", "FullName");
            ViewData["Projects"] = new SelectList(companyProjects, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Archived,ArchivedByProject,ProjectId,TicketType,TicketStatus,TicketPriority,DeveloperUserId")] Ticket ticket)
        {
            ModelState.Remove("SubmitterUserId");
            if (ModelState.IsValid)
            {
                ticket.CreatedDate = DateTimeOffset.Now;
                ticket.SubmitterUserId = _userManager.GetUserId(User);
                await _ticketService.AddTicketAsync(ticket);

                //add history
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, _companyId);
                await _ticketHistoryService.AddHistoryAsync(null!, newTicket, _userId);

                //notify
                await _notificationService.TicketUpdateNotificationAsync(ticket.Id, _userId, nameof(TTTicketNotificationTypes.NewTicket));

                return RedirectToAction(nameof(Index));
            }
            int companyId = User.Identity!.GetCompanyId();
            IEnumerable<Project> companyProjects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);
            IEnumerable<TTUser> companyUsers = await _companyService.GetMembersAsync(companyId);
            ViewData["DeveloperUsers"] = new SelectList(companyUsers, "Id", "FullName");
            ViewData["Projects"] = new SelectList(companyProjects, "Id", "Name");
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            int companyId = User.Identity!.GetCompanyId();
            IEnumerable<Project> companyProjects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);
            IEnumerable<TTUser> companyUsers = await _companyService.GetMembersAsync(companyId);
            ViewData["DeveloperUsers"] = new SelectList(companyUsers, "Id", "FullName");
            ViewData["Projects"] = new SelectList(companyProjects, "Id", "Name");
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,CreatedDate,Archived,ArchivedByProject,ProjectId,TicketType,TicketStatus,TicketPriority,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, _companyId);
                try
                {
                    ticket.UpdatedDate = DateTimeOffset.Now;

                    await _ticketService.UpdateTicketAsync(ticket);
                    Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, _companyId);

                    //history
                    await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, _userId);

                    //notification
                    await _notificationService.TicketUpdateNotificationAsync(ticket.Id, _userId, nameof(TTTicketNotificationTypes.UpdateTicket));

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            int companyId = User.Identity!.GetCompanyId();
            IEnumerable<Project> companyProjects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);
            IEnumerable<TTUser> companyUsers = await _companyService.GetMembersAsync(companyId);
            ViewData["DeveloperUsers"] = new SelectList(companyUsers, "Id", "FullName");
            ViewData["Projects"] = new SelectList(companyProjects, "Id", "Name");
            return View(ticket);
        }
        //authorize PMs and devs
        //GET: Tickets/AssignTicketView
        public async Task<IActionResult> AssignTicket(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);
            AssignTicketViewModel assignTicketViewModel = new AssignTicketViewModel()
            {
                Ticket = ticket,
                DeveloperId = String.Empty,
                Developers = new SelectList(await _projectService.GetProjectMembersByRoleAsync(ticket.ProjectId, nameof(TTRoles.Developer), _companyId), "Id", "FullName", ticket.DeveloperUserId)
            };

            if (ticket.DeveloperUserId != null)
            {
                assignTicketViewModel.DeveloperId = ticket.DeveloperUserId;
            }
            return View(assignTicketViewModel);
        }

        // POST: Tickets/AssignTicketView/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTicket(AssignTicketViewModel assignTicketViewModel)
        {//OldTicket is a snapshot of the Ticket data before updating
         //and NewTicket is a snapshot of the Ticket data after the update.
         //They are used for comparison in the TicketHistoryService.
         //AsNoTracking allows for this by telling EntityFramework
         //not to track the queried data.  This avoids the error of
         //multiple active result sets

            Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(assignTicketViewModel.Ticket!.Id, _companyId);
            try
            {
                if (assignTicketViewModel != null && assignTicketViewModel.Ticket != null && assignTicketViewModel.DeveloperId != null)
                {
                    Ticket ticket = assignTicketViewModel.Ticket;
                    ticket.DeveloperUserId = assignTicketViewModel.DeveloperId;
                    await _ticketService.AssignTicketAsync(ticket.Id, ticket.DeveloperUserId);

                    //Add History
                    Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, _companyId);
                    await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, _userId);

                    //Notify
                    await _notificationService.TicketUpdateNotificationAsync(ticket.Id, assignTicketViewModel.DeveloperId, nameof(TTTicketNotificationTypes.AssignedTicket));
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {

                throw;
            }

        }

        // POST: Tickets/AssignTicketView/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTicketFromDetails(int? ticketId, string? developerId)
        {//OldTicket is a snapshot of the Ticket data before updating
         //and NewTicket is a snapshot of the Ticket data after the update.
         //They are used for comparison in the TicketHistoryService.
         //AsNoTracking allows for this by telling EntityFramework
         //not to track the queried data.  This avoids the error of
         //multiple active result sets

            Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticketId, _companyId);
            try
            {
                if (ticketId != null && developerId != null)
                {
                    await _ticketService.AssignTicketAsync(ticketId, developerId);

                    //Add History
                    Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticketId, _companyId);
                    await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, _userId);

                    //Notify
                    await _notificationService.TicketUpdateNotificationAsync(ticketId, developerId, nameof(TTTicketNotificationTypes.AssignedTicket));
                }


                return RedirectToAction(nameof(Details), new { id = ticketId });

            }
            catch (Exception)
            {

                throw;
            }

        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Archive/5
        [HttpPost, ActionName("ArchiveConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);
            if (ticket != null)
            {
                await _ticketService.ArchiveTicketAsync(ticket);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Tickets/Restore/5
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/RestoreConfirmed/5
        [HttpPost, ActionName("RestoreConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);
            if (ticket != null)
            {
                await _ticketService.RestoreTicketAsync(ticket);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,ImageFormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticketAttachment.TicketId, _companyId);

            string statusMessage;
            ModelState.Remove("TTUserId");
            if (ModelState.IsValid && ticketAttachment.ImageFormFile != null)

            {
                ticketAttachment.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.ImageFormFile);
                ticketAttachment.ImageFileName = ticketAttachment.ImageFormFile.FileName;
                ticketAttachment.ImageFileType = ticketAttachment.ImageFormFile.ContentType;
                ticketAttachment.TTUserId = _userId;
                ticketAttachment.CreatedDate = DateTimeOffset.Now;

                await _ticketService.AddTicketAttachmentAsync(ticketAttachment);
                statusMessage = "Success: New attachment added to Ticket.";

                //Add History
                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticketAttachment.TicketId, _companyId);
                await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, _userId);

                //Notify
                await _notificationService.TicketUpdateNotificationAsync(ticketAttachment.TicketId, _userId, nameof(TTTicketNotificationTypes.UpdateTicket));
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }

        public async Task<IActionResult> RemoveTicketAttachment(int? Id)
        {
            TicketAttachment? ticketAttachment = new();
            if (Id != null)
            {
                ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(Id);
            }

            Ticket? oldTicket = new();
            if (ticketAttachment != null)
            {
                oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticketAttachment.TicketId, _companyId);
                await _ticketService.RemoveTicketAttachmentAsync(ticketAttachment);

                //Add History
                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticketAttachment.TicketId, _companyId);
                await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, _userId);

                //Notify
                await _notificationService.TicketUpdateNotificationAsync(ticketAttachment.TicketId, _userId, nameof(TTTicketNotificationTypes.AttachmentRemoved));


                return RedirectToAction("Details", new { id = ticketAttachment!.TicketId });
            }
            return RedirectToAction(nameof(Index));
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketComment([Bind("Id, Comment,TicketId,UserId")] TicketComment ticketComment)
        {
            Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticketComment.TicketId, _companyId);

            if (ModelState.IsValid)
            {
                ticketComment.CreatedDate = DateTimeOffset.Now;
                await _ticketService.AddTicketCommentAsync(ticketComment);

                //Add History
                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticketComment.TicketId, _companyId);
                await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, _userId);

                //Notify
                await _notificationService.TicketUpdateNotificationAsync(ticketComment.TicketId, _userId, nameof(TTTicketNotificationTypes.CommentAdded));

                return RedirectToAction(nameof(Details), new { id = ticketComment.TicketId });
            }
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketComment.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketComment.UserId);
            return RedirectToAction(nameof(Details), new { id = ticketComment.TicketId });
        }


        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment? ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            if (ticketAttachment != null)
            {
                string? fileName = ticketAttachment.ImageFileName;
                byte[] fileData = ticketAttachment.ImageFileData!;
                string ext = Path.GetExtension(fileName!).Replace(".", "");
                Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
                return File(fileData, $"application/{ext}");
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        private bool TicketExists(int id)
        {
            return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
