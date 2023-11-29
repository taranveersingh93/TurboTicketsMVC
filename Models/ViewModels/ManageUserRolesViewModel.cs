using Microsoft.AspNetCore.Mvc.Rendering;

namespace TurboTicketsMVC.Models.ViewModels
{
    public class ManageUserRolesViewModel
    {
        public TTUser? TTUser { get; set; }
        public MultiSelectList? Roles {  get; set; }
        public IEnumerable<string>? SelectedRoles { get; set; }
    }
}
