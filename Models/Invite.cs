using System.ComponentModel.DataAnnotations;

namespace TurboTicketsMVC.Models
{
    public class Invite
    {
        private DateTimeOffset _inviteDate;
        private DateTimeOffset? _joinDate;
        public int Id { get; set; }
        public DateTimeOffset InviteDate { get { return _inviteDate; } set { _inviteDate = value.ToUniversalTime(); } }
        public DateTimeOffset? JoinDate
        {
            get => _joinDate; set
            {
                if (value.HasValue)
                {
                    _joinDate = value.Value.ToUniversalTime();
                }
            }
        }

        public Guid CompanyToken { get; set; }
        //foreign keys
        public int CompanyId { get; set; }
        public int? ProjectId { get; set; }

        [Required]
        public string? InvitorId { get; set; }
        public string? InviteeId { get; set; }
        //foreign keys

        [Required]
        public string? InviteeEmail { get; set; }
        [Required]
        public string? InviteeFirstName { get; set; }
        [Required]
        public string? InviteeLastName { get; set; }

        [StringLength(500, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Message { get; set; }

        public bool IsValid { get; set; }

        //navigation
        public Company? Company { get; set; }
        public Project? Project { get; set; }
        public TTUser? Invitor { get; set; }
        public TTUser? Invitee {  get; set; }
    }
}
