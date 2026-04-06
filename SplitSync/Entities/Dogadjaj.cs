using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics.Eventing.Reader;

namespace SplitSync.Entities
{
    public class Dogadjaj
    {
        public long Id { get; set; }
        public long GrupaId { get; set; }
        public long CreatorId { get; set; }
        public byte[]? Slika { get; set; }
        public string Naziv { get; set; } = "";
        public string? Opis { get; set; }
        public string? Lokacija { get; set; }
        public DateTime VremeDogadjaja { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public Group? Grupa { get; set; }
        public User? Creator { get; set; }
        public ICollection<DogadjajGlas> Glasovi { get; set; } = new List<DogadjajGlas>();
    }
}
