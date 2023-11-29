using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using TurboTicketsMVC.Models;

namespace TurboTicketsMVC.Extensions
{
    public class TTUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<TTUser, IdentityRole>
    {
        public TTUserClaimsPrincipalFactory(UserManager<TTUser> userManager,
                                            RoleManager<IdentityRole> roleManager,
                                            IOptions<IdentityOptions> optionsAccessor)
            :base(userManager, roleManager, optionsAccessor)
        { }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(TTUser user)
        {
            ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("CompanyId", user.CompanyId.ToString()));
            return identity;
        }
    }
}
