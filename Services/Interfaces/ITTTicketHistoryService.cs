using TurboTicketsMVC.Models;

namespace TurboTicketsMVC.Services.Interfaces
{
    public interface ITTTicketHistoryService
    {
        public Task AddHistoryAsync(Ticket? oldTicket, Ticket? newTicket, string? userId);

        public Task<IEnumerable<TicketHistory>> GetProjectTicketsHistoriesAsync(int? projectId, int? companyId);

        public Task<IEnumerable<TicketHistory>> GetCompanyTicketsHistoriesAsync(int? companyId);

        public Task<IEnumerable<TicketHistory>> GetUserTicketsHistoriesAsync(int? companyId, string? userId);

    }
}
