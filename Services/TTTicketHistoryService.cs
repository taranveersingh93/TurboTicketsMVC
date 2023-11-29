using System.Collections.Generic;
using TurboTicketsMVC.Models;
using TurboTicketsMVC.Services.Interfaces;

namespace TurboTicketsMVC.Services
{
    public class TTTicketHistoryService:ITTTicketHistoryService
    {
        public Task AddHistoryAsync(Ticket? oldTicket, Ticket? newTicket, string? userId) {
            try
            {
                                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task AddHistoryAsync(int? ticketId, string? model, string? userId) {
            try
            {
                                throw new NotImplementedException();


            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int? projectId, int? companyId) {
            try
            {
                                throw new NotImplementedException();


            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int? companyId) {
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
