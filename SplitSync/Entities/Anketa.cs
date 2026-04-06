namespace SplitSync.Entities
{
    public class Anketa
    {
        public long Id { get; set; }
        public long GroupId { get; set; }
        public long CreatorId { get; set; }
        public string Naziv { get; set; } = "";
        public DateTime CreatedAtUtc { get; set; }
        public bool HasStarted { get; set; } = false;
        public bool IsFinished { get; set; } = false;

        public Group? Group { get; set; }
        public User? Creator { get; set; }
        public ICollection<AnketaOption> AnketaOptions { get; set; } = new List<AnketaOption>();
        public ICollection<AnketaAnswer> AnketaAnswers { get; set; } = new List<AnketaAnswer>();
    }
}
