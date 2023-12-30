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
        private readonly ITTRolesService _roleService;
        public TicketsController(ApplicationDbContext context,
                                 UserManager<TTUser> userManager,
                                 ITTTicketService ticketService,
                                 ITTCompanyService companyService,
                                 ITTProjectService projectService,
                                 ITTFileService fileService,
                                 ITTTicketHistoryService ticketHistoryService,
                                 ITTNotificationService notificationService,
                                 ITTRolesService roleService)
        {
            _context = context;
            _userManager = userManager;
            _ticketService = ticketService;
            _companyService = companyService;
            _projectService = projectService;
            _fileService = fileService;
            _ticketHistoryService = ticketHistoryService;
            _notificationService = notificationService;
            _roleService = roleService;
        }

        // GET: Tickets

        public async Task<IActionResult> Index()
        {
            IEnumerable<Ticket> userTickets = await _ticketService.GetTicketsByUserIdAsync(_userId, _companyId);
            return View(userTickets);
        }

        public async Task<IActionResult> ActiveTickets()
        {
            IEnumerable<Ticket> userTickets = await _ticketService.GetTicketsByCompanyIdAsync(_companyId);
            IEnumerable<Ticket> activeTickets = userTickets.Where(t => t.TicketStatus != TTTicketStatuses.Resolved).ToList();

            return View(activeTickets);
        }

        public async Task<IActionResult> ResolvedTickets()
        {
            IEnumerable<Ticket> userTickets = await _ticketService.GetTicketsByCompanyIdAsync(_companyId);
            IEnumerable<Ticket> resolvedTickets = userTickets.Where(t => t.TicketStatus == TTTicketStatuses.Resolved).ToList();

            return View(resolvedTickets);
        }

        [Authorize(Roles = "Admin, ProjectManager")]

        public async Task<IActionResult> AllTickets()
        {
            IEnumerable<Ticket> allTickets = await _ticketService.GetAllTicketsByCompanyIdAsync(_companyId);

            return View(allTickets);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> ArchivedTickets()
        {
            IEnumerable<Ticket> allTickets = await _ticketService.GetAllTicketsByCompanyIdAsync(_companyId);
            IEnumerable<Ticket> archivedTickets = allTickets.Where(t => t.Archived || t.ArchivedByProject).ToList();

            return View(archivedTickets);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> UnarchivedTickets()
        {
            IEnumerable<Ticket> allTickets = await _ticketService.GetAllTicketsByCompanyIdAsync(_companyId);
            IEnumerable<Ticket> unarchivedTickets = allTickets.Where(t => !t.Archived && !t.ArchivedByProject).ToList();

            return View(unarchivedTickets);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> UnassignedTickets()
        {
            IEnumerable<Ticket> allTickets = await _ticketService.GetAllTicketsByCompanyIdAsync(_companyId);
            IEnumerable<Ticket> unassignedTickets = allTickets.Where(t => t.DeveloperUserId == null).ToList();

            return View(unassignedTickets);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AssignedTickets()
        {
            IEnumerable<Ticket> allTickets = await _ticketService.GetAllTicketsByCompanyIdAsync(_companyId);
            IEnumerable<Ticket> assignedTickets = allTickets.Where(t => t.DeveloperUserId != null).ToList();

            return View(assignedTickets);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);
            bool canViewTicket = User.IsInRole("ProjectManager");
            bool canActOnTicket = await _ticketService.CanActOnTicket(_userId, ticket.Id, _companyId);
            if (ticket == null)
            {
                return NotFound();
            }

            if (canActOnTicket || canViewTicket)
            {
                //assigning dev
                IEnumerable<TTUser> projectDevelopers = await _projectService.GetProjectMembersByRoleAsync(ticket.ProjectId, nameof(TTRoles.Developer), _companyId);
                ViewData["ProjectDevelopers"] = new SelectList(projectDevelopers, "Id", "FullName", ticket.DeveloperUserId);
                //editing project team modal
                IEnumerable<TTUser> companyManagers = await _roleService.GetUsersInRoleAsync(nameof(TTRoles.ProjectManager), _companyId);
                IEnumerable<TTUser> companyDevelopers = await _roleService.GetUsersInRoleAsync(nameof(TTRoles.Developer), _companyId);
                IEnumerable<TTUser> companySubmitters = await _roleService.GetUsersInRoleAsync(nameof(TTRoles.Submitter), _companyId);
                IEnumerable<TTUser> selectedDevelopers = await _projectService.GetProjectMembersByRoleAsync(ticket.ProjectId, nameof(TTRoles.Developer), _companyId);
                IEnumerable<TTUser> selectedSubmitters = await _projectService.GetProjectMembersByRoleAsync(ticket.ProjectId, nameof(TTRoles.Submitter), _companyId);
                TTUser? projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);
                string? projectManagerId = string.Empty;
                if (projectManager != null)
                {
                    projectManagerId = projectManager.Id;
                }
                IEnumerable<string> developerIds = selectedDevelopers.Select(d => d.Id);
                IEnumerable<string> submitterIds = selectedSubmitters.Select(s => s.Id);


                ViewData["CompanyManagers"] = new SelectList(companyManagers, "Id", "FullName", projectManagerId);
                ViewData["CompanyDevelopers"] = new MultiSelectList(companyDevelopers, "Id", "FullName", developerIds);
                ViewData["CompanySubmitters"] = new MultiSelectList(companySubmitters, "Id", "FullName", submitterIds);
                return View(ticket);
            }
            return NotFound();
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity!.GetCompanyId();
            IEnumerable<Project> userProjects = await _projectService.GetUserProjectsAsync(_userId);
            ViewData["Projects"] = new SelectList(userProjects, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Archived,ArchivedByProject,ProjectId,TicketType,TicketStatus,TicketPriority")] Ticket ticket)
        {
            ModelState.Remove("SubmitterUserId");
            bool canMakeTickets = false;
            if (ticket.ProjectId != null)
            {
                canMakeTickets = await _ticketService.CanMakeTickets(_userId, ticket.ProjectId, _companyId);
            }
            if (ModelState.IsValid && canMakeTickets)
            {
                ticket.CreatedDate = DateTimeOffset.Now;
                ticket.SubmitterUserId = _userManager.GetUserId(User);
                await _ticketService.AddTicketAsync(ticket);

                //add history
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, _companyId);
                await _ticketHistoryService.AddHistoryAsync(null!, newTicket, _userId);

                //notify
                await _notificationService.TicketUpdateNotificationAsync(ticket.Id, _userId, nameof(TTTicketNotificationTypes.NewTicket), _userId);

                return RedirectToAction(nameof(Index));
            }
            IEnumerable<Project> userProjects = await _projectService.GetUserProjectsAsync(_userId);
            IEnumerable<TTUser> companyDevs = await _roleService.GetUsersInRoleAsync(nameof(TTRoles.Developer), _companyId);
            ViewData["DeveloperUsers"] = new SelectList(companyDevs, "Id", "FullName");
            ViewData["Projects"] = new SelectList(userProjects, "Id", "Name");
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            bool canActOnTicket = await _ticketService.CanActOnTicket(_userId, id, _companyId);

            if (canActOnTicket)
            {
                Ticket ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);
                if (ticket == null)
                {
                    return NotFound();
                }
                int companyId = User.Identity!.GetCompanyId();
                IEnumerable<Project> companyProjects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);
                IEnumerable<TTUser> companyDevs = await _roleService.GetUsersInRoleAsync(nameof(TTRoles.Developer), _companyId);
                ViewData["DeveloperUsers"] = new SelectList(companyDevs, "Id", "FullName");
                ViewData["Projects"] = new SelectList(companyProjects, "Id", "Name");
                return View(ticket);

            }
            else
            {
                return NotFound();
            }
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
                bool canActOnTicket = await _ticketService.CanActOnTicket(_userId, oldTicket.Id, _companyId);
                if (canActOnTicket)
                {
                    try
                    {
                        ticket.UpdatedDate = DateTimeOffset.Now;

                        await _ticketService.UpdateTicketAsync(ticket);
                        Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, _companyId);

                        //history
                        await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, _userId);

                        //notification
                        await _notificationService.TicketUpdateNotificationAsync(ticket.Id, _userId, nameof(TTTicketNotificationTypes.UpdateTicket), _userId);

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

                }
                return RedirectToAction(nameof(Details), new { id = ticket.Id });
            }
            int companyId = User.Identity!.GetCompanyId();
            IEnumerable<Project> companyProjects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);
            IEnumerable<TTUser> companyDevs = await _roleService.GetUsersInRoleAsync(nameof(TTRoles.Developer), _companyId);
            ViewData["DeveloperUsers"] = new SelectList(companyDevs, "Id", "FullName");
            ViewData["Projects"] = new SelectList(companyProjects, "Id", "Name");
            return View(ticket);
        }
        //authorize PMs and devs
        //GET: Tickets/AssignTicketView
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AssignTicket(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);
            bool canAssignDeveloper = await _ticketService.CanAssignDeveloper(_userId, ticket.Id, _companyId);

            if (canAssignDeveloper)
            {
                IEnumerable<TTUser> availableDevelopers = await _projectService.GetProjectMembersByRoleAsync(ticket.ProjectId, nameof(TTRoles.Developer), _companyId);

                AssignTicketViewModel assignTicketViewModel = new AssignTicketViewModel()
                {
                    Ticket = ticket,
                    DeveloperId = String.Empty,
                    Developers = new SelectList(availableDevelopers, "Id", "FullName", ticket.DeveloperUserId),
                    DevelopersAvailable = availableDevelopers.Count() > 0
                };

                if (ticket.DeveloperUserId != null)
                {
                    assignTicketViewModel.DeveloperId = ticket.DeveloperUserId;
                }

                //Data for Viewbag for edit team modal
                IEnumerable<TTUser> projectManagers = await _roleService.GetUsersInRoleAsync(nameof(TTRoles.ProjectManager), _companyId);
                IEnumerable<TTUser> developers = await _roleService.GetUsersInRoleAsync(nameof(TTRoles.Developer), _companyId);
                IEnumerable<TTUser> submitters = await _roleService.GetUsersInRoleAsync(nameof(TTRoles.Submitter), _companyId);
                IEnumerable<TTUser> selectedDevelopers = await _projectService.GetProjectMembersByRoleAsync(ticket.ProjectId, nameof(TTRoles.Developer), _companyId);
                IEnumerable<TTUser> selectedSubmitters = await _projectService.GetProjectMembersByRoleAsync(ticket.ProjectId, nameof(TTRoles.Submitter), _companyId);
                TTUser? projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);
                string? projectManagerId = string.Empty;
                if (projectManager != null)
                {
                    projectManagerId = projectManager.Id;
                }
                IEnumerable<string> developerIds = selectedDevelopers.Select(d => d.Id);
                IEnumerable<string> submitterIds = selectedSubmitters.Select(s => s.Id);


                ViewData["ProjectManagers"] = new SelectList(projectManagers, "Id", "FullName", projectManagerId);
                ViewData["Developers"] = new MultiSelectList(developers, "Id", "FullName", developerIds);
                ViewData["Submitters"] = new MultiSelectList(submitters, "Id", "FullName", submitterIds);
                return View(assignTicketViewModel);
            }
            return NotFound();
        }

        // POST: Tickets/AssignTicketView/
        [Authorize(Roles = "Admin, ProjectManager")]
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
            bool canAssignDeveloper = await _ticketService.CanAssignDeveloper(_userId, oldTicket.Id, _companyId);
            try
            {
                if (canAssignDeveloper && assignTicketViewModel != null && assignTicketViewModel.Ticket != null && assignTicketViewModel.DeveloperId != null)
                {
                    Ticket ticket = assignTicketViewModel.Ticket;
                    if (ticket.TicketStatus == TTTicketStatuses.New)
                    {
                        await _ticketService.ChangeTicketStatus(ticket.Id, _companyId, TTTicketStatuses.Development);
                        //Add History
                        Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, _companyId);
                        await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, _userId);

                        //Notify
                        await _notificationService.TicketUpdateNotificationAsync(ticket.Id, _userId, nameof(TTTicketNotificationTypes.UpdateTicket), _userId);

                        //Reassign old ticket
                        oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(assignTicketViewModel.Ticket!.Id, _companyId);
                    }
                    ticket.DeveloperUserId = assignTicketViewModel.DeveloperId;
                    await _ticketService.AssignTicketAsync(ticket.Id, ticket.DeveloperUserId);
                    //Add History
                    Ticket? newerTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, _companyId);
                    await _ticketHistoryService.AddHistoryAsync(oldTicket, newerTicket, _userId);

                    //Notify
                    await _notificationService.TicketUpdateNotificationAsync(ticket.Id, assignTicketViewModel.DeveloperId, nameof(TTTicketNotificationTypes.AssignedTicket), _userId);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {

                throw;
            }

        }

        // POST: Tickets/AssignTicketView/
        [Authorize(Roles = "Admin, ProjectManager")]
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
            bool canAssignDeveloper = await _ticketService.CanAssignDeveloper(_userId, ticketId, _companyId);

            try
            {
                if (canAssignDeveloper)
                {
                    if (ticketId != null && developerId != null)
                    {
                        if (oldTicket.TicketStatus == TTTicketStatuses.New)
                        {
                            await _ticketService.ChangeTicketStatus(oldTicket.Id, _companyId, TTTicketStatuses.Development);
                            //Add History
                            Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(oldTicket.Id, _companyId);
                            await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, _userId);

                            //Notify
                            await _notificationService.TicketUpdateNotificationAsync(newTicket.Id, _userId, nameof(TTTicketNotificationTypes.UpdateTicket), _userId);

                            //Reassign old ticket
                            oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(newTicket.Id, _companyId);
                        }

                        await _ticketService.AssignTicketAsync(ticketId, developerId);

                        //Add History
                        Ticket? newerTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticketId, _companyId);
                        await _ticketHistoryService.AddHistoryAsync(oldTicket, newerTicket, _userId);

                        //Notify
                        await _notificationService.TicketUpdateNotificationAsync(ticketId, developerId, nameof(TTTicketNotificationTypes.AssignedTicket), _userId);
                    }


                    return RedirectToAction(nameof(Details), new { id = ticketId });

                }
                else
                {
                    return NotFound();
                }

            }
            catch (Exception)
            {

                throw;
            }

        }

        // GET: Tickets/Delete/5
        [Authorize(Roles = "Admin, ProjectManager")]
        //needs controller logic for PM
        public async Task<IActionResult> Archive(int? id)
        {
            bool userAuthorized = await _ticketService.CanAssignDeveloper(_userId, id, _companyId);
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }
            if (userAuthorized)
            {
                Ticket ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);
                if (ticket == null)
                {
                    return NotFound();
                }

                return View(ticket);

            }
            return NotFound();
        }

        // POST: Tickets/Archive/5
        [HttpPost, ActionName("ArchiveConfirmed")]
        [Authorize(Roles = "Admin, ProjectManager")]

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            bool userAuthorized = await _ticketService.CanAssignDeveloper(_userId, id, _companyId);

            Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(id, _companyId);

            if (oldTicket != null && userAuthorized)
            {
                Ticket? ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);

                await _ticketService.ArchiveTicketAsync(ticket);
                //Add History

                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(id, _companyId);
                await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, _userId);

                //Notify
                await _notificationService.TicketUpdateNotificationAsync(id, _userId, nameof(TTTicketNotificationTypes.UpdateTicket), _userId);

                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }

        // GET: Tickets/Restore/5
        [Authorize(Roles = "Admin, ProjectManager")]

        public async Task<IActionResult> Restore(int? id)
        {
            bool userAuthorized = await _ticketService.CanAssignDeveloper(_userId, id, _companyId);
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }
            if (userAuthorized)
            {
                Ticket ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);
                if (ticket == null)
                {
                    return NotFound();
                }

                return View(ticket);
            }
            return NotFound();
        }

        // POST: Tickets/RestoreConfirmed/5
        [HttpPost, ActionName("RestoreConfirmed")]
        [Authorize(Roles = "Admin, ProjectManager")]

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            bool userAuthorized = await _ticketService.CanAssignDeveloper(_userId, id, _companyId);

            Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(id, _companyId);

            if (oldTicket != null && userAuthorized)
            {
                Ticket? ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);

                await _ticketService.RestoreTicketAsync(ticket);
                //Add History

                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(id, _companyId);
                await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, _userId);

                //Notify
                await _notificationService.TicketUpdateNotificationAsync(id, _userId, nameof(TTTicketNotificationTypes.UpdateTicket), _userId);

                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,ImageFormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticketAttachment.TicketId, _companyId);

            bool canActOnTicket = await _ticketService.CanActOnTicket(_userId, ticketAttachment.TicketId, _companyId);

            string statusMessage;
            ModelState.Remove("TTUserId");
            if (ModelState.IsValid && ticketAttachment.ImageFormFile != null && canActOnTicket)

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
                await _notificationService.TicketUpdateNotificationAsync(ticketAttachment.TicketId, _userId, nameof(TTTicketNotificationTypes.AttachmentAdded), _userId);
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }

        public async Task<IActionResult> RemoveTicketAttachment(int? Id)
        {
            bool canActOnTicket = false;
            TicketAttachment? ticketAttachment = new();
            if (Id != null)
            {
                ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(Id);
                canActOnTicket = await _ticketService.CanActOnTicket(_userId, ticketAttachment!.TicketId, _companyId);
            }



            Ticket? oldTicket = new();
            if (ticketAttachment != null && canActOnTicket)
            {
                oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticketAttachment.TicketId, _companyId);
                await _ticketService.RemoveTicketAttachmentAsync(ticketAttachment);

                //Add History
                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticketAttachment.TicketId, _companyId);
                await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, _userId);

                //Notify
                await _notificationService.TicketUpdateNotificationAsync(ticketAttachment.TicketId, _userId, nameof(TTTicketNotificationTypes.AttachmentRemoved), _userId);


                return RedirectToAction("Details", new { id = ticketAttachment!.TicketId });
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Developer")]
        public async Task<IActionResult> Resolve(int? id)
        {
            if (id != null)
            {
                Ticket? ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);
                if (ticket.DeveloperUserId == _userId)
                {
                    return View(ticket);
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost, ActionName("ResolveConfirmed")]
        [Authorize(Roles = "Developer")]

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveConfirmed(int id)
        {
            Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(id, _companyId);

            if (oldTicket != null && oldTicket.DeveloperUserId == _userId)
            {
                Ticket? ticket = await _ticketService.GetTicketByIdAsync(id, _companyId);

                await _ticketService.ResolveTicketAsync(ticket);
                //Add History

                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(id, _companyId);
                await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, _userId);

                //Notify
                await _notificationService.TicketUpdateNotificationAsync(id, _userId, nameof(TTTicketNotificationTypes.UpdateTicket), _userId);

                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketComment([Bind("Id, Comment,TicketId,UserId")] TicketComment ticketComment)
        {
            Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticketComment.TicketId, _companyId);
            bool canActOnTicket = await _ticketService.CanActOnTicket(_userId, ticketComment.TicketId, _companyId);

            if (ModelState.IsValid && canActOnTicket)
            {
                ticketComment.CreatedDate = DateTimeOffset.Now;
                await _ticketService.AddTicketCommentAsync(ticketComment);

                //Add History
                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticketComment.TicketId, _companyId);
                await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, _userId);

                //Notify
                await _notificationService.TicketUpdateNotificationAsync(ticketComment.TicketId, _userId, nameof(TTTicketNotificationTypes.CommentAdded), _userId);

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

        [HttpGet]
        public async Task<IActionResult> MarkRead(int id)
        {
            if (id != null)
            {
                Notification? notification = await _notificationService.GetNotificationAsync(id);
                await _notificationService.MarkNotificationRead(notification);
                return RedirectToAction(nameof(Details), new { id = notification.TicketId });
            }
            else
            {
                return NotFound();
            }
        }

        private bool TicketExists(int id)
        {
            return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
