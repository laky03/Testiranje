namespace SplitSync.Entities
{
    public class Chat
    {
        public long Id { get; set; }
        public long GrupaId { get; set; }
        public long SentById { get; set; }
        public string Poruka { get; set; } = "";
        public DateTime SentAtUtc { get; set; }

        public Group? Grupa { get; set; }
        public User? SentBy { get; set; }
    }
}
