using TurboTicketsMVC.Models;

namespace TurboTicketsMVC.Services.Interfaces
{
    public interface ITTTicketHistoryService
    {
        public Task AddHistoryAsync(Ticket? oldTicket, Ticket? newTicket, string? userId);

        public Task AddHistoryAsync(int? ticketId, string? model, string? userId);

        public Task<IEnumerable<TicketHistory>> GetProjectTicketsHistoriesAsync(int? projectId, int? companyId);

        public Task<IEnumerable<TicketHistory>> GetCompanyTicketsHistoriesAsync(int? companyId);
    }
}
