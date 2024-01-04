using TurboTicketsMVC.Models.Enums;
using TurboTicketsMVC.Models;

namespace TurboTicketsMVC.Services.Interfaces
{
    public interface ITTNotificationService
    {
        public Task AddNotificationAsync(Notification? notification);

        public Task NotificationsByRoleAsync(int? companyId, Notification? notification, string? role);

        public Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(string? userId);


        public Task<bool> SendEmailNotificationByRoleAsync(int? companyId, Notification? notification, string? role);

        public Task<bool> SendEmailNotificationAsync(Notification? notification, string? emailSubject);

        public Task<bool> TicketUpdateNotificationAsync(int? ticketId, string? ticketUserId, string? ticketNotificationType, string? assignerId);
        public Task<bool> ProjectUpdateNotificationAsync(int? projectId, string? projectUserId, string projectNotificationType);
        public Task MarkNotificationRead(Notification? notification);
        public Task<Notification> GetNotificationAsync(int? id);

        public Task<bool> NotifyDeveloper(Ticket? ticket, TTUser? ticketUser);

        public Task<bool> NotifyDeveloperOfAssignment(Notification? notification, TTUser? ticketUser);

        public Task MarkAllNotificationsRead(string? userId);
        public Task MarkAllNotificationsUnread(string? userId);

    }
}
