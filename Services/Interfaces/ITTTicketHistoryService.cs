using TurboTicketsMVC.Models;

namespace TurboTicketsMVC.Services.Interfaces
{
    public interface ITTTicketHistoryService
    {
        public Task AddHistoryAsync(Ticket? oldTicket, Ticket? newTicket, string? userId);

        public Task AddHistoryAsync(int? ticketId, string? model, string? userId);

        public Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int? projectId, int? companyId);

        public Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int? companyId);
    }
}
