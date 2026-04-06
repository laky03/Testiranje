namespace SplitSync.Entities
{
    public class ShoppingListaItem
    {
        public long Id { get; set; }
        public long GroupId { get; set; }
        public long TrazioUserId { get; set; }
        public long? NabavioUserId { get; set; }
        public string Naziv { get; set; } = "";
        public DateTime TrazenoUtc { get; set; }
        public DateTime? NabavljenoUtc { get; set; }

        public Group? Grupa { get; set; }
        public User? Trazio { get; set; }
        public User? Nabavio { get; set; }
    }
}
