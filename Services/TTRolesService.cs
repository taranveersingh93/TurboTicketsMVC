using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TurboTicketsMVC.Data;
using TurboTicketsMVC.Models;
using TurboTicketsMVC.Models.Enums;
using TurboTicketsMVC.Services.Interfaces;

namespace TurboTicketsMVC.Services
{
	public class TTRolesService : ITTRolesService
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<TTUser> _userManager;

		public TTRolesService(ApplicationDbContext context, UserManager<TTUser> userManager)
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
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);

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
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);

				throw;
			}
		}

		public async Task<IEnumerable<IdentityRole>> GetProdRoles()
		{
			try
			{
				IEnumerable<IdentityRole> allRoles = await GetRolesAsync();
				IEnumerable<IdentityRole> roles = allRoles.Where(r => r.Name != "DemoUser");
				return roles;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);

				throw;
			}
		}
		public async Task<IEnumerable<string?>> GetUserRolesAsync(TTUser? user)
		{
			try
			{
				IEnumerable<string?> userRoles = Enumerable.Empty<string>();
				if (user != null)
				{
					userRoles = await _userManager.GetRolesAsync(user);
				}
				return userRoles;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);

				throw;
			}
		}

		public async Task<IEnumerable<TTUser>> GetUsersInRoleAsync(string? roleName, int? companyId)
		{
			try
			{
				IEnumerable<TTUser> roleUsers = Enumerable.Empty<TTUser>();
				if (companyId != null)
				{
					if (string.IsNullOrEmpty(roleName))
					{
						roleUsers = await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync();
					}
					else
					{
						roleUsers = await _userManager.GetUsersInRoleAsync(roleName);
						roleUsers = roleUsers.Where(u => u.CompanyId == companyId).ToList();
					}
				}
				return roleUsers;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);

				throw;
			}
		}

		public async Task<bool> IsUserInRoleAsync(TTUser? member, string? roleName)
		{
			try
			{
				IEnumerable<string> userRoles = Enumerable.Empty<string>();
				if (member != null)
				{
					userRoles = await _userManager.GetRolesAsync(member);
				}
				return userRoles.Any(role => role == roleName);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);

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
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);

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
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);

				throw;
			}
		}
		public async Task<TTUser> GetUserByIdAsync(string? userId)
		{
			try
			{
				TTUser? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
				return user!;
			}
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
	}
}
