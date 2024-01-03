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
    public class ProjectsController : TTBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ITTFileService _fileService;
        private readonly ITTProjectService _projectService;
        private readonly ITTRolesService _roleService;
        private readonly ITTNotificationService _notificationService;
        private readonly UserManager<TTUser> _userManager;
        public ProjectsController(ApplicationDbContext context,
                                  ITTFileService fileService,
                                  ITTProjectService projectService,
                                  ITTRolesService roleService,
                                  ITTNotificationService notificationService,
                                  UserManager<TTUser> userManager)
        {
            _fileService = fileService;
            _context = context;
            _projectService = projectService;
            _roleService = roleService;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        // GET: Projects
        public async Task<IActionResult> Index(string? swalMessage)
        {
            if (!string.IsNullOrEmpty(swalMessage))
            {
                ViewData["SwalMessage"] = swalMessage;
            }
            IEnumerable<Project> projects = (await _projectService.GetUserProjectsAsync(_userId))!;
            return View(projects);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AllProjects()
        {
            IEnumerable<Project> allProjects = (await _projectService.GetAllProjectsByCompanyIdAsync(_companyId))!;
            return View(allProjects);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> ArchivedProjects()
        {
            IEnumerable<Project> allProjects = (await _projectService.GetAllProjectsByCompanyIdAsync(_companyId))!;
            IEnumerable<Project> archivedProjects = allProjects.Where(p => p.Archived).ToList();
            return View(archivedProjects);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> UnarchivedProjects()
        {
            IEnumerable<Project> allProjects = (await _projectService.GetAllProjectsByCompanyIdAsync(_companyId))!;
            IEnumerable<Project> unarchivedProjects = allProjects.Where(p => p.Archived == false).ToList();
            return View(unarchivedProjects);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AssignedProjects()
        {
            IEnumerable<Project> assignedProjects = await _projectService.GetAssignedProjects(_companyId);
            return View(assignedProjects);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> UnassignedProjects()
        {
            IEnumerable<Project> unassignedProjects = await _projectService.GetUnassignedProjects(_companyId);
            return View(unassignedProjects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectByIdAsync(id, _companyId);
            bool canViewProject = await _projectService.CanViewProject(id, _userId!, _companyId);

            if (project == null)
            {
                return NotFound();
            }

            if (canViewProject)
            {
                IEnumerable<TTUser> projectManagers = await _roleService.GetUsersInRoleAsync(nameof(TTRoles.ProjectManager), _companyId);
                IEnumerable<TTUser> developers = await _roleService.GetUsersInRoleAsync(nameof(TTRoles.Developer), _companyId);
                IEnumerable<TTUser> submitters = await _roleService.GetUsersInRoleAsync(nameof(TTRoles.Submitter), _companyId);
                IEnumerable<TTUser> selectedDevelopers = await _projectService.GetProjectMembersByRoleAsync(project.Id, nameof(TTRoles.Developer), _companyId);
                IEnumerable<TTUser> selectedSubmitters = await _projectService.GetProjectMembersByRoleAsync(project.Id, nameof(TTRoles.Submitter), _companyId);
                TTUser? projectManager = await _projectService.GetProjectManagerAsync(project.Id);
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
                return View(project);
            }
            return NotFound();
        }


        [Authorize(Roles = "Admin, ProjectManager")]
        // GET: Projects/Create
        public async Task<IActionResult> Create()
        {
            IEnumerable<TTUser> projectManagers = await _roleService.GetUsersInRoleAsync(nameof(TTRoles.ProjectManager), _companyId);

            ViewData["ProjectManagers"] = new SelectList(projectManagers, "Id", "FullName");

            return View();
        }


        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, ProjectManager")]

        public async Task<IActionResult> Create([Bind("Id, Name,Description,ProjectPriority,ImageFormFile,Archived, StartDate, EndDate")] Project project, string? selectedProjectManagerId)
        {
            if (ModelState.IsValid)
            {
                project.CompanyId = User.Identity!.GetCompanyId();
                project.CreatedDate = DateTimeOffset.Now;

                if (project.ImageFormFile != null)
                {
                    project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                    project.ImageFileType = project.ImageFormFile.ContentType;
                }
                await _projectService.AddProjectAsync(project);

                bool pmAdded = false;
                bool projectNotificationConfirmation = false;
                bool pmNotificationConfirmation = false;

                if (selectedProjectManagerId == "(Project Creator)" && User.IsInRole("ProjectManager"))
                {
                    pmAdded = await _projectService.AddProjectManagerAsync(_userId, project.Id);
                }
                else
                {
                    pmAdded = await _projectService.AddProjectManagerAsync(selectedProjectManagerId, project.Id);
                }

                if (pmAdded)
                {
                    projectNotificationConfirmation = await _notificationService.ProjectUpdateNotificationAsync(project.Id, _userId, nameof(TTProjectNotificationTypes.NewProject));
                    pmNotificationConfirmation = await _notificationService.ProjectUpdateNotificationAsync(project.Id, _userId, nameof(TTProjectNotificationTypes.AssignedProject));
                }

                if (pmNotificationConfirmation && projectNotificationConfirmation)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            return View(project);
        }


        //POST: EditTeam
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> EditTeam(
            IEnumerable<string> DeveloperIds,
            IEnumerable<string> SubmitterIds,
            string ProjectManagerId,
            int? projectId,
            string? redirect
            )
        {
            TTUser? user = await _userManager.GetUserAsync(User);
            bool isAdmin = await _roleService.IsUserInRoleAsync(user, "Admin");
            bool isUserPm = await _projectService.IsUserPmAsync(projectId, _userId!);

            if (isAdmin || isUserPm)
            {
                TTUser? oldPM = await _projectService.GetProjectManagerAsync(projectId);
                bool pmChanged = false;
                bool pmAssigned = oldPM == null && ProjectManagerId != null;
                if (oldPM != null && ProjectManagerId != null)
                {
                    pmChanged = oldPM.Id != ProjectManagerId;
                }
                await _projectService.RemoveMembersFromProjectAsync(projectId, _companyId);
                await _projectService.AddProjectManagerAsync(ProjectManagerId, projectId);
                await _projectService.AddMembersToProjectAsync(DeveloperIds, projectId, _companyId);
                await _projectService.AddMembersToProjectAsync(SubmitterIds, projectId, _companyId);

                if (pmChanged || pmAssigned)
                {
                    await _notificationService.ProjectUpdateNotificationAsync(projectId, _userId, nameof(TTProjectNotificationTypes.AssignedProject));
                }
                bool teamChangeNotification = await _notificationService.ProjectUpdateNotificationAsync(projectId, _userId, nameof(TTProjectNotificationTypes.TeamChanged));
                if (teamChangeNotification)
                {
                    return RedirectToAction(redirect, new { id = projectId });
                }
                else
                {
                    string? swalMessage = "Action failed, something went wrong";
                    return RedirectToAction(nameof(Index), new { swalMessage });
                }
            }
            else
            {
                return NotFound();
            }
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectByIdAsync(id, _companyId);
            if (project == null)
            {
                return NotFound();
            }

            bool isUserPm = false;
            bool isAdmin = false;

            if (id != null)
            {
                isUserPm = await _projectService.IsUserPmAsync(id, _userId!);
                TTUser? user = await _userManager.GetUserAsync(User);
                isAdmin = await _roleService.IsUserInRoleAsync(user, "Admin");
            }

            if (isAdmin || isUserPm)
            {
                //Viewbag for editing the team
                IEnumerable<TTUser> projectManagers = await _roleService.GetUsersInRoleAsync(nameof(TTRoles.ProjectManager), _companyId);
                IEnumerable<TTUser> developers = await _roleService.GetUsersInRoleAsync(nameof(TTRoles.Developer), _companyId);
                IEnumerable<TTUser> submitters = await _roleService.GetUsersInRoleAsync(nameof(TTRoles.Submitter), _companyId);
                IEnumerable<TTUser> selectedDevelopers = await _projectService.GetProjectMembersByRoleAsync(project.Id, nameof(TTRoles.Developer), _companyId);
                IEnumerable<TTUser> selectedSubmitters = await _projectService.GetProjectMembersByRoleAsync(project.Id, nameof(TTRoles.Submitter), _companyId);
                TTUser? projectManager = await _projectService.GetProjectManagerAsync(project.Id);
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
                return View(project);
            }
            else
            {
                return NotFound();
            }
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, ProjectManager")]

        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,CreatedDate,StartDate,EndDate,ProjectPriority,ImageFormFile,Archived")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }


            bool isUserPm = await _projectService.IsUserPmAsync(project.Id, _userId!);
            TTUser? user = await _userManager.GetUserAsync(User);
            bool isAdmin = await _roleService.IsUserInRoleAsync(user, "Admin");

            if (isUserPm || isAdmin)
            {

                if (ModelState.IsValid)
                {
                    try
                    {
                        if (project.ImageFormFile != null)
                        {
                            project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                            project.ImageFileType = project.ImageFormFile.ContentType;
                        }

                        await _projectService.UpdateProjectAsync(project);
                        bool updateNotification = await _notificationService.ProjectUpdateNotificationAsync(id, _userId, nameof(TTProjectNotificationTypes.UpdateProject));
                        string? swalMessage = "Action succeeded";
                        if (updateNotification)
                        {
                            return RedirectToAction(nameof(Index), new { swalMessage });
                        }
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ProjectExists(project.Id))
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
                ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
                return View(project);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignPM(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectByIdAsync(id, _companyId);

            if (project == null)
            {
                return NotFound();
            }

            //list of PMs for company
            IEnumerable<TTUser> projectManagers = await _roleService.GetUsersInRoleAsync(nameof(TTRoles.ProjectManager), _companyId);
            //current pm if exists
            TTUser? currentPM = await _projectService.GetProjectManagerAsync(id);
            AssignPMViewModel viewModel = new()
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id),
                PMId = currentPM?.Id
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> AssignPM(AssignPMViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel.PMId))
            {
                if (await _projectService.AddProjectManagerAsync(viewModel.PMId, viewModel.ProjectId))
                {
                    return RedirectToAction(nameof(Details), new { id = viewModel.ProjectId });
                }
                else
                {
                    ModelState.AddModelError("PMId", "Error assigning PM.");
                    return View(viewModel);
                }
            }

            ModelState.AddModelError("PMId", "No Project Manager chosen. Please select a PM.");
            IEnumerable<TTUser> projectManagers = await _roleService.GetUsersInRoleAsync(nameof(TTRoles.ProjectManager), _companyId);
            TTUser? currentPM = await _projectService.GetProjectManagerAsync(viewModel.ProjectId);
            viewModel.PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id);
            return View(viewModel);
        }
        // GET: Projects/Archive/5
        [Authorize(Roles = "Admin, ProjectManager")]

        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            Project project = await _projectService.GetProjectByIdAsync(id, _companyId);
            bool isUserPm = await _projectService.IsUserPmAsync(project.Id, _userId!);
            TTUser? user = await _userManager.GetUserAsync(User);
            bool isAdmin = await _roleService.IsUserInRoleAsync(user, "Admin");
            if (project == null)
            {
                return NotFound();
            }

            if (isAdmin || isUserPm)
            {
                return View(project);
            }
            else
            {
                return NotFound();
            }
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("ArchiveConfirmed")]
        [Authorize(Roles = "Admin, ProjectManager")]

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }
            Project project = await _projectService.GetProjectByIdAsync(id, _companyId);
            bool isUserPm = await _projectService.IsUserPmAsync(project.Id, _userId!);
            TTUser? user = await _userManager.GetUserAsync(User);
            bool isAdmin = await _roleService.IsUserInRoleAsync(user, "Admin");

            if (project != null)
            {
                await _projectService.ArchiveProjectAsync(project, _companyId);
            }

            if (isAdmin || isUserPm)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return NotFound();
            }
        }


        // GET: Projects/Restore/5
        [Authorize(Roles = "Admin, ProjectManager")]

        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            Project project = await _projectService.GetProjectByIdAsync(id, _companyId);
            bool isUserPm = await _projectService.IsUserPmAsync(project.Id, _userId!);
            TTUser? user = await _userManager.GetUserAsync(User);
            bool isAdmin = await _roleService.IsUserInRoleAsync(user, "Admin");

            if (project == null)
            {
                return NotFound();
            }

            if (isAdmin || isUserPm)
            {
                return View(project);
            }
            else
            {
                return NotFound();
            }
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("RestoreConfirmed")]
        [Authorize(Roles = "Admin, ProjectManager")]

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }
            Project project = await _projectService.GetProjectByIdAsync(id, _companyId);
            bool isUserPm = await _projectService.IsUserPmAsync(project.Id, _userId!);
            TTUser? user = await _userManager.GetUserAsync(User);
            bool isAdmin = await _roleService.IsUserInRoleAsync(user, "Admin");

            if (project != null)
            {
                await _projectService.RestoreProjectAsync(project, _companyId);
            }

            if (isUserPm || isAdmin)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return NotFound();
            }

        }


        [HttpGet]
        public async Task<IActionResult> MarkRead(int id)
        {
            if (id != null)
            {
                Notification? notification = await _notificationService.GetNotificationAsync(id);
                await _notificationService.MarkNotificationRead(notification);
                return RedirectToAction(nameof(Details), new { id = notification.ProjectId });
            }
            else
            {
                return NotFound();
            }
        }

        private bool ProjectExists(int id)
        {
            return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
