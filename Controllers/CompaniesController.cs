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
using TurboTicketsMVC.Models;
using TurboTicketsMVC.Models.Enums;
using TurboTicketsMVC.Models.ViewModels;
using TurboTicketsMVC.Services.Interfaces;

namespace TurboTicketsMVC.Controllers
{
    public class CompaniesController : TTBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ITTCompanyService _companyService;
        private readonly ITTRolesService _rolesService;

        public CompaniesController(ApplicationDbContext context,
                                   ITTCompanyService companyService,
                                   ITTRolesService rolesService)
        {
            _context = context;
            _companyService = companyService;
            _rolesService = rolesService;
        }

        //// GET: Companies
        //public async Task<IActionResult> Index()
        //{
        //      return _context.Companies != null ? 
        //                  View(await _context.Companies.ToListAsync()) :
        //                  Problem("Entity set 'ApplicationDbContext.Companies'  is null.");
        //}

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageUserRoles()
        {
            //add an inst of viewModel as a list(model)
            List<ManageUserRolesViewModel> model = new List<ManageUserRolesViewModel>();
            //get companyId
            int? companyId = _companyId;
            //get all companyUsers
            IEnumerable<TTUser> companyUsers = await _companyService.GetMembersAsync(companyId);
            IEnumerable<IdentityRole> prodRoles = await _rolesService.GetProdRoles();
            //loop over users to populate the model.
            foreach (TTUser user in companyUsers)
            {
                //more comprehensive than user.Id == _userId
                if (string.Compare(user.Id, _userId) != 0)
                {

                    ManageUserRolesViewModel viewModel = new ManageUserRolesViewModel();
                    IEnumerable<string?> currentRoles = await _rolesService.GetUserRolesAsync(user);

                    if (currentRoles!.Contains(nameof(TTRoles.DemoUser)))
                    {
                        viewModel.IsDemoUser = true;
                    }
                    else
                    {
                        viewModel.IsDemoUser = false;
                    }
                    if (currentRoles != null)
                    {
                        viewModel.SelectedRole = currentRoles.FirstOrDefault(r => r != nameof(TTRoles.DemoUser));
                    }
                    viewModel.TTUser = user;
                    viewModel.Roles = new SelectList(prodRoles, "Name", "Name", viewModel.SelectedRole);
                    model.Add(viewModel);
                }
            }

            // loop over users to populate 
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel viewModel)
        {
            //get companyid
            //instantiate user
            TTUser? ttUser = (await _companyService.GetMembersAsync(_companyId)).FirstOrDefault(u => u.Id == viewModel.TTUser?.Id);
            //get roles of user
            IEnumerable<string>? currentRoles = await _rolesService.GetUserRolesAsync(ttUser);
            //get selected roles of user
            List<string> selectedRoles = new();
            selectedRoles.Add(viewModel.SelectedRole!);
            if (viewModel.IsDemoUser)
            {
                selectedRoles.Add(nameof(TTRoles.DemoUser));
            }


            if (selectedRoles!.Count() > 0)
            {
                if (await _rolesService.RemoveUserFromRolesAsync(ttUser, currentRoles)) //boolean return
                {
                    foreach (string selectedRole in selectedRoles)
                    {
                        await _rolesService.AddUserToRoleAsync(ttUser, selectedRole);
                    }
                }
            }

            return RedirectToAction(nameof(ManageUserRoles));
            //save changes
            //navigate
        }
        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // GET: Companies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Companies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,ImageFormData,ImageFormType")] Company company)
        {
            if (ModelState.IsValid)
            {
                _context.Add(company);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        // GET: Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        // POST: Companies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ImageFormData,ImageFormType")] Company company)
        {
            if (id != company.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.Id))
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
            return View(company);
        }

        // GET: Companies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Companies == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Companies'  is null.");
            }
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
            return (_context.Companies?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
