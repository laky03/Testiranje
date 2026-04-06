namespace SplitSync.Models
{
    public class HomeViewModel
    {
        public bool DeoGrupe = true;
        public List<HomeEventDto> Events { get; set; } = new List<HomeEventDto>();
        public List<HomeGrupeInfoDto> Grupe { get; set; } = new List<HomeGrupeInfoDto>();
        public List<HomeUserInfoDto> Useri { get; set; } = new List<HomeUserInfoDto>();
        public bool HasMoreEvents { get; set; } = false;
    }

    public class HomeGrupeInfoDto
    {
        public long Id { get; set; }
        public string Naziv { get; set; } = null!;
        public string? SlikaBase64 { get; set; }
    }

    public class HomeUserInfoDto
    {
        public long Id { get; set; }
        public string Username { get; set; } = null!;
        public string? SlikaBase64 { get; set; }
    }

    public class HomeEventDto
    {
        public DateTime Datum { get; set; }
        public long GroupId { get; set; }
        public long CreatedByUserId { get; set; }

        public bool IsRacun { get; set; }
        public List<HomeRacunItemDto>? RacunItems { get; set; }
        public string? RacunNaziv { get; set; }

        public bool IsAnketa { get; set; }
        public long? AnketaId { get; set; }
        public string? AnketaNaziv { get; set; }
        public bool? AnketaStarted { get; set; }
        public bool? AnketaFinished { get; set; }
        public bool? AnketaUserVecGlasao { get; set; }

        public bool IsDogadjaj { get; set; }
        public string? DogadjajNaziv { get; set; }
        public string? DogadjajSlikaBase64 { get; set; }
        public string? DogadjajOpis { get; set; }
        public string? DogadjajLokacija { get; set; }

        public bool IsShoppingListItem { get; set; }
        public string? ShoppingListItemNaziv { get; set; }
    }

    public class HomeRacunItemDto
    {
        public long UserId { get; set; }
        public double Iznos { get; set; }
        public double DeoRacuna { get; set; }
    }

    public class HomeEventHelper
    {
        public long Id { get; set; }
        public DateTime Datum { get; set; }
        public bool IsRacun { get; set; } = false;
        public bool IsAnketa { get; set; } = false;
        public bool IsDogadjaj { get; set; } = false;
        public bool IsShoppingListItem { get; set; } = false;
    }
}
