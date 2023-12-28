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
        private readonly ITTFileService _fileService;

        public TTCompanyService(ApplicationDbContext context,
                                ITTProjectService projectService,
                                ITTInviteService inviteService,
                                ITTFileService fileService)
        {
            _inviteService = inviteService;
            _projectService = projectService;
            _context = context;
            _fileService = fileService;
        }
        public async Task<Company> GetCompanyInfoAsync(int? companyId)
        {
            try
            {
                Company? company = new Company();
                if (companyId != null)
                {
                    company = await _context.Companies.Include(c => c.Members).FirstOrDefaultAsync(c => c.Id == companyId);
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
        public async Task<TTUser?> GetUserByEmail(string? email, int? companyId)
        {
            try
            {
                if (email == null || companyId == null)
                {
                    return null;
                } else
                {
                    TTUser? user = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == email && u.CompanyId == companyId);
                    return user;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateCompanyAsync(Company? company)
        {
            try
            {
                if(company != null)
                {
                    if (company.ImageFormFile != null)
                    {
                        company.ImageFormData = await _fileService.ConvertFileToByteArrayAsync(company.ImageFormFile);
                        company.ImageFormType = company.ImageFormFile.ContentType;
                    }
                    _context.Update(company);
                   await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
