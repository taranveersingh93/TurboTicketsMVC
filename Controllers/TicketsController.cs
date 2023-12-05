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

        public TicketsController(ApplicationDbContext context,
                                 UserManager<TTUser> userManager,
                                 ITTTicketService ticketService,
                                 ITTCompanyService companyService,
                                 ITTProjectService projectService)
        {
            _context = context;
            _userManager = userManager;
            _ticketService = ticketService;
            _companyService = companyService;
            _projectService = projectService;
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

            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
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
                _context.Add(ticket);
                await _context.SaveChangesAsync();
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
                try
                {
                    ticket.UpdatedDate = DateTimeOffset.Now;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
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
                Developers = new SelectList (await _projectService.GetProjectMembersByRoleAsync(ticket.ProjectId, nameof(TTRoles.Developer), _companyId), "Id", "FullName", ticket.DeveloperUserId)
            };
            return View(assignTicketViewModel);
        }

        // POST: Tickets/AssignTicketView/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTicket(AssignTicketViewModel assignTicketViewModel)
        {
            if (assignTicketViewModel != null && assignTicketViewModel.Ticket != null && assignTicketViewModel.DeveloperId != null)
            {
                Ticket ticket = assignTicketViewModel.Ticket;
                ticket.DeveloperUserId = assignTicketViewModel.DeveloperId;
                await _ticketService.AssignTicketAsync(ticket.Id, ticket.DeveloperUserId);
            }
            return RedirectToAction(nameof(Index));
        }



        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.SubmitterUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddTicketComment([Bind("Id, Comment,TicketId,UserId")] TicketComment ticketComment)
		{
			if (ModelState.IsValid)
			{
                ticketComment.CreatedDate = DateTimeOffset.Now;
                await _ticketService.AddTicketCommentAsync(ticketComment);
				return RedirectToAction(nameof(Details), new {id = ticketComment.TicketId});
			}
			ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketComment.TicketId);
			ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketComment.UserId);
			return RedirectToAction(nameof(Details), new {id = ticketComment.TicketId});
		}

		private bool TicketExists(int id)
        {
          return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
