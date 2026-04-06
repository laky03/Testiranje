namespace SplitSync.Models
{
    public class GroupsIndexViewModel
    {
        public List<GroupItem> Groups { get; set; } = new List<GroupItem>();
        public List<InviteItem> Invitations { get; set; } = new List<InviteItem>();
    }

    public class GroupItem
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public string? ImageBase64 { get; set; }
        public bool IsOwner { get; set; }
    }

    public class InviteItem
    {
        public long InvitationId { get; set; }
        public long GroupId { get; set; }
        public string GroupName { get; set; } = "";
        public string InvitedBy { get; set; } = "";
        public DateTime CreatedAtUtc { get; set; }
    }

    public class GroupCreateViewModel
    {
        public string Name { get; set; } = "";
        public IFormFile? ImageUpload { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class GroupDashboardViewModel
    {
        public long GroupId { get; set; }
        public string GroupName { get; set; } = "";
        public string? ImageBase64 { get; set; }
        public bool CurrentUserIsAdmin { get; set; }

        public List<MemberItem> Members { get; set; } = new();

        public string? ErrorMessage { get; set; }
    }

    public class MemberItem
    {
        public long UserId { get; set; }
        public string Username { get; set; } = "";
        public string? Nickname { get; set; }
        public bool IsAdmin { get; set; }
    }
}
