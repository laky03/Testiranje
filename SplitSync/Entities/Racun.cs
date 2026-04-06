namespace SplitSync.Entities
{
    public class Racun
    {
        public long Id { get; set; }
        public long GroupId { get; set; }
        public string? Naziv { get; set; }
        public double Iznos { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public long? CreatorUserId { get; set; }

        public ICollection<RacunItem> Items { get; set; } = new List<RacunItem>();
        public Group? Group { get; set; }
        public User? Creator { get; set; }
    }
}
