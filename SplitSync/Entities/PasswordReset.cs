namespace SplitSync.Entities
{
    public class PasswordReset
    {
        public long Id { get; set; }
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
    }
}
