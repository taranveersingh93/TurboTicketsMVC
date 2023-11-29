using TurboTicketsMVC.Models;

namespace TurboTicketsMVC.Services.Interfaces
{
    public interface ITurboTicketsService
    {
        public Task<IEnumerable<Company>> GetAllCompaniesAsync();
        public Task<IEnumerable<Ticket>> GetTicketsByCompanyAsync(int companyId);
    }
}
