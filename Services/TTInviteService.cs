using TurboTicketsMVC.Models;
using TurboTicketsMVC.Services.Interfaces;

namespace TurboTicketsMVC.Services
{
    public class TTInviteService:ITTInviteService
    {
        public Task<bool> AcceptInviteAsync(Guid? token, string? userId) {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task AddNewInviteAsync(Invite? invite) {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<bool> AnyInviteAsync(Guid? token, string? email, int? companyId) {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<Invite?> GetInviteAsync(int? inviteId, int? companyId) {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<Invite?> GetInviteAsync(Guid? token, string? email, int? companyId) {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<bool> ValidateInviteCodeAsync(Guid? token) {
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
