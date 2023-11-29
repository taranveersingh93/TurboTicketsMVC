using TurboTicketsMVC.Models;

namespace TurboTicketsMVC.Services.Interfaces
{
    public interface ITTCompanyService
    {
        public Task<Company> GetCompanyInfoAsync(int? companyId);

        public Task<IEnumerable<TTUser>> GetMembersAsync(int? companyId);

        public Task<IEnumerable<Project>> GetProjectsAsync(int? companyId);

        public Task<IEnumerable<Invite>> GetInvitesAsync(int? companyId);
    }
}
