using TurboTicketsMVC.Models;

namespace TurboTicketsMVC.Services.Interfaces
{
    public interface ITTProjectService
    {
        public Task AddProjectAsync(Project project);
        public Task<bool> AddMemberToProjectAsync(TTUser? member, int? projectId);
        public Task AddMembersToProjectAsync(IEnumerable<string>? userIds, int? projectId, int? companyId);

        public Task<bool> AddProjectManagerAsync(string? userId, int? projectId);
        public Task ArchiveProjectAsync(Project? project, int? companyId);
        public Task RestoreProjectAsync(Project? project, int? companyId);
        public Task<IEnumerable<Project>> GetAllProjectsByCompanyIdAsync(int? companyId);
        public Task<IEnumerable<Project>> GetProjectsByCompanyIdAsync(int? companyId);
        public Task<IEnumerable<Project>> GetArchivedProjectsByCompanyIdAsync(int? companyId);
        public Task<Project> GetProjectByIdAsync(int? projectId, int? companyId);
        public Task<TTUser> GetProjectManagerAsync(int? projectId);
        public Task<IEnumerable<TTUser>> GetProjectMembersByRoleAsync(int? projectId, string? roleName, int? companyId);

        public Task<IEnumerable<Project>?> GetUserProjectsAsync(string? userId);
        public Task RemoveMembersFromProjectAsync(int? projectId, int? companyId);

        public Task RemoveProjectManagerAsync(int? projectId);
        public Task<bool> RemoveMemberFromProjectAsync(TTUser? member, int? projectId);
        public Task UpdateProjectAsync(Project? project);
        public Task<bool> IsUserPmAsync(int? projectId, string userId);
        public Task<bool> CanViewProject(int? projectId, string userId, int? companyId);
        public Task<IEnumerable<Project>> GetAssignedProjects(int? companyId);
        public Task<IEnumerable<Project>> GetUnassignedProjects(int? companyId);

    }
}
