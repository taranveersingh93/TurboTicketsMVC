using TurboTicketsMVC.Models;

namespace TurboTicketsMVC.Services.Interfaces
{
    public interface ITTTicketService
    {
        public Task AddTicketAsync(Ticket? ticket);
        public Task AssignTicketAsync(int? ticketId, string? userId);
        public Task AddTicketAttachmentAsync(TicketAttachment? ticketAttachment);
        public Task AddTicketCommentAsync(TicketComment? ticketComment);

        public Task UpdateTicketAsync(Ticket? ticket);
        public Task<IEnumerable<Ticket>> GetAllTicketsByCompanyIdAsync(int? companyId);
        public Task<Ticket> GetTicketAsNoTrackingAsync(int? ticketId, int? companyId);
        public Task<Ticket> GetTicketByIdAsync(int? ticketId, int? companyId);
        public Task<TicketAttachment?> GetTicketAttachmentByIdAsync(int? ticketAttachmentId);
        public Task<TTUser?> GetTicketDeveloperAsync(int? ticketId, int? companyId);

        public Task<List<Ticket>> GetTicketsByUserIdAsync(string? userId, int? companyId);
        public Task<IEnumerable<TicketPriority>> GetTicketPrioritiesAsync();
        public Task<IEnumerable<TicketStatus>> GetTicketStatusesAsync();
        public Task<IEnumerable<TicketType>> GetTicketTypesAsync();

		public Task ArchiveTicketAsync(Ticket? ticket);
        public Task RestoreTicketAsync(Ticket? ticket);
    }
}
