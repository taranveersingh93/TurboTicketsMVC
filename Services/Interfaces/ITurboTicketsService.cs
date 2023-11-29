using TurboTicketsMVC.Models;

namespace TurboTicketsMVC.Services.Interfaces
{
    public interface ITurboTicketsService
    {
        public Task<IEnumerable<Company>> GetAllCompaniesAsync();
        public Task<IEnumerable<Ticket>> GetTicketsByCompanyAsync(int companyId);
        public Task<IEnumerable<Project>> GetProjectsByCompanyAsync(int companyId);
        public Task<IEnumerable<TTUser>> GetUsersByCompanyAsync(int companyId);

    }
}
