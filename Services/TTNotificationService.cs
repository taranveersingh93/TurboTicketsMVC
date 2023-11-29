using TurboTicketsMVC.Models.Enums;
using TurboTicketsMVC.Models;
using TurboTicketsMVC.Services.Interfaces;

namespace TurboTicketsMVC.Services
{
    public class TTNotificationService:ITTNotificationService
    {
        public Task AddNotificationAsync(Notification? notification) {
            try
            {
                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task NotificationsByRoleAsync(int? companyId, Notification? notification, TTRoles role) {
            try
            {
                                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<List<Notification>> GetNotificationsByUserIdAsync(string? userId) {
            try
            {
                                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<bool> NewDeveloperNotificationAsync(int? ticketId, string? developerId, string? senderId) {
            try
            {
                                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<bool> NewTicketNotificationAsync(int? ticketId, string? senderId) {
            try
            {
                                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<bool> SendEmailNotificationByRoleAsync(int? companyId, Notification? notification, TTRoles role) {
            try
            {
                                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<bool> SendEmailNotificationAsync(Notification? notification, string? emailSubject) {
            try
            {
                                throw new NotImplementedException();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<bool> TicketUpdateNotificationAsync(int? ticketId, string? developerId, string? senderId = null) {
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
