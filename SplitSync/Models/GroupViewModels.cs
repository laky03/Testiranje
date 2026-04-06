using SplitSync.Entities;

namespace SplitSync.Models
{
    public class RacuniViewModel
    {
        public long GroupId { get; set; }
        public List<RacuniDto> Racuni { get; set; } = new List<RacuniDto>();
    }
    public class RacuniDto
    {
        public long Id { get; set; }
        public bool UserCanDelete { get; set; } = false;
        public List<RacunItemDto> Items { get; set; } = new List<RacunItemDto>();
        public double Iznos { get; set; }
        public string Naziv { get; set; } = "";
        public DateTime CreatedAtUtc { get; set; }
    }
    public class RacunItemDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Username { get; set; } = "";
        public string? Nickname { get; set; }
        public double Iznos { get; set; }
        public double DeoRacuna { get; set; }
    }


    public class NoviRacunViewModel
    {
        public long GroupId { get; set; }
        public string? Naziv { get; set; }
        public List<NoviRacunClan> Clanovi { get; set; } = new();

        public string? ErrorMessage { get; set; }
    }
    public class NoviRacunClan
    {
        public long UserId { get; set; }
        public string Username { get; set; } = "";
        public string? Nickname { get; set; }
        public bool IsSelected { get; set; }
        public double? Iznos { get; set; }
        public double? DeoRacuna { get; set; }
    }


    public class PredlogUplataViewModel
    {
        public string? ErrorMessage { get; set; }
        public List<JedanPredlogUplataViewModel> PredlogUplata { get; set; } = new List<JedanPredlogUplataViewModel>();
    }
    public class JedanPredlogUplataViewModel
    {
        public long SenderId { get; set; }
        public string SenderUsername { get; set; } = string.Empty;
        public string? SenderNickname { get; set; }
        public long RecieverId { get; set; }
        public string RecieverUsername { get; set; } = string.Empty;
        public string? RecieverNickname { get; set; }
        public double Iznos { get; set; }
    }


    public class AnketeViewModel
    {
        public long GrupaId { get; set; }
        public List<AnketaSimpleViewModelDto> AnketeGlasanje { get; set; } = new List<AnketaSimpleViewModelDto>();
        public List<AnketaSimpleViewModelDto> AnketeZaEdit { get; set; } = new List<AnketaSimpleViewModelDto>();
        public List<AnketaSimpleViewModelDto> ZavrseneAnkete { get; set; } = new List<AnketaSimpleViewModelDto>();
        public List<AnketaSimpleViewModelDto> AnketeUToku { get; set; } = new List<AnketaSimpleViewModelDto>();
    }
    public class AnketaSimpleViewModelDto
    {
        public long Id { get; set; }
        public string Naziv { get; set; } = "";
        public DateTime CreatedAtUtc { get; set; }
        public int BrojOpcija { get; set; } = 0;
        public bool HasStarted { get; set; } = false;
        public bool IsFinished { get; set; } = false;
        public bool UserVecGlasao { get; set; } = false;
        public bool MozeDaObrise { get; set; } = false;
        public List<RezultatiZavrseneAnkete> Rezultati { get; set; } = new List<RezultatiZavrseneAnkete>();
    }
    public class RezultatiZavrseneAnkete
    {
        public string Naziv { get; set; } = "";
        public double ProsecnaOcena { get; set; } = 0;
    }

    public class AnketaCreateViewModel
    {
        public long GrupaId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public bool StartOdmah { get; set; } = false;
        public List<string> Opcije { get; set; } = new() { "", "", "" };
        public string? ErrorMessage { get; set; }
    }

    public class AnketaEditViewModel
    {
        public long GrupaId { get; set; }
        public long AnketaId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public bool HasStarted { get; set; }
        public bool IsCreator { get; set; }
        public List<AnketaOptionEditVm> Opcije { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }
    public class AnketaOptionEditVm
    {
        public long Id { get; set; }
        public string Naziv { get; set; } = string.Empty;
    }

    public class AnketaGlasanjeViewModel
    {
        public long GrupaId { get; set; }
        public long AnketaId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public List<AnketaGlasanjeOpcijaVm> Opcije { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }
    public class AnketaGlasanjeOpcijaVm
    {
        public long OpcijaId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public int? Ocena { get; set; }
    }

    public class AddOptionPostModel
    {
        public long GrupaId { get; set; }
        public long AnketaId { get; set; }
        public string Naziv { get; set; } = string.Empty;
    }

    public class UpdateAnketaNazivPostModel
    {
        public long GrupaId { get; set; }
        public long AnketaId { get; set; }
        public string Naziv { get; set; } = string.Empty;
    }

    public class AnketaGlasanjePostModel
    {
        public long GrupaId { get; set; }
        public long AnketaId { get; set; }
        public Dictionary<long, int> Ocene { get; set; } = new Dictionary<long, int>();
    }


    public class DogadjajiViewModel
    {
        public long GrupaId { get; set; }
        public string? ErrorMessage { get; set; }
        public List<DogadjajListItem> Dogadjaji { get; set; } = new();
    }

    public class DogadjajListItem
    {
        public long Id { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string? Opis { get; set; }
        public string? Lokacija { get; set; }
        public DateTime VremeDogadjaja { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public long CreatorId { get; set; }
        public string? CreatorNickname { get; set; }
        public string CreatorUsername { get; set; } = string.Empty;

        public string? SlikaBase64 { get; set; }

        public GlasOptions? MojGlas { get; set; }
        public List<string> Idu { get; set; } = new List<string>();
        public List<string> MozdaIdu { get; set; } = new List<string>();
        public List<string> NeIdu { get; set; } = new List<string>();
        public bool CanEdit { get; set; }
    }

    public class DogadjajCreateViewModel
    {
        public long GrupaId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string? Opis { get; set; }
        public string? Lokacija { get; set; }
        public DateTime? VremeDogadjaja { get; set; }
        public IFormFile? Slika { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class DogadjajEditViewModel
    {
        public long GrupaId { get; set; }
        public long Id { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string? Opis { get; set; }
        public string? Lokacija { get; set; }

        public string? CurrentSlikaBase64 { get; set; }

        public IFormFile? NovaSlika { get; set; }
        public string? ErrorMessage { get; set; }
    }


    public class ShoppingListaViewModel
    {
        public long GrupaId { get; set; }
        public List<ShoppingListaItemDto> Items { get; set; } = new List<ShoppingListaItemDto>();
        public List<ShoppingListaItemDto> Nabavljeni { get; set; } = new List<ShoppingListaItemDto>();
        public string? ErrorMessage { get; set; }
    }
    public class ShoppingListaItemDto
    {
        public long Id { get; set; }
        public string Naziv { get; set; } = "";
        public long TrazioUserId { get; set; }
        public string? TrazioUserNickname { get; set; }
        public string TrazioUserUsername { get; set; } = "";
        public long? NabavioUserId { get; set; }
        public string? NabavioUserNickname { get; set; }
        public string? NabavioUserUsername { get; set; }
        public DateTime TrazenoUtc { get; set; }
        public DateTime? NabavljenoUtc { get; set; }
        public bool CanDelete { get; set; }
    }


    public class ChatViewModel
    {
        public long GrupaId { get; set; }
        public List<ChatUserInfoDto> UserInfo { get; set; } = new List<ChatUserInfoDto>();
        public List<ChatMessagesDto> Poruke { get; set; } = new List<ChatMessagesDto>();
    }
    public class ChatUserInfoDto
    {
        public long UserId { get; set; }
        public string? Nickname { get; set; }
        public string Username { get; set; } = "";
        public string? Base64Image { get; set; } = "";
    }
    public class ChatMessagesDto
    {
        public long Id { get; set; }
        public long SentById { get; set; }
        public string Poruka { get; set; } = "";
        public DateTime SentAtUtc { get; set; }
    }

    public class DogadjajiDto
    {
        public long Id { get; set; }
        public string Naziv { get; set; } = "";
        public string? Opis { get; set; }
        public string? Lokacija { get; set; }
        public DateTime VremeDogadjaja { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public long CreatorId { get; set; }
        public string? CreatorNickname { get; set; }
        public string CreatorUsername { get; set; } = "";
        public string? SlikaBase64 { get; set; }
        public GlasOptions? MojGlas { get; set; }
        public List<string> Idu { get; set; } = new List<string>();
        public List<string> MozdaIdu { get; set; } = new List<string>();
        public List<string> NeIdu { get; set; } = new List<string>();
        public bool CanEdit { get; set; }
    }

    public class RacuniApiDto
    {
        public long Id { get; set; }
        public string Naziv { get; set; } = "";
        public double Iznos { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public List<RacunItemDto> Items { get; set; } = new List<RacunItemDto>();
    }

    public class InvitationsDto
    {
        public long InvitationId { get; set; }
        public long GroupId { get; set; }
        public string GroupName { get; set; } = "";
        public string InvitedBy { get; set; } = "";
        public DateTime CreatedAtUtc { get; set; }
    }

    public class SettingsViewModel
    {
        public long GrupaId { get; set; }
        public bool IsAdmin { get; set; }
        public string? NazivGrupe { get; set; }
        public string? Base64Slika { get; set; }
        public IFormFile? Slika { get; set; }
        public string? ErrorMessage { get; set; }
        public List<UserGroupSettingsDto> Users { get; set; } = new List<UserGroupSettingsDto>();
    }
    public class UserGroupSettingsDto
    {
        public long Id { get; set; }
        public string? Base64Image { get; set; }
        public string Username { get; set; } = "";
        public string? Nickname { get; set; }
        public bool IsAdmin { get; set; }
    }
}
