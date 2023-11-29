using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TurboTicketsMVC.Data;
using TurboTicketsMVC.Models;
using TurboTicketsMVC.Services.Interfaces;

namespace TurboTicketsMVC.Services
{
    public class TTRolesService : ITTRolesService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<TTUser> _userManager;

        public TTRolesService(ApplicationDbContext context,  UserManager<TTUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<bool> AddUserToRoleAsync(TTUser? user, string? roleName)
        {
            try
            {
                if (user != null && !string.IsNullOrEmpty(roleName))
                {
                    //whether we add a user to a successful role

                    bool userAdded = (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;
                    return userAdded;
                }

                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<IdentityRole>> GetRolesAsync()
        {
            try
            {
                IEnumerable<IdentityRole> allRoles = Enumerable.Empty<IdentityRole>();
                allRoles = await _context.Roles.ToListAsync();
                return allRoles;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<string>?> GetUserRolesAsync(TTUser? user)
        {
            try
            {
                IEnumerable<string> userRoles = Enumerable.Empty<string>();
                if (user != null)
                {
                    userRoles = await _userManager.GetRolesAsync(user);
                }
                return userRoles;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<IEnumerable<TTUser>> GetUsersInRoleAsync(string? roleName, int? companyId)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<bool> IsUserInRoleAsync(TTUser? member, string? roleName)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveUserFromRoleAsync(TTUser? user, string? roleName)
        {
            try
            {
                if (user != null && !string.IsNullOrEmpty(roleName))
                {
                    bool userRemoved = (await _userManager.RemoveFromRoleAsync(user, roleName)).Succeeded;
                    return userRemoved;
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveUserFromRolesAsync(TTUser? user, IEnumerable<string>? roleNames)
        {
            try
            {
                if (user != null && roleNames != null)
                {
                    bool userRemoved = (await _userManager.RemoveFromRolesAsync(user, roleNames)).Succeeded;
                    return userRemoved;
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
