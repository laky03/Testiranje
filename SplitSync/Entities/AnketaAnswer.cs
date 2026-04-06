namespace SplitSync.Entities
{
    public class AnketaAnswer
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long AnketaId { get; set; }
        public DateTime SubmittedAtUtc { get; set; }

        public User? User { get; set; }
        public Anketa? Anketa { get; set; }
        public ICollection<AnketaAnswerOption> AnketaAnswerOptions { get; set; } = new List<AnketaAnswerOption>();
    }
}
