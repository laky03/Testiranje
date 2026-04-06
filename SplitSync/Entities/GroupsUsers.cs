namespace SplitSync.Entities
{
    public class GroupsUsers
    {
        public long GroupId { get; set; }
        public long UserId { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime JoinedAtUtc { get; set; }
        public string? Nickname { get; set; }

        public Group Group { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
