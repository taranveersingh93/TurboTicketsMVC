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
using TurboTicketsMVC.Services.Interfaces;

namespace TurboTicketsMVC.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITurboTicketsService _turboTicketsService;
        private readonly UserManager<TTUser> _userManager;

        public TicketsController(ApplicationDbContext context,
                                 ITurboTicketsService turboTicketsService,
                                 UserManager<TTUser> userManager)
        {
            _context = context;
            _turboTicketsService = turboTicketsService;
            _userManager = userManager;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity!.GetCompanyId();
            IEnumerable<Ticket> companyTickets = await _turboTicketsService.GetTicketsByCompanyAsync(companyId);
            return View(companyTickets);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity!.GetCompanyId();
            IEnumerable<Project> companyProjects = await _turboTicketsService.GetProjectsByCompanyAsync(companyId);
            IEnumerable<TTUser> companyUsers = await _turboTicketsService.GetUsersByCompanyAsync(companyId);
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
            IEnumerable<Project> companyProjects = await _turboTicketsService.GetProjectsByCompanyAsync(companyId);
            IEnumerable<TTUser> companyUsers = await _turboTicketsService.GetUsersByCompanyAsync(companyId);
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
            IEnumerable<Project> companyProjects = await _turboTicketsService.GetProjectsByCompanyAsync(companyId);
            IEnumerable<TTUser> companyUsers = await _turboTicketsService.GetUsersByCompanyAsync(companyId);
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
            IEnumerable<Project> companyProjects = await _turboTicketsService.GetProjectsByCompanyAsync(companyId);
            IEnumerable<TTUser> companyUsers = await _turboTicketsService.GetUsersByCompanyAsync(companyId);
            ViewData["DeveloperUsers"] = new SelectList(companyUsers, "Id", "FullName");
            ViewData["Projects"] = new SelectList(companyProjects, "Id", "Name");
            return View(ticket);
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

        private bool TicketExists(int id)
        {
          return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
