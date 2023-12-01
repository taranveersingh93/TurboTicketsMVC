using Microsoft.EntityFrameworkCore;
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
        public TTProjectService(ApplicationDbContext context,
                                ITTRolesService roleService)
        {
            _roleService = roleService;
            _context = context;
        }

        public async Task AddProjectAsync(Project project)
        {
            try
            {             
                _context.Add(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

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
                            await _context.SaveChangesAsync();
                            return true;
                        }
                    }
                }
                return false;

            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task AddMembersToProjectAsync(IEnumerable<string>? userIds, int? projectId, int? companyId)
        {
            try
            {
                throw new NotImplementedException();

            }
            catch (Exception)
            {

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
                    catch (Exception)
                    {
                        throw;
                    }
                }
                return false;

            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task ArchiveProjectAsync(Project? project, int? companyId)
        {
            try
            {
                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<IEnumerable<Project>> GetAllProjectsByCompanyIdAsync(int? companyId)
        {
            try
            {
                IEnumerable<Project> companyProjects = await _context.Projects
                                                       .Include(p => p.Tickets)
                                                      .Where(p => p.CompanyId == companyId).ToListAsync();
                return companyProjects;

            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int? companyId)
        {
            try
            {
                throw new NotImplementedException();

            }
            catch (Exception)
            {

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
                    .Include(p => p.Members)
                    .FirstOrDefaultAsync(project => project.Id == projectId && project.CompanyId == companyId);
                }
                return project!;

            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<TTUser> GetProjectManagerAsync(int? projectId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);

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
            catch (Exception)
            {

                throw;
            }
        }
        public Task<List<TTUser>> GetProjectMembersByRoleAsync(int? projectId, string? roleName, int? companyId)
        {
            try
            {
                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<IEnumerable<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            try
            {
                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task<List<Project>?> GetUserProjectsAsync(string? userId)
        {
            try
            {
                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task RemoveMembersFromProjectAsync(int? projectId, int? companyId)
        {
            try
            {
                throw new NotImplementedException();

            }
            catch (Exception)
            {

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
            catch (Exception)
            {

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
            catch (Exception)
            {

                throw;
            }
        }
        public Task RestoreProjectAsync(Project? project, int? companyId)
        {
            try
            {
                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task UpdateProjectAsync(Project? project)
        {
            try
            {
                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
//were you sending in applications on linkedin or company websites?
//when you had your first interview, how fluent you were with explaining or demoing your code?
//After graduating, were you still adding functionalities to your app?

