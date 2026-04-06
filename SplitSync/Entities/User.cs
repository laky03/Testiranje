namespace SplitSync.Entities
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PasswordHash { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsEmailVerified { get; set; }
        public string? GoogleSubject { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public byte[]? Slika { get; set; }
        public string? SlikaExtension { get; set; }

        public ICollection<GroupsUsers> MemberGroups { get; set; } = new List<GroupsUsers>();
        public ICollection<GroupInvitation> GroupInvitations { get; set; } = new List<GroupInvitation>();
        public ICollection<GroupInvitation> GroupInvites { get; set; } = new List<GroupInvitation>();
        public ICollection<Racun> RacuniKaoKreator { get; set; } = new List<Racun>();
        public ICollection<RacunItem> RacunItems { get; set; } = new List<RacunItem>();
        public ICollection<Anketa> KreiraneAnkete { get; set; } = new List<Anketa>();
        public ICollection<AnketaAnswer> AnketaAnswers { get; set; } = new List<AnketaAnswer>();
        public ICollection<Dogadjaj> CreatedDogadjaji { get; set; } = new List<Dogadjaj>();
        public ICollection<DogadjajGlas> Glasovi { get; set; } = new List<DogadjajGlas>();
        public ICollection<ShoppingListaItem> TrazeniItemi { get; set; } = new List<ShoppingListaItem>();
        public ICollection<ShoppingListaItem> NabavljeniItemi { get; set; } = new List<ShoppingListaItem>();
        public ICollection<Chat> Chats { get; set; } = new List<Chat>();
    }
}
