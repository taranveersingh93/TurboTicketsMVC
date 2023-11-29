using TurboTicketsMVC.Models;
using TurboTicketsMVC.Services.Interfaces;

namespace TurboTicketsMVC.Services
{
    public class TTTicketService:ITTTicketService
    {
        public Task AddTicketAsync(Ticket? ticket) {

            try
            {
                throw new NotImplementedException();


            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task AssignTicketAsync(int? ticketId, string? userId) {
            try
            {
                                throw new NotImplementedException();


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
        public Task AddTicketCommentAsync(TicketComment? ticketComment) {
            try
            {
                                throw new NotImplementedException();


            }
            catch (Exception)
            {

                throw;
            }

        }

        public Task UpdateTicketAsync(Ticket? ticket) {

            try
            {
                                throw new NotImplementedException();


            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int? companyId) {

            try
            {
                                throw new NotImplementedException();


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
        public Task<Ticket> GetTicketByIdAsync(int? ticketId, int? companyId) {

            try
            {
                                throw new NotImplementedException();


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
