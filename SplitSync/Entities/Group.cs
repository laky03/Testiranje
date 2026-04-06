namespace SplitSync.Entities
{
    public class Group
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public byte[]? Slika { get; set; }
        public long OwnerUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string DefaultValuta { get; set; } = "RSD";

        public ICollection<GroupsUsers> Members { get; set; } = new List<GroupsUsers>();
        public ICollection<GroupInvitation> GroupInvitations { get; set; } = new List<GroupInvitation>();
        public ICollection<Racun> Racuni { get; set; } = new List<Racun>();
        public ICollection<Anketa> Ankete { get; set; } = new List<Anketa>();
        public ICollection<Dogadjaj> Dogadjaji { get; set; } = new List<Dogadjaj>();
        public ICollection<ShoppingListaItem> ShoppingListaItems { get; set; } = new List<ShoppingListaItem>();
        public ICollection<Chat> Chats { get; set; } = new List<Chat>();
    }
}
