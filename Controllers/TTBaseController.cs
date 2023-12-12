using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TurboTicketsMVC.Extensions;

namespace TurboTicketsMVC.Controllers
{
    public abstract class TTBaseController: Controller
    {
        protected string? _userId => User.FindFirstValue(ClaimTypes.NameIdentifier);
        protected int _companyId => User.Identity!.GetCompanyId();
    }
}
