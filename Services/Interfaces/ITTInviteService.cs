using TurboTicketsMVC.Models;

namespace TurboTicketsMVC.Services.Interfaces
{
    public interface ITTInviteService
    {
        public Task<IEnumerable<Invite>> GetCompanyInvites(int? companyId);
        public Task<bool> AcceptInviteAsync(Guid? token, string? userId, int companyId);

        public Task<bool> AddNewInviteAsync(Invite? invite);

        public Task<bool> AnyInviteAsync(Guid token, string? email, int? companyId);
        public Task<Invite> GetInviteByTokenAsync(Guid token, string? email, int? companyId);

        public Task<Invite?> GetInviteByIdAsync(int? inviteId, int? companyId);


        public Task<bool> ValidateInviteCodeAsync(Guid? token);
        public Task InvalidateExistingCompanyInvites(Invite invite);

        public Task<bool> IsInviteEmailValid(Invite invite);


    }
}
