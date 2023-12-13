﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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

        public ProjectsController(ApplicationDbContext context,
                                  ITTFileService fileService,
                                  ITTProjectService projectService,
                                  ITTRolesService roleService)
        {
            _fileService = fileService;
            _context = context;
            _projectService = projectService;
            _roleService = roleService;
        }

        [Authorize]
        // GET: Projects
        public async Task<IActionResult> Index()
        {
            IEnumerable<Project> projects = (await _projectService.GetProjectsByCompanyIdAsync(_companyId))!;
            return View(projects);
        }

        public async Task<IActionResult> MyProjects()
        {
            IEnumerable<Project> userProjects = (await _projectService.GetUserProjectsAsync(_userId))!;
            return View(userProjects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Remember that the _context should not be used directly in the controller so....     

            // Edit the following code to use the service layer. 
            // Your goal is to return the 'project' from the databse
            // with the Id equal to the parameter passed in.               
            // This is the only modification necessary for this method/action.     

            Project? project = await _projectService.GetProjectByIdAsync(id, _companyId);


            if (project == null)
            {
                return NotFound();
            }
            IEnumerable<TTUser> projectManagers = await _roleService.GetUsersInRoleAsync(nameof(TTRoles.ProjectManager), _companyId);
            IEnumerable<TTUser> developers = await _roleService.GetUsersInRoleAsync(nameof(TTRoles.Developer), _companyId);
            IEnumerable<TTUser> submitters = await _roleService.GetUsersInRoleAsync(nameof(TTRoles.Submitter), _companyId);
            IEnumerable<TTUser> selectedDevelopers = await _projectService.GetProjectMembersByRoleAsync(project.Id, nameof(TTRoles.Developer), _companyId);
            IEnumerable<TTUser> selectedSubmitters = await _projectService.GetProjectMembersByRoleAsync(project.Id, nameof(TTRoles.Submitter), _companyId);
            TTUser projectManager = await _projectService.GetProjectManagerAsync(project.Id);
            string projectManagerId = projectManager.Id;
            IEnumerable<string> developerIds = selectedDevelopers.Select(d => d.Id);
            IEnumerable<string> submitterIds = selectedDevelopers.Select(d => d.Id);
            

            ViewData["ProjectManagers"] = new SelectList(projectManagers, "Id", "FullName", projectManagerId);
            ViewData["Developers"] = new MultiSelectList(developers, "Id", "FullName", developerIds);
            ViewData["Submitters"] = new MultiSelectList(submitters, "Id", "FullName", submitterIds);
            return View(project);
        }
        [Authorize(Roles = "Admin, ProjectManager")]


        // GET: Projects/Create
        public IActionResult Create()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name");
            return View();
        }


        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, ProjectManager")]

        public async Task<IActionResult> Create([Bind("Id, Name,Description,ProjectPriority,ImageFormFile,Archived")] Project project)
        {
            if (ModelState.IsValid)
            {
                project.CompanyId = User.Identity!.GetCompanyId();
                project.CreatedDate = DateTimeOffset.Now;
                project.StartDate = DateTimeOffset.Now;
                project.EndDate = DateTimeOffset.Now;
                if (project.ImageFormFile != null)
                {
                    project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                    project.ImageFileType = project.ImageFormFile.ContentType;
                }
                await _projectService.AddProjectAsync(project);
                bool pmAdded = await _projectService.AddProjectManagerAsync(_userId, project.Id);
                if (pmAdded)
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
        [Authorize(Roles ="Admin,ProjectManager")]
        public async Task<IActionResult> EditTeam(
            IEnumerable<string> DeveloperIds,
            IEnumerable<string> SubmitterIds,
            string ProjectManagerId,
            int? projectId)
        {
            await _projectService.RemoveMembersFromProjectAsync(projectId, _companyId);
            await _projectService.AddProjectManagerAsync(ProjectManagerId, projectId);
            await _projectService.AddMembersToProjectAsync(DeveloperIds, projectId, _companyId);
            await _projectService.AddMembersToProjectAsync(SubmitterIds, projectId, _companyId);
            return RedirectToAction(nameof(Details), new {id = projectId});
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin, ProjectManager")]

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            return View(project);
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

            if (ModelState.IsValid)
            {
                try
                {
                    if (project.ImageFormFile != null)
                    {
                        project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                        project.ImageFileType = project.ImageFormFile.ContentType;
                    }
                    _context.Update(project);
                    await _context.SaveChangesAsync();
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

        [HttpGet]
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
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            Project project = await _projectService.GetProjectByIdAsync(id, _companyId);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("ArchiveConfirmed")]
        [Authorize(Roles = "Admin")]

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }
            Project project = await _projectService.GetProjectByIdAsync(id, _companyId);
            if (project != null)
            {
                await _projectService.ArchiveProjectAsync(project, _companyId);
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: Projects/Restore/5
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            Project project = await _projectService.GetProjectByIdAsync(id, _companyId);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("RestoreConfirmed")]
        [Authorize(Roles = "Admin")]

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }
            Project project = await _projectService.GetProjectByIdAsync(id, _companyId);
            if (project != null)
            {
                await _projectService.RestoreProjectAsync(project, _companyId);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
