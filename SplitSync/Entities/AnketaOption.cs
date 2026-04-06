namespace SplitSync.Entities
{
    public class AnketaOption
    {
        public long Id { get; set; }
        public long AnketaId { get; set; }
        public string Naziv { get; set; } = "";
        public DateTime CreatedAtUtc { get; set; }

        public Anketa? Anketa { get; set; }
        public ICollection<AnketaAnswerOption> AnketaAnswerOptions { get; set; } = new List<AnketaAnswerOption>();
    }
}
