using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitSync.Data;
using SplitSync.Models;
using System.Security.Cryptography.X509Certificates;

namespace SplitSync.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        private long CurrentUserId()
        {
            return long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        }

        public async Task<IActionResult> Index()
        {
            long userId = CurrentUserId();

            var groups = await _context.Groups
                .Where(g => _context.GroupsUsers.Any(gu => gu.GroupId == g.Id && gu.UserId == CurrentUserId()))
                .ToListAsync();

            if (groups == null || groups.Count == 0)
            {
                var emptyVm = new HomeViewModel
                {
                    DeoGrupe = false
                };
                ViewBag.UsernameCookie = Request.Cookies["username"];
                return View(emptyVm);
            }

            var vm = await GetEventsAsync(userId, null);
            ViewBag.UsernameCookie = Request.Cookies["username"];
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> LoadMoreEvents(DateTime before)
        {
            long userId = CurrentUserId();
            var vm = await GetEventsAsync(userId, before);
            return Json(vm);
        }

        private async Task<HomeViewModel> GetEventsAsync(long userId, DateTime? beforeDate)
        {
            var groups = await _context.Groups
                .Where(g => _context.GroupsUsers.Any(gu => gu.GroupId == g.Id && gu.UserId == userId))
                .ToListAsync();

            List<long> groupIds = groups.Select(g => g.Id).ToList();

            List<HomeEventHelper> helperList = new List<HomeEventHelper>();

            // Najskorijih 20 racuna iz user-ovih grupa, ako ima nekih racuna, uzimamo detalje posle da ne povlacimo velike podatke nepotrebno
            if (beforeDate.HasValue)
            {
                helperList.AddRange(
                    await _context.Racuns
                        .Where(r => groupIds.Contains(r.GroupId) && r.CreatedAtUtc < beforeDate.Value)
                        .OrderByDescending(r => r.CreatedAtUtc)
                        .Select(r => new HomeEventHelper
                        {
                            Id = r.Id,
                            Datum = r.CreatedAtUtc,
                            IsRacun = true
                        })
                        .Take(20)
                        .ToListAsync());
            }
            else
            {
                helperList.AddRange(
                    await _context.Racuns
                        .Where(r => groupIds.Contains(r.GroupId))
                        .OrderByDescending(r => r.CreatedAtUtc)
                        .Select(r => new HomeEventHelper
                        {
                            Id = r.Id,
                            Datum = r.CreatedAtUtc,
                            IsRacun = true
                        })
                        .Take(20)
                        .ToListAsync());
            }

            // Najskorijih 20 anketa iz user-ovih grupa, izvlace se svi relevantni podaci odmah
            var ankete = new List<dynamic>();
            if (beforeDate.HasValue)
            {
                ankete = await _context.Anketas
                    .Where(r => groupIds.Contains(r.GroupId) && r.CreatedAtUtc < beforeDate.Value)
                    .OrderByDescending(a => a.CreatedAtUtc)
                    .Select(a => new
                    {
                        Id = a.Id,
                        Datum = a.CreatedAtUtc,
                        IsAnketa = true,
                        AnketaNaziv = a.Naziv,
                        AnketaStarted = a.HasStarted,
                        AnketaFinished = a.IsFinished,
                        UserId = a.CreatorId,
                        GrupaId = a.GroupId,
                        UserVecGlasao = a.AnketaAnswers.Any(aa => aa.UserId == userId),
                    })
                    .Take(20)
                    .ToListAsync<dynamic>();
            }
            else
            {
                ankete = await _context.Anketas
                    .Where(r => groupIds.Contains(r.GroupId))
                    .OrderByDescending(a => a.CreatedAtUtc)
                    .Select(a => new
                    {
                        Id = a.Id,
                        Datum = a.CreatedAtUtc,
                        IsAnketa = true,
                        AnketaNaziv = a.Naziv,
                        AnketaStarted = a.HasStarted,
                        AnketaFinished = a.IsFinished,
                        UserId = a.CreatorId,
                        GrupaId = a.GroupId,
                        UserVecGlasao = a.AnketaAnswers.Any(aa => aa.UserId == userId),
                    })
                    .Take(20)
                    .ToListAsync<dynamic>();
            }

            helperList.AddRange(ankete.Select(a => new HomeEventHelper
            {
                IsAnketa = true,
                Datum = a.Datum,
                Id = a.Id
            }));

            // Najskorijih 20 dogadjaja iz user-ovih grupa, ako ima nekih racuna, uzimamo detalje posle da ne povlacimo velike podatke nepotrebno
            if (beforeDate.HasValue)
            {
                helperList.AddRange(await _context.Dogadjaji
                        .Where(r => groupIds.Contains(r.GrupaId) && r.VremeDogadjaja < beforeDate.Value)
                        .OrderByDescending(d => d.VremeDogadjaja)
                        .Select(d => new HomeEventHelper
                        {
                            Id = d.Id,
                            Datum = d.VremeDogadjaja,
                            IsDogadjaj = true
                        })
                        .Take(20)
                        .ToListAsync());
            }
            else
            {
                helperList.AddRange(await _context.Dogadjaji
                        .Where(r => groupIds.Contains(r.GrupaId))
                        .OrderByDescending(d => d.VremeDogadjaja)
                        .Select(d => new HomeEventHelper
                        {
                            Id = d.Id,
                            Datum = d.VremeDogadjaja,
                            IsDogadjaj = true
                        })
                        .Take(20)
                        .ToListAsync());
            }

            // Najskorijih 20 racuna iz user-ovih grupa
            var shoppingListItems = new List<dynamic>();
            if (beforeDate.HasValue)
            {
                shoppingListItems = await _context.ShoppingListaItems
                    .Where(r => groupIds.Contains(r.GroupId) && r.NabavioUserId == null && r.TrazenoUtc < beforeDate.Value)
                    .OrderByDescending(s => s.TrazenoUtc)
                    .Select(s => new
                    {
                        Id = s.Id,
                        Datum = s.TrazenoUtc,
                        IsShoppingListItem = true,
                        Naziv = s.Naziv,
                        UserId = s.TrazioUserId,
                        GrupaId = s.GroupId
                    })
                    .Take(20)
                    .ToListAsync<dynamic>();
            }
            else
            {
                shoppingListItems = await _context.ShoppingListaItems
                    .Where(r => groupIds.Contains(r.GroupId) && r.NabavioUserId == null)
                    .OrderByDescending(s => s.TrazenoUtc)
                    .Select(s => new
                    {
                        Id = s.Id,
                        Datum = s.TrazenoUtc,
                        IsShoppingListItem = true,
                        Naziv = s.Naziv,
                        UserId = s.TrazioUserId,
                        GrupaId = s.GroupId
                    })
                    .Take(20)
                    .ToListAsync<dynamic>();
            }

            helperList.AddRange(shoppingListItems.Select(a => new HomeEventHelper
            {
                IsShoppingListItem = true,
                Datum = a.Datum,
                Id = a.Id
            }));

            // Sortiraju se svi eventovi opadajuce po datumu i uzima se prvih 21 da bi se proverilo da li ima jos
            var recentEvents = helperList
                .OrderByDescending(e => e.Datum)
                .Take(21)
                .ToList();

            bool hasMoreEvents = recentEvents.Count > 20;
            if (hasMoreEvents)
            {
                recentEvents = recentEvents.Take(20).ToList();
            }

            // Onda uzimamo informacije o dogadjajima i racunima koji treba da se vrate u view model
            List<HomeEventDto> eventDtos = new List<HomeEventDto>();
            var racunIdsToFetch = recentEvents.Where(e => e.IsRacun).Select(e => e.Id).ToList();
            var racuniDict = await _context.Racuns
                .Where(r => racunIdsToFetch.Contains(r.Id))
                .Include(r => r.Items)
                .ToDictionaryAsync(r => r.Id);

            var dogadjajIdsToFetch = recentEvents.Where(e => e.IsDogadjaj).Select(e => e.Id).ToList();
            var dogadjajiDict = await _context.Dogadjaji
                .Where(d => dogadjajIdsToFetch.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id);

            List<long> userIds = new List<long> { userId };
            for (int i = 0; i < recentEvents.Count; i++)
            {
                if (recentEvents[i].IsRacun)
                {
                    List<HomeRacunItemDto> racunItems = new List<HomeRacunItemDto>();
                    var r = racuniDict[recentEvents[i].Id];
                    foreach (var item in r.Items)
                    {
                        racunItems.Add(new HomeRacunItemDto
                        {
                            UserId = item.UserId,
                            Iznos = item.Iznos,
                            DeoRacuna = item.DeoRacuna
                        });
                    }
                    eventDtos.Add(new HomeEventDto
                    {
                        Datum = r.CreatedAtUtc,
                        GroupId = r.GroupId,
                        CreatedByUserId = r.CreatorUserId ?? -1,
                        IsRacun = true,
                        RacunNaziv = r.Naziv,
                        RacunItems = racunItems
                    });
                }
                else if (recentEvents[i].IsAnketa)
                {
                    var a = ankete.First(a => a.Id == recentEvents[i].Id);
                    eventDtos.Add(new HomeEventDto
                    {
                        Datum = a.Datum,
                        GroupId = a.GrupaId,
                        CreatedByUserId = a.UserId,
                        IsAnketa = true,
                        AnketaNaziv = a.AnketaNaziv,
                        AnketaStarted = a.AnketaStarted,
                        AnketaFinished = a.AnketaFinished,
                        AnketaUserVecGlasao = a.UserVecGlasao,
                        AnketaId = a.Id
                    });
                }
                else if (recentEvents[i].IsDogadjaj)
                {
                    var d = dogadjajiDict[recentEvents[i].Id];
                    eventDtos.Add(new HomeEventDto
                    {
                        Datum = d.VremeDogadjaja,
                        GroupId = d.GrupaId,
                        CreatedByUserId = d.CreatorId,
                        IsDogadjaj = true,
                        DogadjajNaziv = d.Naziv,
                        DogadjajSlikaBase64 = (d.Slika != null && d.Slika.Length > 0) ? Convert.ToBase64String(d.Slika) : null,
                        DogadjajOpis = d.Opis,
                        DogadjajLokacija = d.Lokacija
                    });
                }
                else if (recentEvents[i].IsShoppingListItem)
                {
                    var s = shoppingListItems.First(s => s.Id == recentEvents[i].Id);
                    eventDtos.Add(new HomeEventDto
                    {
                        Datum = s.Datum,
                        GroupId = s.GrupaId,
                        CreatedByUserId = s.UserId,
                        IsShoppingListItem = true,
                        ShoppingListItemNaziv = s.Naziv
                    });
                }
            }

            foreach (var ev in eventDtos)
            {
                if (!userIds.Contains(ev.CreatedByUserId))
                {
                    userIds.Add(ev.CreatedByUserId);
                }
                if (ev.RacunItems != null)
                {
                    foreach (var ri in ev.RacunItems)
                    {
                        if (!userIds.Contains(ri.UserId))
                        {
                            userIds.Add(ri.UserId);
                        }
                    }
                }
            }

            var vm = new HomeViewModel
            {
                Events = eventDtos,

                Grupe = groups.Select(g => new HomeGrupeInfoDto
                {
                    Id = g.Id,
                    Naziv = g.Name,
                    SlikaBase64 = (g.Slika != null && g.Slika.Length > 0) ? Convert.ToBase64String(g.Slika) : null
                }).ToList(),

                Useri = await _context.Users
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new HomeUserInfoDto
                    {
                        Id = u.Id,
                        Username = u.Username ?? u.Email,
                        SlikaBase64 = (u.Slika != null && u.Slika.Length > 0) ? Convert.ToBase64String(u.Slika) : null
                    })
                    .ToListAsync(),

                HasMoreEvents = hasMoreEvents
            };

            return vm;
        }
    }
}