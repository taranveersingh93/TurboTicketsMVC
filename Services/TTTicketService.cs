using TurboTicketsMVC.Data;
using TurboTicketsMVC.Models;
using TurboTicketsMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace TurboTicketsMVC.Services
{
    public class TTTicketService:ITTTicketService
    {
        private readonly ApplicationDbContext _context; 
        public TTTicketService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddTicketAsync(Ticket? ticket) {

            try
            {
                if (ticket != null)
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }


            }
            catch (Exception)
            {

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
            catch (Exception)
            {

                throw;
            }

        }
        public Task AddTicketAttachmentAsync(TicketAttachment? ticketAttachment) {
            try
            {
                                throw new NotImplementedException();


            }
            catch (Exception)
            {

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
            catch (Exception)
            {

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
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<IEnumerable<Ticket>> GetAllTicketsByCompanyIdAsync(int? companyId) {

            try
            {
                IEnumerable<Ticket> companyTickets = await _context.Tickets
                            .Include(t => t.DeveloperUser)
                            .Include(t => t.Project)
                            .Include(t => t.SubmitterUser)
                            .Where(t => t.Project!.CompanyId == companyId).ToListAsync();
                return companyTickets;


            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task<Ticket> GetTicketAsNoTrackingAsync(int? ticketId, int? companyId) {

            try
            {
                                throw new NotImplementedException();


            }
            catch (Exception)
            {

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
	                            .Include(t => t.SubmitterUser)
                                .Include(t => t.Comments)
                                    .ThenInclude(c => c.User)
	                            .FirstOrDefaultAsync(t => t.Id == ticketId && t.Project!.CompanyId == companyId);
                }
                return ticket!;
			}
            catch (Exception)
            {

                throw;
            }
        }
        public Task<TicketAttachment?> GetTicketAttachmentByIdAsync(int? ticketAttachmentId) {
            try
            {
                                throw new NotImplementedException();


            }
            catch (Exception)
            {

                throw;
            }

        }
        public Task<TTUser?> GetTicketDeveloperAsync(int? ticketId, int? companyId) {
            try
            {
                                throw new NotImplementedException();


            }
            catch (Exception)
            {

                throw;
            }

        }

        public Task<List<Ticket>> GetTicketsByUserIdAsync(string? userId, int? companyId) {
            try
            {
                                throw new NotImplementedException();


            }
            catch (Exception)
            {

                throw;
            }

        }
        public Task<IEnumerable<TicketPriority>> GetTicketPrioritiesAsync() {
            try
            {
                                throw new NotImplementedException();


            }
            catch (Exception)
            {

                throw;
            }

        }
        public Task<IEnumerable<TicketStatus>> GetTicketStatusesAsync() {

            try
            {
                                throw new NotImplementedException();


            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task<IEnumerable<TicketType>> GetTicketTypesAsync() {
            try
            {
                                throw new NotImplementedException();


            }
            catch (Exception)
            {

                throw;
            }

        }

        public Task ArchiveTicketAsync(Ticket? ticket) {

            try
            {
                                throw new NotImplementedException();


            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task RestoreTicketAsync(Ticket? ticket) {
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
