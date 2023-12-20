using TurboTicketsMVC.Models.Enums;
using TurboTicketsMVC.Models;

namespace TurboTicketsMVC.Services.Interfaces
{
    public interface ITTNotificationService
    {
        public Task AddNotificationAsync(Notification? notification);

        public Task NotificationsByRoleAsync(int? companyId, Notification? notification, string? role);

        public Task<List<Notification>> GetNotificationsByUserIdAsync(string? userId);


        public Task<bool> SendEmailNotificationByRoleAsync(int? companyId, Notification? notification, string? role);

        public Task<bool> SendEmailNotificationAsync(Notification? notification, string? emailSubject);

        public Task<bool> TicketUpdateNotificationAsync(int? ticketId, string? ticketUserId, string? ticketNotificationType);
        public Task<bool> ProjectUpdateNotificationAsync(int? projectId, string? projectUserId, string projectNotificationType);

    }
}
