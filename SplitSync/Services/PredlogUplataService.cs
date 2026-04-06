using SplitSync.Models;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace SplitSync.Services
{
    public class StanjeKorisnikaGrupe
    {
        public long GroupUserId { get; set; }
        public string Username { get; set; } = "";
        public string? Nickname { get; set; }
        public double UkupanUplacenIznos { get; set; } = 0;
        public double UkupanTrosak { get; set; } = 0;
        public double TrenutnoStanje { get; set; } = 0;
    }

    public static class PredlogUplataService
    {
        public static List<JedanPredlogUplataViewModel> GetPredlogUplata(List<RacuniDto> racuni, List<MemberItem> clanovi)
        {
            return GetPredlogUplataFromStanja(GetStanjaKorisnikaUGrupi(racuni, clanovi));
        }

        public static List<StanjeKorisnikaGrupe> GetStanjaKorisnikaUGrupi(List<RacuniDto> racuni, List<MemberItem> clanovi)
        {
            Dictionary<long, StanjeKorisnikaGrupe> stanja = new Dictionary<long, StanjeKorisnikaGrupe>();
            foreach(var clan in clanovi)
            {
                stanja.Add(clan.UserId, new StanjeKorisnikaGrupe { GroupUserId = clan.UserId, Username = clan.Username, Nickname = clan.Nickname });
            }

            foreach(var racun in racuni)
            {
                foreach(var racunItem in racun.Items)
                {
                    stanja[racunItem.UserId].UkupanUplacenIznos += racunItem.Iznos;
                    stanja[racunItem.UserId].UkupanTrosak += racunItem.DeoRacuna;
                }
            }

            List<StanjeKorisnikaGrupe> rezultat = new List<StanjeKorisnikaGrupe>();
            foreach(var stanje in stanja.Values)
            {
                stanje.TrenutnoStanje = stanje.UkupanUplacenIznos - stanje.UkupanTrosak;
                rezultat.Add(stanje);
            }

            if (rezultat.Select(s => s.TrenutnoStanje).Sum() < -1 || rezultat.Select(s => s.TrenutnoStanje).Sum() > 1)
                throw new Exception("Nemoguce je doci na nulu na osnovu primljenih racuna.");

            return rezultat;
        }

        public static List<JedanPredlogUplataViewModel> GetPredlogUplataFromStanja(List<StanjeKorisnikaGrupe> stanja)
        {
            if (stanja.Select(s => s.TrenutnoStanje).Sum() < -1 || stanja.Select(s => s.TrenutnoStanje).Sum() > 1)
                throw new Exception("Nemoguce je doci na nulu na osnovu ovih stanja.");

            if (stanja.Count == 0 || !stanja.Any(s => s.TrenutnoStanje != 0))
                return new List<JedanPredlogUplataViewModel>();

            List<JedanPredlogUplataViewModel> rezultat = new List<JedanPredlogUplataViewModel>();

            // Vrti se petlja dokle god ima neresenih dugova
            while (stanja.Any(s => s.TrenutnoStanje < -1 || s.TrenutnoStanje > 1))
            {
                // Pretrazuju se sva stanja da se vidi da li ima potencijalnih uplata koje resavaju dva usera odjednom
                for(int i = 0; i < stanja.Count - 1; i++)
                {
                    if (stanja[i].TrenutnoStanje > 1)
                    {
                        for(int j = i + 1; j < stanja.Count; j++)
                        {
                            if (stanja[i].TrenutnoStanje + stanja[j].TrenutnoStanje > -1 && stanja[i].TrenutnoStanje + stanja[j].TrenutnoStanje < 1)
                            {
                                rezultat.Add(new JedanPredlogUplataViewModel
                                {
                                    Iznos = stanja[i].TrenutnoStanje,
                                    RecieverId = stanja[i].GroupUserId,
                                    RecieverNickname = stanja[i].Nickname,
                                    RecieverUsername = stanja[i].Username,
                                    SenderId = stanja[j].GroupUserId,
                                    SenderNickname = stanja[j].Nickname,
                                    SenderUsername = stanja[j].Username,
                                });
                                stanja[i].TrenutnoStanje = 0;
                                stanja[j].TrenutnoStanje = 0;
                                break;
                            }
                        }
                    }
                    else if (stanja[i].TrenutnoStanje < -1)
                    {
                        for (int j = i + 1; j < stanja.Count; j++)
                        {
                            if (stanja[j].TrenutnoStanje + stanja[i].TrenutnoStanje > -1 && stanja[j].TrenutnoStanje + stanja[i].TrenutnoStanje < 1)
                            {
                                rezultat.Add(new JedanPredlogUplataViewModel
                                {
                                    Iznos = stanja[j].TrenutnoStanje,
                                    RecieverId = stanja[j].GroupUserId,
                                    RecieverUsername = stanja[j].Username,
                                    RecieverNickname = stanja[j].Nickname,
                                    SenderId = stanja[i].GroupUserId,
                                    SenderUsername = stanja[i].Username,
                                    SenderNickname = stanja[i].Nickname,
                                });
                                stanja[i].TrenutnoStanje = 0;
                                stanja[j].TrenutnoStanje = 0;
                                break;
                            }
                        }
                    }
                }

                if (!stanja.Any(s => s.TrenutnoStanje < -1 || s.TrenutnoStanje > 1))
                    continue;

                // Ako nije nadjen precizan transfer, nalazi se user sa najvecim dugom i user za najmanjim dugom i radi se najveca moguca uplata
                int maxUplataId = -1, minUplataId = -1;
                double maxTrenutnoStanje = -1, minTrenutnoStanje = 1;
                for (int i = 0; i < stanja.Count; i++)
                {
                    if (stanja[i].TrenutnoStanje > maxTrenutnoStanje)
                    {
                        maxTrenutnoStanje = stanja[i].TrenutnoStanje;
                        maxUplataId = i;
                    }
                    else if (stanja[i].TrenutnoStanje < minTrenutnoStanje)
                    {
                        minTrenutnoStanje = stanja[i].TrenutnoStanje;
                        minUplataId = i;
                    }
                }

                // Ili je gotovo ili negde nije uspelo tako da se vraca rezultat
                if (maxUplataId == -1 || minUplataId == -1)
                    return rezultat;

                if(maxTrenutnoStanje > -minTrenutnoStanje)
                {
                    rezultat.Add(new JedanPredlogUplataViewModel
                    {
                        Iznos = -minTrenutnoStanje,
                        RecieverId = stanja[maxUplataId].GroupUserId,
                        RecieverNickname = stanja[maxUplataId].Nickname,
                        RecieverUsername = stanja[maxUplataId].Username,
                        SenderId = stanja[minUplataId].GroupUserId,
                        SenderNickname = stanja[minUplataId].Nickname,
                        SenderUsername = stanja[minUplataId].Username,
                    });
                    stanja[maxUplataId].TrenutnoStanje = stanja[maxUplataId].TrenutnoStanje + stanja[minUplataId].TrenutnoStanje;
                    stanja[minUplataId].TrenutnoStanje = 0;
                }
                else
                {
                    rezultat.Add(new JedanPredlogUplataViewModel
                    {
                        Iznos = maxTrenutnoStanje,
                        RecieverId = stanja[maxUplataId].GroupUserId,
                        RecieverNickname = stanja[maxUplataId].Nickname,
                        RecieverUsername = stanja[maxUplataId].Username,
                        SenderId = stanja[minUplataId].GroupUserId,
                        SenderUsername = stanja[minUplataId].Username,
                        SenderNickname = stanja[minUplataId].Nickname,
                    });
                    stanja[maxUplataId].TrenutnoStanje = 0;
                    stanja[minUplataId].TrenutnoStanje = stanja[minUplataId].TrenutnoStanje + stanja[maxUplataId].TrenutnoStanje;
                }
            }

            return rezultat;
        }
    }
}
