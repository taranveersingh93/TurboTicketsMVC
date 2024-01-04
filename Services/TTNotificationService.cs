using TurboTicketsMVC.Models.Enums;
using TurboTicketsMVC.Models;
using TurboTicketsMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using TurboTicketsMVC.Data;
using Microsoft.EntityFrameworkCore;


namespace TurboTicketsMVC.Services
{
    public class TTNotificationService : ITTNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailService;
        private readonly ITTRolesService _rolesService;
        private readonly ITTProjectService _projectService;
        private readonly UserManager<TTUser> _userManager;

        public TTNotificationService(ApplicationDbContext context,
                                     IEmailSender emailService,
                                     ITTRolesService rolesService,
                                     UserManager<TTUser> userManager,
                                     ITTProjectService projectService)
        {
            _context = context;
            _emailService = emailService;
            _rolesService = rolesService;
            _userManager = userManager;
            _projectService = projectService;
        }
        //add a notification to the db
        public async Task AddNotificationAsync(Notification? notification)
        {
            try
            {
                if (notification != null)
                {
                    await _context.AddAsync(notification);
                    await _context.SaveChangesAsync();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }

        //discuss naming convention
        public async Task NotificationsByRoleAsync(int? companyId, Notification? notification, string? role)
        {
            try
            {
                if (notification != null)
                {
                    IEnumerable<string> memberIds = (await _rolesService.GetUsersInRoleAsync(role, companyId))!.Select(u => u.Id);

                    foreach (string memberId in memberIds)
                    {
                        notification.Id = 0; //db identifies this as a new/unique notification. Only for new records
                        notification.RecipientId = memberId;

                        await _context.AddAsync(notification);
                    }

                    await _context.SaveChangesAsync();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }

        //Get all notifications where either the sender or receiver is a given user
        public async Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(string? userId)
        {
            try
            {

                IEnumerable<Notification> sortedNotifications = Enumerable.Empty<Notification>();
                if (!string.IsNullOrEmpty(userId))
                {

                    IEnumerable<Notification> notifications = Enumerable.Empty<Notification>();
                    notifications = await _context.Notifications
                                                  .Where(n => n.RecipientId == userId || n.SenderId == userId)
                                                  .Include(n => n.Recipient)
                                                  .Include(n => n.Sender)
                                                  .ToListAsync();
                    sortedNotifications = notifications.OrderByDescending(n => n.CreatedDate);
                }

                return sortedNotifications;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }

        public async Task<bool> NotifyDeveloper(Ticket? ticket, TTUser? ticketUser)
        {
            try
            {
                if (ticket != null && ticketUser != null)
                {
                    Notification? notification = new()
                    {
                        TicketId = ticket.Id,
                        CreatedDate = DateTimeOffset.UtcNow,
                        NotificationType = TTNotificationTypes.Ticket,
                        Title = $"Ticket {ticket.Title} Updated",
                        Message = $"Ticket: {ticket?.Title} was updated by {ticketUser?.FullName} ",
                        SenderId = ticketUser!.Id,
                        RecipientId = ticket!.DeveloperUserId
                    };
                    await AddNotificationAsync(notification);
                    await SendEmailNotificationAsync(notification, notification.Title);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }

        public async Task<bool> NotifyDeveloperOfAssignment(Notification? notification, TTUser? ticketUser)
        {
            try
            {
                if (notification != null && ticketUser != null)
                {
                    await AddNotificationAsync(notification);
                    await SendEmailNotificationAsync(notification, notification.Title);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }


        //an update notification to PM if one exists or the sender.
        //WHAT IF SENDER AND PM unassigned?. Should we send it to the developer?
        //when will devId be used?
        public async Task<bool> TicketUpdateNotificationAsync(int? ticketId, string? ticketUserId, string? ticketNotificationType, string? assignerId)
        {
            try
            {
                TTUser? ticketUser = await _userManager.FindByIdAsync(ticketUserId!);
                Ticket? ticket = await _context.Tickets.Include(t => t.Project).FirstOrDefaultAsync(t => t.Id == ticketId);
                TTUser? projectManager = await _projectService.GetProjectManagerAsync(ticket?.ProjectId);
                IEnumerable<TTUser> admins = await _rolesService.GetUsersInRoleAsync(nameof(TTRoles.Admin), ticket!.Project!.CompanyId);
                TTUser admin = admins.First();

                if (ticket != null)
                {
                    int companyId = ticket.Project!.CompanyId;
                    Notification? notification = new()
                    {
                        TicketId = ticketId,
                        CreatedDate = DateTimeOffset.UtcNow, //DataUtility.GetPostGresDate(DateTimeOffset.Now)
                        NotificationType = TTNotificationTypes.Ticket,
                        Title = "",
                        Message = "",
                        SenderId = ticketUserId,
                        RecipientId = projectManager?.Id ?? admin.Id
                    };

                    if (ticketNotificationType == "UpdateTicket")
                    {
                        notification.Title = "Ticket Updated";
                        notification.Message = $"Ticket: {ticket?.Title} was updated by {ticketUser?.FullName} ";
                        notification.SenderId = ticketUserId;
                        notification.RecipientId = projectManager?.Id ?? admin.Id;
                        if (ticket!.DeveloperUserId != null)
                        {
                            await NotifyDeveloper(ticket, ticketUser);
                        }
                    }
                    else if (ticketNotificationType == "NewTicket")
                    {
                        notification.Title = "New Ticket Added";
                        notification.Message = $"New Ticket: {ticket.Title} was created by {ticketUser?.FullName} ";
                        notification.SenderId = ticketUserId;
                        notification.RecipientId = projectManager?.Id ?? admin.Id;
                    }
                    else if (ticketNotificationType == "AssignedTicket")
                    {
                        TTUser? assigner = await _userManager.FindByIdAsync(assignerId!);
                        notification.Title = "Ticket Assigned";
                        notification.Message = $"Ticket: {ticket.Title} was assigned to you by {assigner!.FullName}";
                        notification.SenderId = assigner?.Id ?? admin?.Id;
                        notification.RecipientId = ticketUserId;
                        await NotifyDeveloperOfAssignment(notification, ticketUser);

                    }
                    else if (ticketNotificationType == "CommentAdded")
                    {
                        if (ticket!.DeveloperUserId != null)
                        {
                            await NotifyDeveloper(ticket, ticketUser);
                        }
                        notification.Title = "Comment Added";
                        notification.Message = $"Comment: A comment was added to the ticket '{ticket.Title}' by {ticketUser?.FullName}";
                    }
                    else if (ticketNotificationType == "AttachmentAdded")
                    {
                        if (ticket!.DeveloperUserId != null)
                        {
                            await NotifyDeveloper(ticket, ticketUser);
                        }
                        notification.Title = "Attachment Added";
                        notification.Message = $"Attachment: An Attachment was added to the ticket '{ticket.Title}' by {ticketUser?.FullName}";
                    }
                    else if (ticketNotificationType == "AttachmentRemoved")
                    {
                        if (ticket!.DeveloperUserId != null)
                        {
                            await NotifyDeveloper(ticket, ticketUser);
                        }
                        notification.Title = "Attachment Removed";
                        notification.Message = $"Attachment: An Attachment was removed from the ticket '{ticket.Title}' by {ticketUser?.FullName}";
                    }

                    if (ticketNotificationType != "AssignedTicket")
                    {
                        if (projectManager != null)
                        {
                            await AddNotificationAsync(notification);
                            await SendEmailNotificationAsync(notification, notification.Title);
                        }
                        else
                        {
                            await NotificationsByRoleAsync(companyId, notification, nameof(TTRoles.Admin));
                            await SendEmailNotificationByRoleAsync(companyId, notification, nameof(TTRoles.Admin));
                        }
                    }

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<bool> ProjectUpdateNotificationAsync(int? projectId, string? projectUserId, string projectNotificationType)
        {
            try
            {
                TTUser? projectUser = await _userManager.FindByIdAsync(projectUserId!);
                Project? project = await _context.Projects.Include(p => p.Tickets).Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);
                TTUser? projectManager = await _projectService.GetProjectManagerAsync(projectId);
                IEnumerable<TTUser> admins = await _rolesService.GetUsersInRoleAsync(nameof(TTRoles.Admin), project!.CompanyId);
                TTUser? projectAdmin = null;

                foreach (TTUser member in project.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, nameof(TTRoles.Admin)))
                    {
                        projectAdmin = member;
                    }
                }

                if (projectAdmin == null)
                {
                    projectAdmin = admins.First();
                }

                string? recipientId = "";

                if (projectUserId == projectAdmin!.Id && projectManager != null)
                {
                    recipientId = projectManager!.Id;
                }
                else
                {
                    recipientId = projectAdmin.Id;
                }

                if (project != null)
                {
                    int companyId = project.CompanyId;

                    Notification? notification = new()
                    {
                        ProjectId = projectId,
                        CreatedDate = DateTimeOffset.UtcNow, //DataUtility.GetPostGresDate(DateTimeOffset.Now)
                        NotificationType = TTNotificationTypes.Project,
                        Title = "",
                        Message = "",
                        SenderId = projectUserId,
                        RecipientId = recipientId
                    };

                    if (projectNotificationType == "UpdateProject")
                    {
                        notification.Title = "Project Updated";
                        notification.Message = $"Project: {project?.Name} was updated by {projectUser?.FullName} ";
                    }
                    else if (projectNotificationType == "NewProject")
                    {
                        notification.Title = "New Project Added";
                        notification.Message = $"New Project: {project.Name} was created by {projectUser?.FullName} ";
                    }
                    else if (projectNotificationType == "AssignedProject")
                    {
                        notification.Title = "Project Updated";
                        notification.Message = $"Project: {project.Name} was assigned by {projectUser?.FullName}";
                    }
                    else if (projectNotificationType == "TeamChanged")
                    {
                        notification.Title = "Project Team Updated";
                        notification.Message = $"Project: Team for '{project.Name}' updated by {projectUser!.FullName}";
                    }

                    await AddNotificationAsync(notification);
                    await SendEmailNotificationAsync(notification, notification.Title);

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<bool> SendEmailNotificationByRoleAsync(int? companyId, Notification? notification, string? role)
        {
            try
            {

                if (notification != null)
                {
                    IEnumerable<string> memberEmails = (await _rolesService.GetUsersInRoleAsync(role, companyId))!.Select(u => u.Email)!;

                    foreach (string adminEmail in memberEmails) //naming convention?
                    {
                        await _emailService.SendEmailAsync(adminEmail, notification.Title!, notification.Message!);
                    }
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }

        //given a notification, send an email to the notification object's recipient
        public async Task<bool> SendEmailNotificationAsync(Notification? notification, string? emailSubject)
        {
            try
            {
                if (notification != null)
                {
                    TTUser? ttUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == notification.RecipientId);

                    string? userEmail = ttUser?.Email;

                    if (userEmail != null)
                    {
                        await _emailService.SendEmailAsync(userEmail, emailSubject!, notification.Message!);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }

        public async Task<Notification> GetNotificationAsync(int? id)
        {
            try
            {
                if (id != null)
                {
                    Notification? notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id);
                    return notification!;
                }
                return null!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        public async Task MarkNotificationRead(Notification? notification)
        {
            try
            {
                if (notification != null && !notification.HasBeenViewed)
                {
                    notification.HasBeenViewed = true;
                    _context.Update(notification);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task MarkAllNotificationsRead(string? userId)
        {
            try
            {
                IEnumerable<Notification> notifications = await GetNotificationsByUserIdAsync(userId);
                foreach (Notification notification in notifications)
                {
                    if (!notification.HasBeenViewed)
                    {
                        notification.HasBeenViewed = true;
                        _context.Update(notification);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task MarkAllNotificationsUnread(string? userId)
        {
            try
            {
                IEnumerable<Notification> notifications = await GetNotificationsByUserIdAsync(userId);
                foreach (Notification notification in notifications)
                {
                    if (notification.HasBeenViewed)
                    {
                        notification.HasBeenViewed = false;
                        _context.Update(notification);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

    }
}
