using Microsoft.AspNetCore.Identity;
using TurboTicketsMVC.Models;

namespace TurboTicketsMVC.Services.Interfaces
{
    public interface ITTRolesService
    {
        public Task<bool> AddUserToRoleAsync(TTUser? user, string? roleName);

        public Task<IEnumerable<IdentityRole>> GetRolesAsync();

        public Task<IEnumerable<string?>> GetUserRolesAsync(TTUser? user);
        public Task<IEnumerable<IdentityRole>> GetProdRoles();
        public Task<IEnumerable<TTUser>> GetUsersInRoleAsync(string? roleName, int? companyId);

        public Task<bool> IsUserInRoleAsync(TTUser? member, string? roleName);

        public Task<bool> RemoveUserFromRoleAsync(TTUser? user, string? roleName);

        public Task<bool> RemoveUserFromRolesAsync(TTUser? user, IEnumerable<string>? roleNames);
        public Task<TTUser> GetUserByIdAsync(string? userId);

    }
}
