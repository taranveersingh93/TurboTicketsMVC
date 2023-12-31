﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using TurboTicketsMVC.Data;
using TurboTicketsMVC.Models;
using TurboTicketsMVC.Models.Enums;
using TurboTicketsMVC.Services.Interfaces;

namespace TurboTicketsMVC.Services
{
    public class TTProjectService : ITTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITTRolesService _roleService;
        private readonly UserManager<TTUser> _userManager;

        public TTProjectService(ApplicationDbContext context,
                                ITTRolesService roleService,
                                UserManager<TTUser> userManager)
        {
            _roleService = roleService;
            _context = context;
            _userManager = userManager;
        }

        public async Task AddProjectAsync(Project project)
        {
            try
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public async Task<bool> AddMemberToProjectAsync(TTUser? member, int? projectId)
        {
            try
            {
                if (member != null && projectId != null)
                {
                    Project? project = await GetProjectByIdAsync(projectId, member.CompanyId);

                    if (project != null)
                    {
                        //project should "include" members
                        bool isOnProject = project.Members.Any(m => m.Id == member.Id);

                        if (!isOnProject)
                        {
                            project.Members.Add(member);
                            await UpdateProjectAsync(project);
                            return true;
                        }
                    }
                }
                return false;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public async Task AddMembersToProjectAsync(IEnumerable<string>? userIds, int? projectId, int? companyId)
        {
            try
            {
                if (userIds != null && projectId != null && companyId != null)
                {
                    Project? project = await GetProjectByIdAsync(projectId, companyId);

                    if (project != null)
                    {
                        foreach (string userId in userIds)
                        {
                            bool isOnProject = project.Members.Any(m => m.Id == userId);
                            TTUser? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                            if (!isOnProject && user != null)
                            {
                                project.Members.Add(user);
                                await _context.SaveChangesAsync();
                            }
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public async Task<bool> AddProjectManagerAsync(string? userId, int? projectId)
        {
            try
            {
                if (userId != null && projectId != null)
                {
                    TTUser? currentPM = await GetProjectManagerAsync(projectId);
                    TTUser? selectedPM = await _context.Users.FindAsync(userId);

                    //remove current
                    if (currentPM != null)
                    {
                        await RemoveProjectManagerAsync(projectId);
                    }

                    //add new PM 
                    try
                    {
                        if (await AddMemberToProjectAsync(selectedPM!, projectId))
                        {
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        throw;
                    }
                }
                return false;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public async Task ArchiveProjectAsync(Project? project, int? companyId)
        {
            try
            {
                if (project != null && companyId != null && project.CompanyId == companyId)
                {
                    project.Archived = true;
                    foreach (Ticket ticket in project.Tickets)
                    {
                        ticket.ArchivedByProject = true;
                    }
                    await UpdateProjectAsync(project);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public async Task RestoreProjectAsync(Project? project, int? companyId)
        {
            try
            {
                if (project != null && companyId != null && project.CompanyId == companyId)
                {
                    project.Archived = false;
                    foreach (Ticket ticket in project.Tickets)
                    {
                        ticket.ArchivedByProject = false;
                    }
                    await UpdateProjectAsync(project);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public async Task<IEnumerable<Project>> GetAllProjectsByCompanyIdAsync(int? companyId)
        {
            try
            {
                IEnumerable<Project> companyProjects = await _context.Projects
                                                       .Include(p => p.Tickets.Where(t => t.ArchivedByProject == false && t.Archived == false))
                                                       .Include(p => p.Members)
                                                      .Where(p => p.CompanyId == companyId).ToListAsync();
                return companyProjects;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }

        public async Task<IEnumerable<Project>> GetProjectsByCompanyIdAsync(int? companyId)
        {
            try
            {
                IEnumerable<Project> companyProjects = Enumerable.Empty<Project>();
                if (companyId != null)
                {
                    companyProjects = await _context.Projects.Include(p => p.Tickets.Where(t => t.ArchivedByProject == false && t.Archived == false))
                                                            .Include(p => p.Members)
                                                          .Where(p => p.CompanyId == companyId && p.Archived == false).ToListAsync();

                }
                return companyProjects;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public Task<IEnumerable<Project>> GetArchivedProjectsByCompanyIdAsync(int? companyId)
        {
            try
            {
                throw new NotImplementedException();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public async Task<Project> GetProjectByIdAsync(int? projectId, int? companyId)
        {
            try
            {
                Project? project = new Project();
                if (projectId != null && companyId != null)
                {
                    project = await _context.Projects
                    .Include(p => p.Tickets)
                        .ThenInclude(t => t.DeveloperUser)
                    .Include(p => p.Tickets)
                        .ThenInclude(t => t.SubmitterUser)
                    .Include(p => p.Tickets)
                        .ThenInclude(t => t.History)
                            .ThenInclude(h => h.User)
                    .Include(p => p.Members)
                    .Include(p => p.Company)
                    .FirstOrDefaultAsync(project => project.Id == projectId && project.CompanyId == companyId);
                }
                return project!;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public async Task<TTUser> GetProjectManagerAsync(int? projectId)
        {
            try
            {
                Project? project = await _context.Projects.AsNoTracking().Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);

                if (project != null)
                {
                    foreach (TTUser user in project.Members)
                    {
                        if (await _roleService.IsUserInRoleAsync(user, nameof(TTRoles.ProjectManager)))
                        {
                            return user;
                        }
                    }
                }
                return null!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public async Task<IEnumerable<TTUser>> GetProjectMembersByRoleAsync(int? projectId, string? roleName, int? companyId)
        {
            try
            {
                IEnumerable<TTUser> users = Enumerable.Empty<TTUser>();
                Project project = await GetProjectByIdAsync(projectId, companyId);
                if (project != null && roleName != null && companyId != null)
                {
                    IEnumerable<TTUser> allUsers = await _roleService.GetUsersInRoleAsync(roleName, companyId);
                    users = allUsers.Where(u => project.Members.Contains(u)).ToList();
                }
                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }

        public async Task<IEnumerable<Project>?> GetUserProjectsAsync(string? userId)
        {
            try
            {
                TTUser? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                IEnumerable<Project> companyProjects = await GetProjectsByCompanyIdAsync(user!.CompanyId);
                IEnumerable<Project> userProjects = Enumerable.Empty<Project>();
                if (await _roleService.IsUserInRoleAsync(user, nameof(TTRoles.Admin)))
                {
                    return companyProjects;
                }
                else
                {
                    userProjects = companyProjects.Where(p => p.Members.Contains(user)).ToList();
                }
                return userProjects;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public async Task RemoveMembersFromProjectAsync(int? projectId, int? companyId)
        {
            try
            {
                if (projectId != null && companyId != null)
                {
                    Project project = await GetProjectByIdAsync(projectId, companyId);
                    project.Members = new Collection<TTUser>();
                    await UpdateProjectAsync(project);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int? projectId)
        {
            try
            {
                if (projectId != null)
                {
                    Project? project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);
                    if (project != null)
                    {
                        foreach (TTUser user in project.Members)
                        {
                            bool isProjectManager = await _roleService.IsUserInRoleAsync(user, nameof(TTRoles.ProjectManager));
                            if (isProjectManager)
                            {
                                await RemoveMemberFromProjectAsync(user, projectId);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }

        public async Task<bool> RemoveMemberFromProjectAsync(TTUser? member, int? projectId)
        {
            try
            {
                if (projectId != null && member != null)
                {
                    Project? project = await GetProjectByIdAsync(projectId, member.CompanyId);
                    if (project != null)
                    {
                        bool isOnProject = project.Members.Any(m => m.Id == member.Id);

                        if (isOnProject)
                        {
                            project.Members.Remove(member);
                            await _context.SaveChangesAsync();
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public async Task UpdateProjectAsync(Project? project)
        {
            try
            {
                if (project != null)
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }

        public async Task<bool> IsUserPmAsync(int? projectId, string userId)
        {
            try
            {
                if (projectId != null)
                {
                    TTUser? projectPM = await GetProjectManagerAsync(projectId);
                    if (projectPM != null)
                    {
                        return projectPM.Id == userId;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }

        public async Task<bool> CanViewProject(int? projectId, string userId, int? companyId)
        {
            try
            {

                if (projectId != null && companyId != null)
                {
                    Project? project = await _context.Projects.AsNoTracking()
                        .Include(p => p.Tickets)
                            .ThenInclude(t => t.DeveloperUser)
                        .Include(p => p.Tickets)
                            .ThenInclude(t => t.SubmitterUser)
                        .Include(p => p.Tickets)
                            .ThenInclude(t => t.History)
                                .ThenInclude(h => h.User)
                        .Include(p => p.Members)
                    .Include(p => p.Company)
                        .FirstOrDefaultAsync(project => project.Id == projectId && project.CompanyId == companyId);

                    TTUser? user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                    bool isAdmin = await _roleService.IsUserInRoleAsync(user, nameof(TTRoles.Admin));
                    bool isPM = await _roleService.IsUserInRoleAsync(user, nameof(TTRoles.ProjectManager));
                    bool isMember = false;
                    if (project != null)
                    {
                        isMember = project!.Members.Any(m => m.Id == userId);
                    }
                    return isAdmin || isMember || isPM;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }

        public async Task<IEnumerable<Project>> GetAssignedProjects(int? companyId)
        {
            try
            {
                IEnumerable<Project> allProjects = await GetAllProjectsByCompanyIdAsync(companyId);
                List<Project> assignedProjects = new();
                foreach (Project project in allProjects)
                {
                    if ((await GetProjectManagerAsync(project.Id)) != null)
                    {
                        assignedProjects.Add(project);
                    }
                }
                IEnumerable<Project> projects = assignedProjects;
                return projects;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }

        public async Task<IEnumerable<Project>> GetUnassignedProjects(int? companyId)
        {
            try
            {
                IEnumerable<Project> allProjects = await GetAllProjectsByCompanyIdAsync(companyId);
                List<Project> unassignedProjects = new();
                foreach (Project project in allProjects)
                {
                    if ((await GetProjectManagerAsync(project.Id)) == null)
                    {
                        unassignedProjects.Add(project);
                    }
                }
                IEnumerable<Project> projects = unassignedProjects;
                return projects;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
    }
}

