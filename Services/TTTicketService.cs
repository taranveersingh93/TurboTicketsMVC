using TurboTicketsMVC.Data;
using TurboTicketsMVC.Models;
using TurboTicketsMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TurboTicketsMVC.Models.Enums;

namespace TurboTicketsMVC.Services
{
    public class TTTicketService:ITTTicketService
    {
        private readonly ApplicationDbContext _context; 
        private readonly ITTProjectService _projectService;
        private readonly UserManager<TTUser> _userManager;
        private readonly ITTRolesService _roleService;
        public TTTicketService(ApplicationDbContext context,
                                ITTProjectService projectService,
                                UserManager<TTUser> userManager,
                                ITTRolesService roleService)
        {
            _context = context;
            _projectService = projectService;
            _userManager = userManager;
            _roleService = roleService;
        }

        public async Task AddTicketAsync(Ticket? ticket) {

            try
            {
                if (ticket != null)
                {
                    _context.Add(ticket);
                    await _context.SaveChangesAsync();
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public async Task AssignTicketAsync(int? ticketId, string? userId) {
            try
            {
               if (ticketId != null && !string.IsNullOrEmpty(userId))
                {
                    TTUser? developer = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                    Ticket ticket = await GetTicketByIdAsync(ticketId, developer!.CompanyId);

                    if (ticket != null)
                    {
                        ticket.DeveloperUserId = userId;
                        await UpdateTicketAsync(ticket);
                    }
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }

        }
        public async Task AddTicketAttachmentAsync(TicketAttachment? ticketAttachment) {
            try
            {
                if (ticketAttachment != null)
                {
				await _context.AddAsync(ticketAttachment);
				await _context.SaveChangesAsync();
                }
			}
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task RemoveTicketAttachmentAsync(TicketAttachment? ticketAttachment)
        {
            try
            {
                if (ticketAttachment != null)
                {
                    _context.Remove(ticketAttachment);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }

        }
        public async Task AddTicketCommentAsync(TicketComment? ticketComment) {
            try
            {
                if (ticketComment != null)
                {
                    _context.Add(ticketComment);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }

        }

        public async Task UpdateTicketAsync(Ticket? ticket) {

            try
            {
               if (ticket != null)
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public async Task<IEnumerable<Ticket>> GetAllTicketsByCompanyIdAsync(int? companyId)
        {

            try
            {
                IEnumerable<Ticket> companyTickets = await _context.Tickets
                            .Include(t => t.DeveloperUser)
                            .Include(t => t.History)
                                .ThenInclude(h => h.User)
                            .Include(t => t.Project)
                                .ThenInclude(p => p.Members)
                            .Include(t => t.SubmitterUser)
                            .Where(t => t.Project!.CompanyId == companyId).ToListAsync();
                return companyTickets;


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public async Task<IEnumerable<Ticket>> GetTicketsByCompanyIdAsync(int? companyId) {

            try
            {
                IEnumerable<Ticket> companyTickets = await _context.Tickets
                            .Include(t => t.DeveloperUser)
                            .Include(t => t.History)
                                .ThenInclude(h => h.User)
                            .Include(t => t.Project)
                                .ThenInclude(p => p.Members)
                            .Include(t => t.SubmitterUser)
                            .Where(t => t.Project!.CompanyId == companyId && t.ArchivedByProject == false && t.Archived == false ).ToListAsync();
                return companyTickets;


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public async Task<Ticket> GetTicketAsNoTrackingAsync(int? ticketId, int? companyId) {

            try
            {
                Ticket? ticket = new Ticket();
                if (ticketId != null && companyId != null)
                {
                ticket = await _context.Tickets
                                  .Include(t => t.DeveloperUser)
                                  .Include(t => t.Project)
                                  .Include(t => t.History)
                                    .ThenInclude(h => h.User)
                                  .Include(t => t.SubmitterUser)
                                  .Include(t => t.Attachments)
                                    .ThenInclude(a => a.TTUser)
                                  .Include(t => t.Comments)
                                      .ThenInclude(c => c.User)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync(t => t.Id == ticketId && t.Project!.CompanyId == companyId);
                }
                return ticket!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public async Task<Ticket> GetTicketByIdAsync(int? ticketId, int? companyId) {

            try
            {
                Ticket? ticket = new Ticket();
                if (ticketId != null && companyId != null)
                {
				ticket = await _context.Tickets
	                            .Include(t => t.DeveloperUser)
	                            .Include(t => t.Project)
								.Include(t => t.History.OrderByDescending(h => h.CreatedDate))
                                    .ThenInclude(h => h.User)
								.Include(t => t.SubmitterUser)
                                .Include(t => t.Attachments)
                                    .ThenInclude(a => a.TTUser)
                                .Include(t => t.Comments)
                                    .ThenInclude(c => c.User)
	                            .FirstOrDefaultAsync(t => t.Id == ticketId && t.Project!.CompanyId == companyId);
                }
                return ticket!;
			}
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public async Task<TicketAttachment?> GetTicketAttachmentByIdAsync(int? ticketAttachmentId) {
            try
            {
				TicketAttachment? ticketAttachment = await _context.TicketAttachments
													  .Include(t => t.TTUser)
													  .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
				return ticketAttachment;

			}
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }

        }

        public async Task<IEnumerable<Ticket>> GetTicketsByUserIdAsync(string? userId, int? companyId) {
            try
            {

                IEnumerable<Ticket> companyTickets = await GetTicketsByCompanyIdAsync(companyId); 
                if (userId != null && companyId != null)
                {
					IEnumerable<Ticket> userTickets = Enumerable.Empty<Ticket>();
					TTUser user = (await _context.Users.FirstOrDefaultAsync(u => u.Id == userId))!;
					bool isAdmin = await _roleService.IsUserInRoleAsync(user, nameof(TTRoles.Admin));
					bool isProjectManager = await _roleService.IsUserInRoleAsync(user, nameof(TTRoles.ProjectManager));
					bool isDeveloper = await _roleService.IsUserInRoleAsync(user, nameof(TTRoles.Developer));
					bool isSubmitter = await _roleService.IsUserInRoleAsync(user, nameof(TTRoles.Submitter));
                    
                    if (isAdmin)
                    {
                        userTickets = companyTickets;
                    } else if(isProjectManager)
                    {
                        userTickets = companyTickets.Where(t => t.Project!.Members.Contains(user));
                    } else if(isDeveloper)
                    {
                        userTickets = companyTickets.Where(t => t.DeveloperUserId == userId || t.SubmitterUserId == userId);
                    } else if(isSubmitter)
                    {
                        userTickets = companyTickets.Where(t => t.SubmitterUserId == userId);
                    }
                    return userTickets;
                }
                return companyTickets;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }

        }

        public async Task<IEnumerable<Ticket>> GetTicketsByPMIdAsync(string? userId, int? companyId)
        {
            try
            {
            TTUser user = (await _context.Users.FirstOrDefaultAsync(u => u.Id == userId))!;

            IEnumerable<Ticket> companyTickets = await GetAllTicketsByCompanyIdAsync(companyId);
            IEnumerable<Ticket> userTickets = companyTickets.Where(t => t.Project!.Members.Contains(user));

            return userTickets;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }

        }


        public async Task ArchiveTicketAsync(Ticket? ticket) {

            try
            {
                if (ticket != null)
                {
                    ticket.Archived = true;
                    await UpdateTicketAsync(ticket);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public async Task RestoreTicketAsync(Ticket? ticket) {
            try
            {
                if (ticket != null)
                {
                    ticket.Archived = false;
                    await UpdateTicketAsync(ticket);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }

        }

        public async Task<bool> CanAssignDeveloper(string? userId, int? ticketId, int? companyId)
        {
            try
            {
                bool isUserAuthorized = false;
                TTUser? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                Ticket? ticket = ticket = await GetTicketAsNoTrackingAsync(ticketId, companyId);
                TTUser? projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);
                bool isProjectManager = user?.Id == projectManager?.Id;
                bool isAdmin = await _roleService.IsUserInRoleAsync(user, nameof(TTRoles.Admin));

				if ( isProjectManager|| isAdmin)
                {
                    isUserAuthorized = true;
                }
                return isUserAuthorized;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }

        public async Task<bool> CanActOnTicket(string? userId, int? ticketId, int? companyId)
        {
            try
            {
                bool canActOnTicket = false;
                TTUser? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                Ticket? ticket = await GetTicketAsNoTrackingAsync(ticketId, companyId);
                bool isProjectManager = await _projectService.IsUserPmAsync(ticket.ProjectId, userId!);
                bool isAdmin = await _roleService.IsUserInRoleAsync(user, nameof(TTRoles.Admin));
                bool isDeveloper = ticket.DeveloperUserId == userId;
                bool isSubmitter = ticket.SubmitterUserId == userId;
                if (isAdmin || isDeveloper || isProjectManager || isSubmitter) {
                    canActOnTicket = true;
                }
                return canActOnTicket;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public async Task<bool> CanMakeTickets(string? userId, int? projectId, int? companyId)
        {
            try
            {
                bool canMakeTickets = false;
                if (projectId != null)
                {
                    TTUser? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                    Project project = await _projectService.GetProjectByIdAsync(projectId, companyId);
                    bool isAdmin = await _roleService.IsUserInRoleAsync(user, nameof(TTRoles.Admin));
                    bool isMember = project.Members.Any(m => m.Id == userId);
                    if (isAdmin || isMember)
                    {
                        canMakeTickets = true;
                    }
                }
                return canMakeTickets;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public async Task ChangeTicketStatus(int? ticketId,int? companyId, TTTicketStatuses status)
        {
            try
            {
            if(ticketId != null && companyId != null)
            {
                Ticket? ticket = await GetTicketByIdAsync(ticketId, companyId);
                ticket.TicketStatus = status;
                await UpdateTicketAsync(ticket);
            }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public async Task ResolveTicketAsync(Ticket? ticket)
        {
            try
            {
                if (ticket != null)
                {
                    ticket.TicketStatus = TTTicketStatuses.Resolved;
                    await UpdateTicketAsync(ticket);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }

        }
    }
}
