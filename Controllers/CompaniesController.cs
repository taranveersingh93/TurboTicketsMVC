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
            try
            {
                List<ManageUserRolesViewModel> model = new List<ManageUserRolesViewModel>();
                //get all companyUsers
                IEnumerable<TTUser> companyUsers = await _companyService.GetMembersAsync(_companyId);
                IEnumerable<IdentityRole> prodRoles = await _rolesService.GetProdRoles();
                //loop over users to populate the model.
                foreach (TTUser user in companyUsers)
                {
                    //all users that aren't the logged in user
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

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("GenericError", "Home");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel viewModel)
        {
            try
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

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("GenericError", "Home");
            }

        }

        // GET: Companies/Details
        [Authorize]
        public async Task<IActionResult> Details(string? swalMessage)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("GenericError", "Home");
            }
        }

        // GET: Companies/Edit
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("GenericError", "Home");
            }

        }

        // POST: Companies/Edit
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Company? company)
        {
            try
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

                        return NotFound();

                    }
                }
                return RedirectToAction(nameof(Details), new { swalMessage });

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("GenericError", "Home");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> UserProfile(string? email)
        {
            try
            {
                if (email == null)
                {
                    return NotFound();
                }
                TTUser? user = await _companyService.GetUserByEmail(email, _companyId);
                return View(user);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("GenericError", "Home");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EmailUser(string? email)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("GenericError", "Home");
            }
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("GenericError", "Home");
            }
        }
    }
}
