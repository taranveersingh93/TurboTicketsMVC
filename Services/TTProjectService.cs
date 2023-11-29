using TurboTicketsMVC.Models;
using TurboTicketsMVC.Services.Interfaces;

namespace TurboTicketsMVC.Services
{
    public class TTProjectService:ITTProjectService
    {
        public Task AddProjectAsync(Project project) {
            try
            {
                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task<bool> AddMemberToProjectAsync(TTUser? member, int? projectId) {
            try
            {
                                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task AddMembersToProjectAsync(IEnumerable<string>? userIds, int? projectId, int? companyId) {
            try
            {
                                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<bool> AddProjectManagerAsync(string? userId, int? projectId) {
            try
            {
                                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task ArchiveProjectAsync(Project? project, int? companyId) {
            try
            {
                                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task<List<Project>> GetAllProjectsByCompanyIdAsync(int? companyId) {
            try
            {
                                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int? companyId) {
            try
            {
                                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task<Project> GetProjectByIdAsync(int? projectId, int? companyId) {
            try
            {
                                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task<TTUser> GetProjectManagerAsync(int? projectId) {
            try
            {
                                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task<List<TTUser>> GetProjectMembersByRoleAsync(int? projectId, string? roleName, int? companyId) {
            try
            {
                                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<IEnumerable<ProjectPriority>> GetProjectPrioritiesAsync() {
            try
            {
                                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task<List<Project>?> GetUserProjectsAsync(string? userId) {
            try
            {
                                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task RemoveMembersFromProjectAsync(int? projectId, int? companyId) {
            try
            {
                                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task RemoveProjectManagerAsync(int? projectId) {
            try
            {
                                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task<bool> RemoveMemberFromProjectAsync(TTUser? member, int? projectId) {
            try
            {
                                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task RestoreProjectAsync(Project? project, int? companyId) {
            try
            {
                                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task UpdateProjectAsync(Project? project) {
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
