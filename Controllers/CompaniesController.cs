using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TurboTicketsMVC.Data;
using TurboTicketsMVC.Models;
using TurboTicketsMVC.Models.Enums;
using TurboTicketsMVC.Models.ViewModels;
using TurboTicketsMVC.Services;
using TurboTicketsMVC.Services.Interfaces;

namespace TurboTicketsMVC.Controllers
{
    public class CompaniesController : TTBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ITTCompanyService _companyService;
        private readonly ITTRolesService _rolesService;
        private readonly IEmailSender _emailService;
        private readonly UserManager<TTUser> _userManager;
        public CompaniesController(ApplicationDbContext context,
                                   ITTCompanyService companyService,
                                   ITTRolesService rolesService,
                                   IEmailSender emailService,
                                   UserManager<TTUser> userManager
                                  )
        {
            _context = context;
            _companyService = companyService;
            _rolesService = rolesService;
            _emailService = emailService;
            _userManager = userManager;
        }

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
        
        // GET: Companies/Details
        [Authorize]
        public async Task<IActionResult> Details(string? swalMessage)
        {
            Company? company = await _companyService.GetCompanyInfoAsync(_companyId);

            if (company == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(swalMessage))
            {

                ViewData["SwalMessage"] = swalMessage;
            }
            return View(company);
        }

        // GET: Companies/Edit
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }
            if (id == _companyId)
            {
                Company company = await _companyService.GetCompanyInfoAsync(_companyId);

                if (company == null)
                {
                    return NotFound();
                }
                return View(company);
            }
            return NotFound();

        }

        // POST: Companies/Edit
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Company? company)
        {
            string? swalMessage = "Changes failed";
            if (ModelState.IsValid && company != null)
            {

                try
                {
                    await _companyService.UpdateCompanyAsync(company);
                    swalMessage = "Changes successful";

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
            }
            return RedirectToAction(nameof(Details), new { swalMessage });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> UserProfile(string? email)
        {
            if (email == null)
            {
                return NotFound();
            }
            TTUser? user = await _companyService.GetUserByEmail(email, _companyId);
            return View(user);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EmailUser(string? email)
        {
            if (email == null)
            {
                return NotFound();
            }
            TTUser? user = await _companyService.GetUserByEmail(email, _companyId);
            if (user != null && user.CompanyId == _companyId)
            {
                EmailData emailData = new()
                {
                    EmailAddress = email,
                    EmailBody = "",
                    EmailSubject = "",
                    Recipient = user!.FullName
                };
                return View(emailData);

            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> EmailUser(EmailData emailData)
        {
            try
            {
                string? swalMessage = string.Empty;
                if (ModelState.IsValid)
                {
                    string? signature = (await _userManager.GetUserAsync(User))!.FullName;
                    string? subject = emailData.EmailSubject;
                    subject += $" - sent by: {signature}";
                    string? recipientEmail = emailData.EmailAddress;
                    string? body = emailData.EmailBody;
                    await _emailService.SendEmailAsync(recipientEmail!, subject!, body!);
                    swalMessage = "Success! Email sent.";
                }
                else
                {
                    swalMessage = "Email failed. Something went wrong";
                }


                return RedirectToAction(nameof(Details), new { swalMessage });
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
