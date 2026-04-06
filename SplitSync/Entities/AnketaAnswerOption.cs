namespace SplitSync.Entities
{
    public class AnketaAnswerOption
    {
        public long Id { get; set; }
        public long AnketaAnswerId { get; set; }
        public long AnketaOptionId { get; set; }
        public int Ocena { get; set; }

        public AnketaAnswer? AnketaAnswer { get; set; }
        public AnketaOption? AnketaOption { get; set; }
    }
}
