using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TurboTicketsMVC.Data;
using TurboTicketsMVC.Models;
using Microsoft.AspNetCore.Authorization;
using TurboTicketsMVC.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using TurboTicketsMVC.Extensions;
using System.ComponentModel.Design;
using Microsoft.CodeAnalysis;

namespace TurboTicketsMVC.Controllers
{
    public class InvitesController : TTBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ITTProjectService _projectService;
        private readonly IDataProtector _protector;
        private readonly ITTCompanyService _companyService;
        private readonly IEmailSender _emailService;
        private readonly UserManager<TTUser> _userManager;
        private readonly ITTInviteService _inviteService;

        public InvitesController(ApplicationDbContext context,
                                 ITTProjectService projectService,
                                 IDataProtectionProvider dataProtectionProvider,
                                 ITTCompanyService companyService,
                                 IEmailSender emailSender,
                                 UserManager<TTUser> userManager,
                                 ITTInviteService inviteService)
        {
            _context = context;
            _projectService = projectService;
            _protector = dataProtectionProvider.CreateProtector("CF.StaRLink.BugTr@cker.2022");
            _companyService = companyService;
            _emailService = emailSender;
            _userManager = userManager;
            _inviteService = inviteService;
        }

        // GET: Invites
        [Authorize]
        public async Task<IActionResult> Index(string? swalMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(swalMessage))
                {
                    ViewData["SwalMessage"] = swalMessage;
                }
                IEnumerable<Invite> companyInvites = await _inviteService.GetCompanyInvites(_companyId);
                return View(companyInvites);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return RedirectToAction("GenericError", "Home");

            }
        }

        // GET: Invites/Create
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Create()
        {
            try
            {
                int? companyId = User.Identity!.GetCompanyId();
                IEnumerable<Models.Project> companyProjects = await _projectService.GetProjectsByCompanyIdAsync(companyId);

                ViewData["Projects"] = new SelectList(companyProjects, "Id", "Name");
                return View();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return RedirectToAction("GenericError", "Home");

            }
        }

        // POST: Invites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Create([Bind("Id,ProjectId,InviteeEmail,InviteeFirstName,InviteeLastName,Message")] Invite invite)
        {
            ModelState.Remove("InvitorId");

            if (ModelState.IsValid)
            {
                try
                {
                    //encrypt code for invite
                    Guid guid = Guid.NewGuid();

                    //create callbackURL attributes
                    string token = _protector.Protect(guid.ToString());
                    string email = _protector.Protect(invite.InviteeEmail!);
                    string company = _protector.Protect(_companyId.ToString()!);



                    // Save invite in the DB
                    invite.CompanyToken = guid;
                    invite.CompanyId = _companyId;
                    invite.InviteDate = DateTimeOffset.Now;
                    invite.InvitorId = _userId;
                    invite.IsValid = true;

                    // Add Invite service method for "AddNewInviteAsync"
                    if (await _inviteService.AddNewInviteAsync(invite))
                    {
                        string? callbackUrl = Url.Action("ProcessInvite", "Invites", new { token, email, company }, protocol: Request.Scheme);


                        string body = $@"{invite.Message} <br />
                       
                       Please click the following link to join our team. <br />
                       <a href=""{callbackUrl}"">Collaborate</a>";

                        string? destination = invite.InviteeEmail;

                        Company ttCompany = await _companyService.GetCompanyInfoAsync(_companyId);

                        string? subject = $"Turbo Tickets: {ttCompany.Name} Invite";

                        await _emailService.SendEmailAsync(destination!, subject, body);
                        string? swalMessage = "Invite sent";
                        return RedirectToAction(nameof(Index), new { swalMessage });
                    }
                    else
                    {
                        string? swalMessage = "Invite failed, please try another email address";
                        return RedirectToAction(nameof(Index), new { swalMessage });
                    }



                    // TODO: Possibly use SWAL message

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                    return RedirectToAction("GenericError", "Home");

                }
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", invite.CompanyId);
            ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
            ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", invite.ProjectId);
            return View(invite);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> InvalidateInvite(int? id)
        {
            try
            {
                if (id != null)
                {
                    Invite? invite = await _inviteService.GetInviteByIdAsync(id, _companyId);
                    if (invite != null)
                    {
                        await _inviteService.InvalidateExistingCompanyInvites(invite);
                        string? swalMessage = "Invalidation successful";

                        return RedirectToAction(nameof(Index), new { swalMessage });
                    }
                    else
                    {
                        string? swalMessage = "Invalidation failed";
                        return RedirectToAction(nameof(Index), new { swalMessage });
                    }
                }
                else
                {
                    return NotFound();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return RedirectToAction("GenericError", "Home");

            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> ResendInvite(int? id)
        {

            try
            {
                Invite? invite = await _inviteService.GetInviteByIdAsync(id, _companyId);

                if (invite != null)
                {
                    //encrypt code for invite
                    Guid guid = Guid.NewGuid();

                    //create callbackURL attributes
                    string token = _protector.Protect(guid.ToString());
                    string email = _protector.Protect(invite.InviteeEmail!);
                    string company = _protector.Protect(_companyId.ToString()!);

                    // Save invite in the DB
                    Invite newInvite = new();
                    newInvite.CompanyToken = guid;
                    newInvite.CompanyId = _companyId;
                    newInvite.InviteDate = DateTimeOffset.Now;
                    newInvite.InvitorId = _userId;
                    newInvite.IsValid = true;
                    newInvite.InviteeEmail = invite.InviteeEmail;
                    newInvite.InviteeFirstName = invite.InviteeFirstName;
                    newInvite.InviteeLastName = invite.InviteeLastName;
                    newInvite.ProjectId = invite.ProjectId;
                    newInvite.Message = invite.Message;

                    // Add Invite service method for "AddNewInviteAsync"
                    if (await _inviteService.AddNewInviteAsync(newInvite))
                    {
                        string? callbackUrl = Url.Action("ProcessInvite", "Invites", new { token, email, company }, protocol: Request.Scheme);


                        string body = $@"{invite.Message} <br />
                       
                       Please click the following link to join our team. <br />
                       <a href=""{callbackUrl}"">Collaborate</a>";

                        string? destination = invite.InviteeEmail;

                        Company ttCompany = await _companyService.GetCompanyInfoAsync(_companyId);

                        string? subject = $"Turbo Tickets: {ttCompany.Name} Invite";

                        await _emailService.SendEmailAsync(destination!, subject, body);
                        return RedirectToAction(nameof(Index), new { swalMessage = "Invite sent" });
                    }
                    else
                    {
                        return RedirectToAction(nameof(Index), new { swalMessage = "Invite failed, please try another email address" });
                    }

                }
                else
                {
                    return RedirectToAction(nameof(Index), new { swalMessage = "Invite failed, please try another email address" });
                }
            }
            catch
            {
                return NotFound();
            }

        }



        [HttpGet]
        [AllowAnonymous]
        public ViewResult InvalidInvite()
        {
            return View();
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ProcessInvite(string token, string email, string company)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(company))
            {
                return NotFound();
            }

            Guid companyToken = Guid.Parse(_protector.Unprotect(token));
            string? inviteeEmail = _protector.Unprotect(email);
            int companyId = int.Parse(_protector.Unprotect(company));

            try
            {
                Invite? invite = await _inviteService.GetInviteByTokenAsync(companyToken, inviteeEmail, companyId);

                if (invite != null)
                {
                    if (await _inviteService.ValidateInviteCodeAsync(companyToken))
                    {
                        return View(invite);
                    }
                    else
                    {
                        return RedirectToAction(nameof(InvalidInvite));
                    }
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return RedirectToAction("GenericError", "Home");

            }

        }

    }
}
