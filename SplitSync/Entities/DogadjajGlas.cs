namespace SplitSync.Entities
{
    public enum GlasOptions
    {
        Ide = 0,
        Mozda = 1,
        NeIde = 2,
    }
    public class DogadjajGlas
    {
        public long Id { get; set; }
        public long DogadjajId { get; set; }
        public long UserId { get; set; }
        public GlasOptions GlasOption { get; set; }

        public Dogadjaj? Dogadjaj { get; set; }
        public User? User { get; set; }
    }
}
