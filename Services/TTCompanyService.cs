using Microsoft.EntityFrameworkCore;
using TurboTicketsMVC.Data;
using TurboTicketsMVC.Models;
using TurboTicketsMVC.Services.Interfaces;

namespace TurboTicketsMVC.Services
{
    public class TTCompanyService : ITTCompanyService
    {
        private readonly ApplicationDbContext _context;

        public TTCompanyService(ApplicationDbContext context)
        {
            _context = context;
        }
        public Task<Company> GetCompanyInfoAsync(int? companyId)
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

        public async Task<IEnumerable<TTUser>> GetMembersAsync(int? companyId)
        {
            try
            {
                IEnumerable<TTUser> companyMembers = Enumerable.Empty<TTUser>();
                
                if (companyId != null)
                {
                    companyMembers = await _context.Users.Where(u => u.CompanyId == companyId)                                                       
                                                          .ToListAsync();
                }

                return companyMembers;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<IEnumerable<Project>> GetProjectsAsync(int? companyId)
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

        public Task<IEnumerable<Invite>> GetInvitesAsync(int? companyId)
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
