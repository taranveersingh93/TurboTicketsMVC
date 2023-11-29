using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Security.Claims;
using System.Security.Principal;

namespace TurboTicketsMVC.Extensions
{
    public static class IdentityExtensions
    {
        //we use "This" to add a method GetCompanyId to the identity being passed in.
        //wouldn't be possible without "this". Inside the method we just define what it's supposed to do.

        public static int GetCompanyId(this IIdentity identity)
        {
            Claim claim = ((ClaimsIdentity)identity).FindFirst("CompanyId")!;
            return int.Parse(claim.Value);
        }
    }
}
