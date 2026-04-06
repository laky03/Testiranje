namespace SplitSync.Entities
{
    public class RacunItem
    {
        public long Id { get; set; }
        public long RacunId { get; set; }
        public long UserId { get; set; }
        public double Iznos { get; set; }
        public double DeoRacuna { get; set; }

        public Racun? Racun { get; set; }
        public User? User { get; set; }
    }
}
