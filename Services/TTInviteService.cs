using Microsoft.EntityFrameworkCore;
using TurboTicketsMVC.Data;
using TurboTicketsMVC.Models;
using TurboTicketsMVC.Services.Interfaces;

namespace TurboTicketsMVC.Services
{
    public class TTInviteService : ITTInviteService
    {
        #region Properties
        private readonly ApplicationDbContext _context;

        #endregion

        #region Constructor
        public TTInviteService(ApplicationDbContext context)
        {
            _context = context;
        }

        #endregion

        #region Get Company Invites
        public async Task<IEnumerable<Invite>> GetCompanyInvites(int? companyId)
        {

            try
            {
                IEnumerable<Invite> companyInvites = Enumerable.Empty<Invite>();
                if (companyId != null)
                {
                    companyInvites = await _context.Invites.Include(i => i.Company)
                                                     .Include(i => i.Invitee)
                                                     .Include(i => i.Invitor)
                                                     .Include(i => i.Project)
                                                     .Where(i => i.CompanyId == companyId).ToListAsync();

                    //time validity check
                    foreach (Invite invite in companyInvites)
                    {
                        DateTime inviteDate = invite.InviteDate.DateTime;

                        bool validDate = (DateTime.Now - inviteDate).TotalDays <= 7;
                        if (!validDate)
                        {
                            invite.IsValid = false;
                            _context.Update(invite);
                            await _context.SaveChangesAsync();
                        }
                    }

                }
                return companyInvites;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

        }
        #endregion
        #region Accept Invite
        public async Task<bool> AcceptInviteAsync(Guid? token, string? userId, int companyId)
        {
            Invite invite = (await _context.Invites.FirstOrDefaultAsync(i => i.CompanyToken == token))!;

            if (invite == null)
            {
                return false;
            }

            try
            {
                invite.JoinDate = DateTimeOffset.Now;
                invite.IsValid = false;
                invite.InviteeId = userId;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        #endregion

        #region Add new Invite
        public async Task<bool> AddNewInviteAsync(Invite? invite)
        {
            try
            {
                if (invite != null)
                {
                    if (await IsInviteEmailValid(invite))
                    {
                        await InvalidateExistingCompanyInvites(invite);
                        await _context.AddAsync(invite);
                        await _context.SaveChangesAsync();
                        return true;
                    }
                    else
                    {
                        return false;
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

        #endregion

        #region Check all invites across db
        public async Task<bool> IsInviteEmailValid(Invite invite)
        {
            try
            {
                IEnumerable<Invite> otherCompanyValidInvites = await _context.Invites
                                                                    .Where(i => i.IsValid && i.CompanyId != invite.CompanyId)
                                                                    .ToListAsync();
                IEnumerable<TTUser> allUsers = await _context.Users.ToListAsync();
                bool inviteEmailExists = otherCompanyValidInvites.Any(i => i.InviteeEmail == invite.InviteeEmail);
                bool userEmailExists = allUsers.Any(u => u.Email == invite.InviteeEmail);
                bool invalidEmail = inviteEmailExists || userEmailExists;
                bool isEmailValid = !invalidEmail;
                return isEmailValid;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        #endregion

        #region Invalidate company invites for the email
        public async Task InvalidateExistingCompanyInvites(Invite invite)
        {
            try
            {
                if (invite != null)
                {
                    IEnumerable<Invite> matchingInvites = await _context.Invites
                                                                        .Where(i => i.InviteeEmail == invite.InviteeEmail && i.CompanyId == invite.CompanyId)
                                                                        .ToListAsync();
                    if (matchingInvites.Count() > 0)
                    {
                        foreach (Invite matchingInvite in matchingInvites)
                        {
                            matchingInvite.IsValid = false;
                            _context.Update(matchingInvite);
                        }
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
        #endregion
        #region Any Invite (?)

        public async Task<bool> AnyInviteAsync(Guid token, string? email, int? companyId)
        {
            try
            {
                bool result = await _context.Invites.Where(i => i.CompanyId == companyId)
                                                    .AnyAsync(i => i.CompanyToken == token && i.InviteeEmail == email);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        #endregion

        #region Get Invite {id}
        public async Task<Invite?> GetInviteByIdAsync(int? inviteId, int? companyId)
        {
            try
            {
                Invite? invite = new();
                if (inviteId != null && companyId != null)
                {
                    invite = await _context.Invites.Where(i => i.CompanyId == companyId)
                                                      .Include(i => i.Company)
                                                      .Include(i => i.Project)
                                                      .Include(i => i.Invitor)
                                                      .FirstOrDefaultAsync(i => i.Id == inviteId);
                }

                return invite!;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        #endregion

        #region Get Invite {token, email}
        public async Task<Invite> GetInviteByTokenAsync(Guid token, string? email, int? companyId)
        {
            try
            {
                Invite? invite = await _context.Invites.Where(i => i.CompanyId == companyId)
                                                      .Include(i => i.Company)
                                                      .Include(i => i.Project)
                                                      .Include(i => i.Invitor)
                                                      .FirstOrDefaultAsync(i => i.CompanyToken == token && i.InviteeEmail == email);

                return invite!;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        #endregion

        #region Validate Invite Code
        public async Task<bool> ValidateInviteCodeAsync(Guid? token)
        {
            try
            {
                if (token == null)
                {
                    return false;
                }

                bool result = false;


                Invite? invite = await _context.Invites.FirstOrDefaultAsync(i => i.CompanyToken == token);


                if (invite != null)
                {
                    // Determine invite date
                    DateTime inviteDate = invite.InviteDate.DateTime;

                    // Custom validation of invite based on the date it was issued
                    // In this case we are allowing an invite to be valid for 7 days
                    bool validDate = (DateTime.Now - inviteDate).TotalDays <= 7;

                    if (validDate)
                    {
                        result = invite.IsValid;
                    }


                }
                return result;

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
