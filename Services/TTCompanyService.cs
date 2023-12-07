using Microsoft.EntityFrameworkCore;
using TurboTicketsMVC.Data;
using TurboTicketsMVC.Models;
using TurboTicketsMVC.Services.Interfaces;

namespace TurboTicketsMVC.Services
{
    public class TTCompanyService : ITTCompanyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITTProjectService _projectService;
        private readonly ITTInviteService _inviteService;

        public TTCompanyService(ApplicationDbContext context,
                                ITTProjectService projectService,
                                ITTInviteService inviteService)
        {
            _inviteService = inviteService;
            _projectService = projectService;
            _context = context;
        }
        public async Task<Company> GetCompanyInfoAsync(int? companyId)
        {
            try
            {
                Company? company = new Company();
                if (companyId != null)
                {
                    company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == companyId);
                }
                return company!;
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

        public async Task<IEnumerable<Project>> GetProjectsAsync(int? companyId)
        {
            try
            {
                IEnumerable<Project> projects = Enumerable.Empty<Project>();
                if (companyId != null)
                {
                    projects = await _projectService.GetProjectsByCompanyIdAsync(companyId);
                }
                return projects;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Invite>> GetInvitesAsync(int? companyId)
        {
            try
            {
				IEnumerable<Invite> invites = Enumerable.Empty<Invite>();
				if (companyId != null)
				{
					invites = await _context.Invites
										.Where(i => i.CompanyId == companyId)
										.ToListAsync();
				}
				return invites;
			}
            catch (Exception)
            {

                throw;
            }
        }
    }
}
