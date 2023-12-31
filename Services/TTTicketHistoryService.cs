﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TurboTicketsMVC.Data;
using TurboTicketsMVC.Models;
using TurboTicketsMVC.Models.Enums;
using TurboTicketsMVC.Services.Interfaces;

namespace TurboTicketsMVC.Services
{
    public class TTTicketHistoryService : ITTTicketHistoryService
    {
        #region Injection
        private readonly ApplicationDbContext _context;
        private readonly UserManager<TTUser> _userManager;
        private readonly ITTRolesService _rolesService;

        public TTTicketHistoryService(ApplicationDbContext context,
                                        UserManager<TTUser> userManager,
                                        ITTRolesService rolesService)
        {
            _context = context;
            _userManager = userManager;
            _rolesService = rolesService;
        }
        #endregion

        #region Add History 1
        public async Task AddHistoryAsync(Ticket? oldTicket, Ticket? newTicket, string? userId)
        {
            try
            {

                TTUser? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (oldTicket == null && newTicket != null)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        PropertyName = "New Ticket",
                        OldValue = "",
                        NewValue = "",
                        CreatedDate = DateTimeOffset.Now,
                        UserId = userId,
                        Description = "New Ticket Created"
                    };
                    await _context.TicketHistories.AddAsync(history);
                    await _context.SaveChangesAsync();

                }
                else
                {
                    //Check Ticket Title
                    if (oldTicket?.Title != newTicket?.Title)
                    {
                        TicketHistory? history = new()
                        {
                            TicketId = newTicket!.Id,
                            PropertyName = "Title",
                            OldValue = oldTicket?.Title,
                            NewValue = newTicket?.Title,
                            CreatedDate = DateTimeOffset.Now,
                            UserId = userId,
                            Description = $"New ticket title: {newTicket?.Title}"
                        };
                        await _context.TicketHistories.AddAsync(history);
                    }

                    //Check Ticket Description
                    if (oldTicket?.Description != newTicket?.Description)
                    {
                        TicketHistory? history = new()
                        {
                            TicketId = newTicket!.Id,
                            PropertyName = "Description",
                            OldValue = oldTicket?.Description,
                            NewValue = newTicket?.Description,
                            CreatedDate = DateTimeOffset.Now,
                            UserId = userId,
                            Description = $"New ticket description"
                        };
                        await _context.TicketHistories.AddAsync(history);
                    }

                    //Check Ticket Priority
                    if (oldTicket?.TicketPriority != newTicket?.TicketPriority)
                    {
                        TicketHistory? history = new()
                        {
                            TicketId = newTicket!.Id,
                            PropertyName = "Ticket Priority",
                            OldValue = oldTicket?.TicketPriority.ToString(),
                            NewValue = newTicket?.TicketPriority.ToString(),
                            CreatedDate = DateTimeOffset.Now,
                            UserId = userId,
                            Description = $"New ticket priority: {newTicket?.TicketPriority!.ToString()}"
                        };
                        await _context.TicketHistories.AddAsync(history);
                    }

                    //Check Ticket Status
                    if (oldTicket?.TicketStatus != newTicket?.TicketStatus)
                    {
                        TicketHistory? history = new()
                        {
                            TicketId = newTicket!.Id,
                            PropertyName = "Ticket Status",
                            OldValue = oldTicket?.TicketStatus!.ToString(),
                            NewValue = newTicket?.TicketStatus!.ToString(),
                            CreatedDate = DateTimeOffset.Now,
                            UserId = userId,
                            Description = $"New ticket Status: {newTicket?.TicketStatus!.ToString()}"
                        };
                        await _context.TicketHistories.AddAsync(history);
                    }

                    //Check Ticket Type
                    if (oldTicket?.TicketType != newTicket?.TicketType)
                    {
                        TicketHistory? history = new()
                        {
                            TicketId = newTicket!.Id,
                            PropertyName = "Ticket Type",
                            OldValue = oldTicket?.TicketType!.ToString(),
                            NewValue = newTicket?.TicketType!.ToString(),
                            CreatedDate = DateTimeOffset.Now,
                            UserId = userId,
                            Description = $"New ticket Type: {newTicket?.TicketType!.ToString()}"
                        };
                        await _context.TicketHistories.AddAsync(history);
                    }

                    //Check Ticket archival
                    if (oldTicket?.Archived != newTicket?.Archived)
                    {
                        TicketHistory? history = new()
                        {
                            TicketId = newTicket!.Id,
                            PropertyName = "Archive Status",
                            OldValue = oldTicket!.Archived.ToString(),
                            NewValue = newTicket.Archived.ToString(),
                            CreatedDate = DateTimeOffset.Now,
                            UserId = userId,
                            Description = $"Ticket Archive status changed: {newTicket?.Archived!.ToString()}"
                        };
                        await _context.TicketHistories.AddAsync(history);
                    }

                    //Check Ticket Developer
                    if (oldTicket?.DeveloperUserId != newTicket?.DeveloperUserId)
                    {
                        TicketHistory? history = new()
                        {
                            TicketId = newTicket!.Id,
                            PropertyName = "Developer",
                            OldValue = oldTicket?.DeveloperUser?.FullName ?? "Not Assigned",
                            NewValue = newTicket?.DeveloperUser?.FullName,
                            CreatedDate = DateTimeOffset.Now,
                            UserId = userId,
                            Description = $"New ticket developer: {newTicket?.DeveloperUser?.FullName}"

                        };
                        await _context.TicketHistories.AddAsync(history);
                    }

                    //Check ticket comment count
                    if (oldTicket?.Comments.Count != newTicket?.Comments.Count)
                    {
                        TicketHistory? history = new()
                        {
                            TicketId = newTicket!.Id,
                            PropertyName = "Ticket Comment",
                            OldValue = oldTicket?.Comments?.Count == 1 ? "1 Comment" : $"{oldTicket?.Comments.Count} comments",
                            NewValue = $"Comment #{newTicket.Comments.Count} added by {newTicket.Comments.Last()!.User!.FullName}",
                            CreatedDate = DateTimeOffset.Now,
                            UserId = userId,
                            Description = $"New Comment by {newTicket.Comments.Last()!.User!.FullName}"

                        };
                        await _context.TicketHistories.AddAsync(history);
                    }

                    if (oldTicket?.Attachments.Count < newTicket?.Attachments.Count)
                    {
                        TicketHistory? history = new()
                        {
                            TicketId = newTicket!.Id,
                            PropertyName = "Ticket Attachment",
                            OldValue = oldTicket?.Attachments?.Count == 1 ? "1 Attachment" : $"{oldTicket?.Attachments.Count} attachments",
                            NewValue = $"Attachment #{newTicket.Attachments.Count} added by {newTicket.Attachments.Last()!.TTUser!.FullName}",
                            CreatedDate = DateTimeOffset.Now,
                            UserId = userId,
                            Description = $"New Attachment by {newTicket.Attachments.Last()!.TTUser!.FullName}"

                        };
                        await _context.TicketHistories.AddAsync(history);
                    }

                    if (oldTicket?.Attachments.Count > newTicket?.Attachments.Count)
                    {
                        TicketHistory? history = new()
                        {
                            TicketId = newTicket!.Id,
                            PropertyName = "Ticket Attachment Removed",
                            OldValue = oldTicket?.Attachments?.Count == 1 ? "1 Attachment" : $"{oldTicket?.Attachments.Count} attachments",
                            NewValue = $"Attachment #{oldTicket!.Attachments.Count} removed by {user!.FullName}",
                            CreatedDate = DateTimeOffset.Now,
                            UserId = userId,
                            Description = $"Attachment removed by {user!.FullName}"

                        };
                        await _context.TicketHistories.AddAsync(history);
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

        #endregion

        #region get Project ticket histories
        public async Task<IEnumerable<TicketHistory>> GetProjectTicketsHistoriesAsync(int? projectId, int? companyId)
        {
            try
            {
                Project? project = await _context.Projects.Where(p => p.CompanyId == companyId)
                                                         .Include(p => p.Tickets)
                                                            .ThenInclude(t => t.History)
                                                                .ThenInclude(h => h.User)
                                                         .FirstOrDefaultAsync(p => p.Id == projectId);

                IEnumerable<TicketHistory> ticketHistory = project!.Tickets.SelectMany(t => t.History).ToList();

                return ticketHistory;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        #endregion

        #region get company ticket histories
        public async Task<IEnumerable<TicketHistory>> GetCompanyTicketsHistoriesAsync(int? companyId)
        {

            try
            {
                IEnumerable<Project> projects = (await _context.Companies.AsNoTracking()
                                                        .Include(c => c.Projects)
                                                            .ThenInclude(p => p.Tickets)
                                                                .ThenInclude(t => t.History)
                                                                    .ThenInclude(h => h.User)
                                                        .FirstOrDefaultAsync(c => c.Id == companyId))!.Projects.ToList();

                IEnumerable<Ticket> tickets = projects.SelectMany(p => p.Tickets.Where(t => t.Archived == false && t.ArchivedByProject == false)).ToList();

                IEnumerable<TicketHistory> ticketHistories = tickets.SelectMany(t => t.History).ToList();
                IEnumerable<TicketHistory> sortedHistories = ticketHistories.OrderByDescending(h => h.CreatedDate);
                return sortedHistories;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        #endregion

        #region get company ticket histories
        public async Task<IEnumerable<TicketHistory>> GetUserTicketsHistoriesAsync(int? companyId, string? userId)
        {

            try
            {
                TTUser? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                IEnumerable<Ticket> userTickets = Enumerable.Empty<Ticket>();

                IEnumerable<Ticket> companyTickets = await _context.Tickets.AsNoTracking()
                            .Include(t => t.DeveloperUser)
                            .Include(t => t.History)
                                .ThenInclude(h => h.User)
                            .Include(t => t.Project)
                                .ThenInclude(p => p!.Members)
                            .Include(t => t.SubmitterUser)
                            .Where(t => t.Project!.CompanyId == companyId && t.ArchivedByProject == false && t.Archived == false).ToListAsync();

                bool isAdmin = await _rolesService.IsUserInRoleAsync(user, nameof(TTRoles.Admin));
                bool isProjectManager = await _rolesService.IsUserInRoleAsync(user, nameof(TTRoles.ProjectManager));
                bool isDeveloper = await _rolesService.IsUserInRoleAsync(user, nameof(TTRoles.Developer));
                bool isSubmitter = await _rolesService.IsUserInRoleAsync(user, nameof(TTRoles.Submitter));

                if (isAdmin)
                {
                    userTickets = companyTickets;
                }
                else if (isProjectManager)
                {
                    userTickets = companyTickets.Where(t => t.Project!.Members.Any(m => m.Id == userId)).ToList();
                }
                else if (isDeveloper)
                {
                    userTickets = companyTickets.Where(t => t.DeveloperUserId == userId || t.SubmitterUserId == userId).ToList();
                }
                else if (isSubmitter)
                {
                    userTickets = companyTickets.Where(t => t.SubmitterUserId == userId).ToList();
                }


                IEnumerable<TicketHistory> ticketHistories = userTickets.SelectMany(t => t.History).ToList();
                IEnumerable<TicketHistory> sortedHistories = ticketHistories.OrderByDescending(h => h.CreatedDate);
                return sortedHistories;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
        #endregion
    }
}
