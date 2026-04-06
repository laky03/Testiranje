namespace SplitSync.Entities
{
    public class GroupInvitation
    {
        public long Id { get; set; }
        public long GroupId { get; set; }
        public long InvitedUserId { get; set; }
        public long InvitedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public Group Group { get; set; } = null!;
        public User InvitedUser { get; set; } = null!;
        public User InvitedByUser { get; set; } = null!;
    }
}
