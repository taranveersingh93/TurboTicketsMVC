using Microsoft.AspNetCore.Mvc.Rendering;

namespace TurboTicketsMVC.Models.ViewModels
{
    public class ManageUserRolesViewModel
    {
        public TTUser? TTUser { get; set; }
        public SelectList? Roles {  get; set; }
        public bool IsDemoUser { get; set; }
        public string? SelectedRole { get; set; }
    }
}
