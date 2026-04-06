using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SplitSync.Data;
using SplitSync.Entities;
using SplitSync.Models;
using SplitSync.Services;
using System.Buffers.Text;
using System.Runtime.CompilerServices;

namespace SplitSync.Controllers
{
    [Authorize]
    public class GroupController : Controller
    {
        private readonly AppDbContext _context;

        public GroupController(AppDbContext context)
        {
            _context = context;
        }

        private long CurrentUserId()
        {
            return long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        }

        public async Task<IActionResult> Index(long id, string? errorMessage)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            var currentGroupUser = group.Members.FirstOrDefault(m => m.UserId == uid);
            if (currentGroupUser == null)
                return RedirectToAction("Index", "Groups");

            Dictionary<long, string?> nicknames = group.Members.ToDictionary(m => m.UserId, m => m.Nickname);

            var vm = new GroupDashboardViewModel
            {
                GroupId = group.Id,
                GroupName = group.Name,
                ImageBase64 = (group.Slika == null || group.Slika.Length <= 0) ? null : Convert.ToBase64String(group.Slika),
                CurrentUserIsAdmin = currentGroupUser.IsAdmin,
                ErrorMessage = errorMessage,
                Members = group.Members
                    .OrderBy(m => m.User.Username ?? m.User.Email)
                    .Select(m => new MemberItem
                    {
                        UserId = m.UserId,
                        Username = m.User.Username,
                        Nickname = nicknames.ContainsKey(m.UserId) ? nicknames[m.UserId] : null,
                        IsAdmin = m.IsAdmin
                    })
                    .ToList()
            };

            ViewData["TrenutnaGrupaId"] = group.Id;
            ViewData["TrenutnaGrupaNaziv"] = group.Name;

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Invite(long groupId, string identifier)
        {
            var userId = CurrentUserId();
            string? errMsg = null;

            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == userId))
                return RedirectToAction("Index", "Groups");

            if (string.IsNullOrWhiteSpace(identifier))
            {
                errMsg = "Unesite username ili email.";
                return RedirectToAction("Index", new { id = groupId, errorMessage = errMsg });
            }

            var ident = identifier.Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                (!string.IsNullOrEmpty(u.Username) && u.Username.ToLower() == ident)
                || u.Email.ToLower() == ident);

            if (user == null)
            {
                errMsg = "Korisnik nije pronađen.";
                return RedirectToAction("Index", new { id = groupId, errorMessage = errMsg });
            }

            if (user.Id == userId)
            {
                errMsg = "Ne možete pozvati sami sebe.";
                return RedirectToAction("Index", new { id = groupId, errorMessage = errMsg });
            }

            if (await _context.GroupsUsers.AnyAsync(gu => gu.GroupId == groupId && gu.UserId == user.Id))
            {
                errMsg = "Korisnik je već član.";
                return RedirectToAction("Index", new { id = groupId, errorMessage = errMsg });
            }

            var existsInvite = await _context.GroupInvitations.AnyAsync(i => i.GroupId == groupId && i.InvitedUserId == user.Id);
            if (existsInvite)
            {
                errMsg = "Pozivnica je već poslata.";
                return RedirectToAction("Index", new { id = groupId, errorMessage = errMsg });
            }

            _context.GroupInvitations.Add(new GroupInvitation
            {
                GroupId = groupId,
                InvitedUserId = user.Id,
                InvitedByUserId = userId,
                CreatedAtUtc = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            errMsg = "Pozivnica poslata.";
            return RedirectToAction("Index", new { id = groupId, errorMessage = errMsg });
        }

        // Racuni
        public async Task<IActionResult> Racuni(long id)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null) return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid))
                return RedirectToAction("Index", "Groups");

            bool isAdmin = group.Members.Any(m => m.UserId == uid && m.IsAdmin);

            Dictionary<long, string?> nicknames = group.Members.ToDictionary(m => m.UserId, m => m.Nickname);

            RacuniViewModel vm = new RacuniViewModel();
            vm.Racuni = await _context.Racuns
                .Where(r => r.GroupId == group.Id)
                .Include(r => r.Items)
                .ThenInclude(r => r.User)
                .Select(r => new RacuniDto
                {
                    Naziv = r.Naziv ?? "",
                    Id = r.Id,
                    Iznos = r.Iznos,
                    CreatedAtUtc = r.CreatedAtUtc,
                    UserCanDelete = isAdmin || r.CreatorUserId == uid,
                    Items = r.Items.Select(ri => new RacunItemDto
                    {
                        Id = ri.Id,
                        Iznos = ri.Iznos,
                        UserId = ri.UserId,
                        Nickname = nicknames.ContainsKey(ri.UserId) ? nicknames[ri.UserId] : null,
                        Username = ri.User!.Username,
                        DeoRacuna = ri.DeoRacuna,
                    }).ToList()
                })
                .OrderByDescending(r => r.CreatedAtUtc)
                .ToListAsync();
            vm.GroupId = group.Id;

            ViewData["TrenutnaGrupaId"] = group.Id;
            ViewData["TrenutnaGrupaNaziv"] = group.Name;

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRacun(long groupId, long racunId)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null) return RedirectToAction("Index", "Groups");

            var currentGroupUser = group.Members.FirstOrDefault(m => m.UserId == uid);
            if (currentGroupUser == null)
                return RedirectToAction("Index", "Groups");

            var racun = await _context.Racuns.FirstOrDefaultAsync(r => r.Id == racunId && r.GroupId == groupId && (r.CreatorUserId == uid || currentGroupUser.IsAdmin));
            if (racun == null)
                return RedirectToAction("Racuni", new { id = groupId });

            _context.Racuns.Remove(racun);
            await _context.SaveChangesAsync();

            return RedirectToAction("Racuni", new { id = groupId });
        }

        public async Task<IActionResult> NoviRacun(long id, string? errorMessage)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null) return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid))
                return RedirectToAction("Index", "Groups");

            Dictionary<long, string?> nicknames = group.Members.ToDictionary(m => m.UserId, m => m.Nickname);

            var vm = new NoviRacunViewModel
            {
                GroupId = group.Id,
                Clanovi = group.Members
                    .OrderBy(m => m.User.Username ?? m.User.Email)
                    .Select(m => new NoviRacunClan
                    {
                        UserId = m.UserId,
                        Nickname = nicknames.ContainsKey(m.UserId) ? nicknames[m.UserId] : null,
                        Username = m.User.Username,
                        IsSelected = false,
                        Iznos = null,
                        DeoRacuna = null
                    })
                    .ToList()
            };
            vm.ErrorMessage = errorMessage;

            ViewData["TrenutnaGrupaId"] = group.Id;
            ViewData["TrenutnaGrupaNaziv"] = group.Name;

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> NoviRacun(NoviRacunViewModel vm)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(g => g.Id == vm.GroupId);

            if (group == null) return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid))
                return RedirectToAction("Index", "Groups");

            bool isAdmin = group.Members.Any(m => m.UserId == uid && m.IsAdmin);

            Dictionary<long, string?> nicknames = group.Members.ToDictionary(m => m.UserId, m => m.Nickname);

            if (vm.Clanovi != null)
            {
                foreach (var member in group.Members)
                {
                    if (!vm.Clanovi.Any(c => c.UserId == member.UserId))
                    {
                        vm.Clanovi.Add(new NoviRacunClan
                        {
                            UserId = member.UserId,
                            Nickname = nicknames.ContainsKey(member.UserId) ? nicknames[member.UserId] : null,
                            Username = member.User.Username,
                            IsSelected = false,
                            Iznos = null,
                            DeoRacuna = null
                        });
                    }
                    else
                    {
                        var existing = vm.Clanovi.First(c => c.UserId == member.UserId);
                        existing.Nickname = nicknames.ContainsKey(member.UserId) ? nicknames[member.UserId] : null;
                        existing.Username = member.User.Username;
                    }
                }
                vm.Clanovi.RemoveAll(cl => cl.UserId == 0);
            }

            if (vm.Clanovi == null || !vm.Clanovi.Any(c => c.IsSelected))
            {
                vm.ErrorMessage = "Izaberite bar jednog učesnika.";
                ViewData["TrenutnaGrupaId"] = group.Id;
                ViewData["TrenutnaGrupaNaziv"] = group.Name;
                return View(vm);
            }

            if (vm.Naziv != null && vm.Naziv.Trim().Length > 30)
            {
                vm.ErrorMessage = "Naziv računa ne sme biti duži od 30 karaktera!";
                ViewData["TrenutnaGrupaId"] = group.Id;
                ViewData["TrenutnaGrupaNaziv"] = group.Name;
                return View(vm);
            }

            var izabraniClanovi = vm.Clanovi!.Where(c => c.IsSelected && c.UserId != 0).ToList();

            vm.ErrorMessage = null;
            foreach (var izabraniClan in izabraniClanovi)
            {
                if (izabraniClan.Iznos != null && izabraniClan.Iznos < 0)
                    vm.ErrorMessage = "Iznos koliko je član platio ne sme da bude negativan broj.";
                if (izabraniClan.DeoRacuna != null && izabraniClan.DeoRacuna < 0)
                    vm.ErrorMessage = "Deo računa člana ne sme da bude negativan broj.";
            }

            double ukupanIznos = izabraniClanovi.Sum(r => r.Iznos ?? 0);
            if (ukupanIznos <= 0)
                vm.ErrorMessage = "Račun mora da ima validan iznos!";
            if (izabraniClanovi.Where(c => c.DeoRacuna != null).Select(c => c.DeoRacuna).Sum() > ukupanIznos)
                vm.ErrorMessage = "Nije dozvoljeno da su članovi platili manje nego što su trebali!";
            // Ako je unet deo racuna za svaku osobu, mora da bude jednak ukupnom iznosu
            if (!izabraniClanovi.Any(c => c.DeoRacuna == null || c.DeoRacuna == 0) && izabraniClanovi.Select(c => c.DeoRacuna).Sum() != ukupanIznos)
                vm.ErrorMessage = "Zbir suma koje ste uneli da su članovi platili nije ista kao suma delova računa koje su članovi trebali da plate!";

            if (vm.ErrorMessage != null)
            {
                ViewData["TrenutnaGrupaId"] = group.Id;
                ViewData["TrenutnaGrupaNaziv"] = group.Name;
                return View(vm);
            }

            if (!string.IsNullOrWhiteSpace(vm.Naziv) && vm.Naziv.Length > 30)
            {
                vm.ErrorMessage = "Naziv računa ne sme da bude duži od 30 karaktera.";
                ViewData["TrenutnaGrupaId"] = group.Id;
                ViewData["TrenutnaGrupaNaziv"] = group.Name;
                return View(vm);
            }

            // Odradjena validacija, ako je doslo ovde, moze da se podeli trosak lepo
            int brojClanovaKojiDeleOstatakRacuna = izabraniClanovi.Count(c => c.DeoRacuna == null || c.DeoRacuna == 0);
            if (brojClanovaKojiDeleOstatakRacuna > 0)
            {
                double deoRacunaZaOstale = (ukupanIznos - izabraniClanovi.Where(c => c.DeoRacuna != null).Select(c => c.DeoRacuna!.Value).Sum()) / brojClanovaKojiDeleOstatakRacuna;
                foreach (var clan in izabraniClanovi)
                {
                    if (clan.DeoRacuna == null || clan.DeoRacuna == 0)
                        clan.DeoRacuna = deoRacunaZaOstale;
                }
            }

            var racun = new Racun
            {
                GroupId = group.Id,
                Naziv = vm.Naziv,
                Iznos = ukupanIznos,
                CreatorUserId = uid
            };
            _context.Racuns.Add(racun);
            await _context.SaveChangesAsync();

            foreach (var r in izabraniClanovi)
            {
                racun.Items.Add(new RacunItem
                {
                    RacunId = racun.Id,
                    UserId = r.UserId,
                    Iznos = r.Iznos ?? 0,
                    DeoRacuna = r.DeoRacuna!.Value
                });
            }
            await _context.SaveChangesAsync();

            return RedirectToAction("Racuni", new { id = group.Id });
        }

        // Predlog uplata
        public async Task<IActionResult> PredlogUplata(long id)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null) return RedirectToAction("Index", "Groups");

            var currentGroupUser = group.Members.FirstOrDefault(m => m.UserId == uid);
            if (currentGroupUser == null)
                return RedirectToAction("Index", "Groups");

            Dictionary<long, string?> nicknames = group.Members.ToDictionary(m => m.UserId, m => m.Nickname);

            PredlogUplataViewModel vm = new PredlogUplataViewModel();
            try
            {
                var members = group.Members
                    .OrderBy(m => m.User.Username ?? m.User.Email)
                    .Select(m => new MemberItem
                    {
                        UserId = m.UserId,
                        Username = m.User.Username,
                        Nickname = m.Nickname,
                        IsAdmin = m.IsAdmin
                    })
                    .ToList();

                var racuni = await _context.Racuns
                    .Where(r => r.GroupId == group.Id)
                    .Include(r => r.Items)
                    .ThenInclude(r => r.User)
                    .Select(r => new RacuniDto
                    {
                        Naziv = r.Naziv ?? "",
                        Id = r.Id,
                        Iznos = r.Iznos,
                        CreatedAtUtc = r.CreatedAtUtc,
                        Items = r.Items.Select(ri => new RacunItemDto
                        {
                            Id = ri.Id,
                            Iznos = ri.Iznos,
                            UserId = ri.UserId,
                            Nickname = nicknames.ContainsKey(ri.UserId) ? nicknames[ri.UserId] : null,
                            Username = ri.User!.Username,
                            DeoRacuna = ri.DeoRacuna,
                        }).ToList()
                    })
                    .ToListAsync();

                List<long> idUseraZaMembere = new List<long>();
                foreach (var racun in racuni)
                    foreach (var item in racun.Items)
                        if (!members.Any(m => m.UserId == item.UserId) && !idUseraZaMembere.Contains(item.UserId))
                            idUseraZaMembere.Add(item.UserId);

                members.AddRange(await _context.Users
                    .Select(u => new MemberItem
                    {
                        IsAdmin = false,
                        Nickname = null,
                        UserId = u.Id,
                        Username = u.Username
                    })
                    .Where(u => idUseraZaMembere.Contains(u.UserId))
                    .ToListAsync());

                vm.PredlogUplata = PredlogUplataService.GetPredlogUplata(racuni, members);
            }
            catch (Exception ex)
            {
                vm.ErrorMessage = "Doslo je do greske sa predlogom uplata: " + ex.Message;
            }

            ViewData["TrenutnaGrupaId"] = group.Id;
            ViewData["TrenutnaGrupaNaziv"] = group.Name;

            return View(vm);
        }

        // Ankete
        public async Task<IActionResult> Ankete(long id)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null) return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid))
                return RedirectToAction("Index", "Groups");

            bool isAdmin = group.Members.Any(m => m.UserId == uid && m.IsAdmin);

            var sveAnkete = await _context.Anketas
                .Where(a => a.GroupId == id)
                .Select(a => new AnketaSimpleViewModelDto
                {
                    CreatedAtUtc = a.CreatedAtUtc,
                    BrojOpcija = a.AnketaOptions.Count,
                    Id = a.Id,
                    Naziv = a.Naziv,
                    UserVecGlasao = a.AnketaAnswers.Any(aa => aa.UserId == uid),
                    HasStarted = a.HasStarted,
                    IsFinished = a.IsFinished,
                    MozeDaObrise = isAdmin || a.CreatorId == uid,
                    Rezultati = a.AnketaOptions.Select(o => new RezultatiZavrseneAnkete
                    {
                        Naziv = o.Naziv,
                        ProsecnaOcena = o.AnketaAnswerOptions.Count > 0 ? o.AnketaAnswerOptions.Select(aao => aao.Ocena).Average() : 0
                    })
                    .OrderByDescending(o => o.ProsecnaOcena)
                    .ToList()
                })
                .OrderByDescending(a => a.CreatedAtUtc)
                .ToListAsync();

            var vm = new AnketeViewModel
            {
                GrupaId = group.Id,
                AnketeGlasanje = sveAnkete.Where(a => !a.UserVecGlasao && a.HasStarted && !a.IsFinished).ToList(),
                AnketeZaEdit = sveAnkete.Where(a => !a.HasStarted).ToList(),
                ZavrseneAnkete = sveAnkete.Where(a => a.IsFinished).ToList(),
                AnketeUToku = sveAnkete.Where(a => a.UserVecGlasao && a.HasStarted && !a.IsFinished).ToList(),
            };

            ViewData["TrenutnaGrupaId"] = group.Id;
            ViewData["TrenutnaGrupaNaziv"] = group.Name;

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ZavrsiAnketuPrerano(long grupaId, long anketaId)
        {
            var uid = CurrentUserId();

            var isAdmin = await _context.GroupsUsers
                .AnyAsync(g => g.GroupId == grupaId && g.IsAdmin && g.UserId == uid);

            var anketa = await _context.Anketas
                .Include(a => a.AnketaAnswers)
                .FirstOrDefaultAsync(a => a.Id == anketaId && a.GroupId == grupaId);

            if (anketa == null)
                return RedirectToAction("Ankete", new { id = grupaId });

            if (anketa.CreatorId != uid && !isAdmin)
                return RedirectToAction("Ankete", new { id = grupaId });

            if (!anketa.AnketaAnswers.Any())
                return RedirectToAction("Ankete", new { id = grupaId });

            anketa.IsFinished = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Ankete", new { id = grupaId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAnketa(long grupaId, long anketaId)
        {
            var uid = CurrentUserId();

            var isAdmin = await _context.GroupsUsers
                .AnyAsync(g => g.GroupId == grupaId && g.IsAdmin && g.UserId == uid);

            var anketa = await _context.Anketas
                .FirstOrDefaultAsync(a => a.Id == anketaId && a.GroupId == grupaId);

            if (anketa == null)
                return RedirectToAction("Ankete", new { id = grupaId });

            if (anketa.CreatorId != uid && !isAdmin)
                return RedirectToAction("Ankete", new { id = grupaId });

            _context.Anketas.Remove(anketa);
            await _context.SaveChangesAsync();

            return RedirectToAction("Ankete", new { id = grupaId });
        }

        public async Task<IActionResult> AnketaCreate(long grupaId)
        {
            var uid = CurrentUserId();
            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == grupaId);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid))
                return RedirectToAction("Index", "Groups");

            var vm = new AnketaCreateViewModel
            {
                GrupaId = grupaId,
                StartOdmah = false,
                Opcije = new() { "", "" }
            };

            ViewData["TrenutnaGrupaId"] = group.Id;
            ViewData["TrenutnaGrupaNaziv"] = group.Name;

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AnketaCreate(AnketaCreateViewModel vm)
        {
            var uid = CurrentUserId();
            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == vm.GrupaId);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid))
                return RedirectToAction("Index", "Groups");

            var opcije = new List<string>();
            if (vm.Opcije != null)
                opcije = vm.Opcije.Where(o => o != null && o.Length != 0).Distinct().ToList();

            if (string.IsNullOrWhiteSpace(vm.Naziv))
                vm.ErrorMessage = "Unesite naziv ankete.";
            if (!string.IsNullOrWhiteSpace(vm.Naziv) && vm.Naziv.Trim().Length > 30)
                vm.ErrorMessage = "Naziv ankete ne sme da bude duži od 30 karaktera.";
            if (opcije.Count == 0)
                vm.ErrorMessage = "Unesite bar jednu opciju.";
            if (opcije.Any(o => o.Length > 30))
                vm.ErrorMessage = "Naziv opcije ne sme da bude duži od 30 karaktera.";

            if (vm.ErrorMessage != null)
            {
                ViewData["TrenutnaGrupaId"] = group.Id;
                ViewData["TrenutnaGrupaNaziv"] = group.Name;
                return View(vm);
            }

            var anketa = new Anketa
            {
                GroupId = group.Id,
                CreatorId = uid,
                Naziv = vm.Naziv.Trim(),
                HasStarted = vm.StartOdmah,
                IsFinished = false,
                CreatedAtUtc = DateTime.UtcNow
            };
            _context.Anketas.Add(anketa);
            await _context.SaveChangesAsync();

            foreach (var o in opcije)
            {
                _context.AnketaOptions.Add(new AnketaOption
                {
                    AnketaId = anketa.Id,
                    Naziv = o,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }
            await _context.SaveChangesAsync();

            return RedirectToAction("Ankete", new { id = group.Id });
        }

        public async Task<IActionResult> AnketaEdit(long grupaId, long anketaId, string? errorMessage = null)
        {
            var uid = CurrentUserId();
            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == grupaId);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid))
                return RedirectToAction("Index", "Groups");

            bool isAdmin = group.Members.Any(m => m.UserId == uid && m.IsAdmin);

            var anketa = await _context.Anketas
                .Include(a => a.AnketaOptions)
                .FirstOrDefaultAsync(a => a.Id == anketaId && a.GroupId == grupaId);
            if (anketa == null)
                return RedirectToAction("Ankete", new { id = grupaId });

            var vm = new AnketaEditViewModel
            {
                GrupaId = grupaId,
                AnketaId = anketa.Id,
                Naziv = anketa.Naziv,
                HasStarted = anketa.HasStarted,
                IsCreator = anketa.CreatorId == uid || isAdmin,
                Opcije = anketa.AnketaOptions
                    .OrderBy(o => o.CreatedAtUtc)
                    .Select(o => new AnketaOptionEditVm { Id = o.Id, Naziv = o.Naziv })
                    .ToList(),
                ErrorMessage = errorMessage
            };

            ViewData["TrenutnaGrupaId"] = group.Id;
            ViewData["TrenutnaGrupaNaziv"] = group.Name;
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddOption(AddOptionPostModel m)
        {
            var uid = CurrentUserId();
            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == m.GrupaId);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(mu => mu.UserId == uid))
                return RedirectToAction("Index", "Groups");

            var anketa = await _context.Anketas.Include(a => a.AnketaOptions)
                .FirstOrDefaultAsync(a => a.Id == m.AnketaId && a.GroupId == m.GrupaId);

            if (anketa == null)
                return RedirectToAction("Ankete", new { id = m.GrupaId });

            if (string.IsNullOrWhiteSpace(m.Naziv))
                return RedirectToAction("AnketaEdit", new { grupaId = m.GrupaId, anketaId = m.AnketaId, errorMessage = "Naziv opcije je obavezan." });

            if (m.Naziv.Trim().Length > 30)
                return RedirectToAction("AnketaEdit", new { grupaId = m.GrupaId, anketaId = m.AnketaId, errorMessage = "Naziv opcije ne sme da bude duži od 30 karaktera." });

            if (anketa.HasStarted || anketa.IsFinished)
                return RedirectToAction("Ankete", new { id = m.GrupaId });

            if (anketa.AnketaOptions.Any(o => o.Naziv.ToLower() == m.Naziv.Trim().ToLower()))
                return RedirectToAction("AnketaEdit", new { grupaId = m.GrupaId, anketaId = m.AnketaId, errorMessage = "Opcija već postoji." });

            _context.AnketaOptions.Add(new AnketaOption
            {
                AnketaId = anketa.Id,
                Naziv = m.Naziv.Trim(),
                CreatedAtUtc = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return RedirectToAction("AnketaEdit", new { grupaId = m.GrupaId, anketaId = m.AnketaId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOption(long grupaId, long anketaId, long optionId)
        {
            var uid = CurrentUserId();

            var isAdmin = await _context.GroupsUsers
                .AnyAsync(g => g.GroupId == grupaId && g.IsAdmin && g.UserId == uid);

            var anketa = await _context.Anketas
                .FirstOrDefaultAsync(a => a.Id == anketaId && a.GroupId == grupaId);

            if (anketa == null) return RedirectToAction("Ankete", new { id = grupaId });

            if (anketa.CreatorId != uid && !isAdmin)
                return RedirectToAction("AnketaEdit", new { grupaId, anketaId, errorMessage = "Ne možete brisati opcije ove ankete." });

            if (anketa.HasStarted || anketa.IsFinished)
                return RedirectToAction("AnketaEdit", new { grupaId, anketaId, errorMessage = "Ne možete brisati opcije nakon starta." });

            var opt = await _context.AnketaOptions
                .FirstOrDefaultAsync(o => o.Id == optionId && o.AnketaId == anketaId);
            if (opt != null)
            {
                _context.AnketaOptions.Remove(opt);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("AnketaEdit", new { grupaId, anketaId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAnketaNaziv(UpdateAnketaNazivPostModel m)
        {
            var uid = CurrentUserId();

            var isAdmin = await _context.GroupsUsers
                .AnyAsync(g => g.GroupId == m.GrupaId && g.IsAdmin && g.UserId == uid);

            var anketa = await _context.Anketas.
                FirstOrDefaultAsync(a => a.Id == m.AnketaId && a.GroupId == m.GrupaId);

            if (anketa == null)
                return RedirectToAction("Ankete", new { id = m.GrupaId });

            if (anketa.CreatorId != uid && !isAdmin)
                return RedirectToAction("AnketaEdit", new { grupaId = m.GrupaId, anketaId = m.AnketaId, errorMessage = "Ne možete menjati naziv ove ankete." });

            if (string.IsNullOrWhiteSpace(m.Naziv))
                return RedirectToAction("AnketaEdit", new { grupaId = m.GrupaId, anketaId = m.AnketaId, errorMessage = "Naziv je obavezan." });

            if (m.Naziv.Trim().Length > 30)
                return RedirectToAction("AnketaEdit", new { grupaId = m.GrupaId, anketaId = m.AnketaId, errorMessage = "Naziv ankete ne sme da bude duži od 30 karaktera." });

            anketa.Naziv = m.Naziv.Trim();
            await _context.SaveChangesAsync();

            return RedirectToAction("AnketaEdit", new { grupaId = m.GrupaId, anketaId = m.AnketaId });
        }

        [HttpPost]
        public async Task<IActionResult> StartAnketa(long grupaId, long anketaId)
        {
            var uid = CurrentUserId();

            var isAdmin = await _context.GroupsUsers
                .AnyAsync(g => g.GroupId == grupaId && g.IsAdmin && g.UserId == uid);

            var anketa = await _context.Anketas
                .Include(a => a.AnketaOptions)
                .FirstOrDefaultAsync(a => a.Id == anketaId && a.GroupId == grupaId);

            if (anketa == null)
                return RedirectToAction("Ankete", new { id = grupaId });

            if (anketa.CreatorId != uid && !isAdmin)
                return RedirectToAction("AnketaEdit", new { grupaId, anketaId, errorMessage = "Ne možete pokrenuti anketu." });

            if (anketa.AnketaOptions.Count == 0)
                return RedirectToAction("AnketaEdit", new { grupaId, anketaId, errorMessage = "Dodajte bar jednu opciju pre starta." });

            anketa.HasStarted = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Ankete", new { id = grupaId });
        }

        [HttpPost]
        public async Task<IActionResult> FinishAnketa(long grupaId, long anketaId)
        {
            var uid = CurrentUserId();

            var isAdmin = await _context.GroupsUsers
                .AnyAsync(g => g.GroupId == grupaId && g.IsAdmin && g.UserId == uid);

            var anketa = await _context.Anketas
                .FirstOrDefaultAsync(a => a.Id == anketaId && a.GroupId == grupaId);

            if (anketa == null)
                return RedirectToAction("Ankete", new { id = grupaId });

            if (anketa.CreatorId != uid && !isAdmin)
                return RedirectToAction("AnketaEdit", new { grupaId, anketaId, err = "Samo kreator može završiti anketu." });

            anketa.IsFinished = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Ankete", new { id = grupaId });
        }

        public async Task<IActionResult> AnketaGlasanje(long grupaId, long anketaId)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == grupaId);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid))
                return RedirectToAction("Index", "Groups");

            var anketa = await _context.Anketas
                .Include(a => a.AnketaOptions)
                .Include(a => a.AnketaAnswers)
                .FirstOrDefaultAsync(a => a.Id == anketaId && a.GroupId == grupaId);

            if (anketa == null)
                return RedirectToAction("Ankete", new { id = grupaId });

            if (!anketa.HasStarted || anketa.IsFinished)
                return RedirectToAction("Ankete", new { id = grupaId });

            if (anketa.AnketaAnswers.Any(a => a.UserId == uid))
                return RedirectToAction("Ankete", new { id = grupaId });

            var vm = new AnketaGlasanjeViewModel
            {
                GrupaId = grupaId,
                AnketaId = anketa.Id,
                Naziv = anketa.Naziv,
                Opcije = anketa.AnketaOptions
                    .OrderBy(o => o.CreatedAtUtc)
                    .Select(o => new AnketaGlasanjeOpcijaVm
                    {
                        OpcijaId = o.Id,
                        Naziv = o.Naziv
                    })
                    .ToList()
            };

            ViewData["TrenutnaGrupaId"] = group.Id;
            ViewData["TrenutnaGrupaNaziv"] = group.Name;
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AnketaGlasanje(AnketaGlasanjePostModel m)
        {
            var uid = CurrentUserId();

            var anketa = await _context.Anketas
                .Include(a => a.AnketaOptions)
                .Include(a => a.AnketaAnswers)
                .FirstOrDefaultAsync(a => a.Id == m.AnketaId && a.GroupId == m.GrupaId);

            if (anketa == null)
                return RedirectToAction("Ankete", new { id = m.GrupaId });

            if (!anketa.HasStarted || anketa.IsFinished)
                return RedirectToAction("Ankete", new { id = m.GrupaId });

            if (anketa.AnketaAnswers.Any(a => a.UserId == uid))
                return RedirectToAction("Ankete", new { id = m.GrupaId });

            foreach (var opt in anketa.AnketaOptions)
            {
                if (!m.Ocene.TryGetValue(opt.Id, out var oc) || oc < 1 || oc > 5)
                {
                    return RedirectToAction("AnketaGlasanje", new { grupaId = m.GrupaId, anketaId = m.AnketaId });
                }
            }

            var answer = new AnketaAnswer
            {
                AnketaId = anketa.Id,
                UserId = uid,
                SubmittedAtUtc = DateTime.UtcNow
            };
            _context.AnketaAnswers.Add(answer);
            await _context.SaveChangesAsync();

            foreach (var opt in anketa.AnketaOptions)
            {
                _context.AnketaAnswerOptions.Add(new AnketaAnswerOption
                {
                    AnketaAnswerId = answer.Id,
                    AnketaOptionId = opt.Id,
                    Ocena = m.Ocene[opt.Id]
                });
            }
            await _context.SaveChangesAsync();

            // Provera da li su svi clanovi glasali
            var brojClanova = await _context.GroupsUsers.CountAsync(gu => gu.GroupId == anketa.GroupId);
            var brojGlasova = await _context.AnketaAnswers.CountAsync(x => x.AnketaId == anketa.Id);

            if (brojGlasova >= brojClanova)
            {
                anketa.IsFinished = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Ankete", new { id = m.GrupaId });
        }

        // Dogadjaji
        public async Task<IActionResult> Dogadjaji(long id, string? errorMessage = null)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid))
                return RedirectToAction("Index", "Groups");

            Dictionary<long, string?> nicknames = group.Members.ToDictionary(m => m.UserId, m => m.Nickname);

            var dogadjaji = await _context.Dogadjaji
                .Where(d => d.GrupaId == id)
                .Include(d => d.Creator)
                .Include(d => d.Glasovi)
                    .ThenInclude(gl => gl.User)
                .OrderByDescending(d => d.VremeDogadjaja)
                .ToListAsync();

            var vm = new DogadjajiViewModel
            {
                GrupaId = group.Id,
                ErrorMessage = errorMessage,
                Dogadjaji = dogadjaji.Select(d => new DogadjajListItem
                {
                    Id = d.Id,
                    Naziv = d.Naziv,
                    Opis = d.Opis,
                    Lokacija = d.Lokacija,
                    VremeDogadjaja = d.VremeDogadjaja,
                    CreatedAtUtc = d.CreatedAtUtc,
                    CreatorId = d.CreatorId,
                    CreatorNickname = nicknames.ContainsKey(d.CreatorId) ? nicknames[d.CreatorId] : null,
                    CreatorUsername = d.Creator!.Username,
                    SlikaBase64 = d.Slika != null ? Convert.ToBase64String(d.Slika) : null,
                    Idu = d.Glasovi.Where(g => g.GlasOption == GlasOptions.Ide).Select(g => g.User!.Username ?? g.User.Email).ToList(),
                    MozdaIdu = d.Glasovi.Where(g => g.GlasOption == GlasOptions.Mozda).Select(g => g.User!.Username ?? g.User.Email).ToList(),
                    NeIdu = d.Glasovi.Where(g => g.GlasOption == GlasOptions.NeIde).Select(g => g.User!.Username ?? g.User.Email).ToList(),
                    MojGlas = d.Glasovi.FirstOrDefault(g => g.User!.Id == uid)?.GlasOption,
                    CanEdit = d.CreatorId == uid
                }).ToList()
            };

            ViewData["TrenutnaGrupaId"] = group.Id;
            ViewData["TrenutnaGrupaNaziv"] = group.Name;

            return View(vm);
        }

        public async Task<IActionResult> DogadjajCreate(long grupaId, string? errorMessage = null)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == grupaId);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid))
                return RedirectToAction("Index", "Groups");

            var vm = new DogadjajCreateViewModel
            {
                GrupaId = grupaId,
                ErrorMessage = errorMessage
            };

            ViewData["TrenutnaGrupaId"] = group.Id;
            ViewData["TrenutnaGrupaNaziv"] = group.Name;

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> DogadjajCreate(DogadjajCreateViewModel vm)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == vm.GrupaId);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid))
                return RedirectToAction("Index", "Groups");

            if (string.IsNullOrWhiteSpace(vm.Naziv))
            {
                vm.ErrorMessage = "Naziv je obavezan.";
                ViewData["TrenutnaGrupaId"] = group.Id;
                ViewData["TrenutnaGrupaNaziv"] = group.Name;
                return View(vm);
            }

            if (!vm.VremeDogadjaja.HasValue)
            {
                vm.ErrorMessage = "Vreme događaja je obavezno.";
                ViewData["TrenutnaGrupaId"] = group.Id;
                ViewData["TrenutnaGrupaNaziv"] = group.Name;
                return View(vm);
            }

            byte[]? slikaBytes = null;
            if (vm.Slika != null && vm.Slika.Length > 0)
            {
                using var ms = new MemoryStream();
                await vm.Slika.CopyToAsync(ms);
                slikaBytes = ms.ToArray();
            }

            if (vm.Lokacija != null)
                vm.Lokacija = vm.Lokacija.Trim();

            var dog = new Dogadjaj
            {
                GrupaId = group.Id,
                CreatorId = uid,
                Naziv = vm.Naziv.Trim(),
                Opis = vm.Opis,
                Lokacija = vm.Lokacija,
                VremeDogadjaja = vm.VremeDogadjaja.Value.ToUniversalTime(),
                Slika = slikaBytes,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Dogadjaji.Add(dog);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dogadjaji", new { id = group.Id });
        }

        public async Task<IActionResult> DogadjajEdit(long grupaId, long dogadjajId, string? errorMessage = null)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(g => g.Id == grupaId);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid))
                return RedirectToAction("Index", "Groups");

            bool isAdmin = group.Members.Any(m => m.IsAdmin && m.UserId == uid);

            var dogadjaj = await _context.Dogadjaji
                .FirstOrDefaultAsync(d => d.Id == dogadjajId && d.GrupaId == grupaId);

            if (dogadjaj == null)
                return RedirectToAction("Dogadjaji", new { id = grupaId });

            if (dogadjaj.CreatorId != uid && !isAdmin)
                return RedirectToAction("Dogadjaji", new { id = grupaId });

            string? base64Slika = null;
            if (dogadjaj.Slika != null)
                base64Slika = Convert.ToBase64String(dogadjaj.Slika);

            var vm = new DogadjajEditViewModel
            {
                GrupaId = grupaId,
                Id = dogadjaj.Id,
                Naziv = dogadjaj.Naziv,
                Opis = dogadjaj.Opis,
                Lokacija = dogadjaj.Lokacija,
                CurrentSlikaBase64 = base64Slika,
                ErrorMessage = errorMessage
            };

            ViewData["TrenutnaGrupaId"] = group.Id;
            ViewData["TrenutnaGrupaNaziv"] = group.Name;

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> DogadjajEdit(DogadjajEditViewModel vm)
        {
            var uid = CurrentUserId();

            var dogadjaj = await _context.Dogadjaji
                .FirstOrDefaultAsync(d => d.Id == vm.Id && d.GrupaId == vm.GrupaId);

            bool isAdmin = await _context.GroupsUsers.AnyAsync(u => u.IsAdmin && u.GroupId == vm.GrupaId && u.UserId == uid);

            if (dogadjaj == null)
                return RedirectToAction("Dogadjaji", new { id = vm.GrupaId });

            if (dogadjaj.CreatorId != uid && !isAdmin)
                return RedirectToAction("Dogadjaji", new { id = vm.GrupaId });

            if (string.IsNullOrWhiteSpace(vm.Naziv))
                return RedirectToAction("DogadjajEdit", new { grupaId = vm.GrupaId, dogadjajId = vm.Id, errorMessage = "Naziv je obavezan." });

            if (vm.Opis != null)
                vm.Opis = vm.Opis.Trim();
            if (vm.Lokacija != null)
                vm.Lokacija = vm.Lokacija.Trim();

            dogadjaj.Naziv = vm.Naziv.Trim();
            dogadjaj.Opis = vm.Opis;
            dogadjaj.Lokacija = vm.Lokacija;

            if (vm.NovaSlika != null && vm.NovaSlika.Length > 0)
            {
                using var ms = new MemoryStream();
                await vm.NovaSlika.CopyToAsync(ms);
                dogadjaj.Slika = ms.ToArray();
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Dogadjaji", new { id = vm.GrupaId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDogadjaj(long grupaId, long dogadjajId)
        {
            var uid = CurrentUserId();

            var dogadjaj = await _context.Dogadjaji
                .FirstOrDefaultAsync(d => d.Id == dogadjajId && d.GrupaId == grupaId);

            bool isAdmin = await _context.GroupsUsers.AnyAsync(u => u.IsAdmin && u.GroupId == grupaId && u.UserId == uid);

            if (dogadjaj == null)
                return RedirectToAction("Dogadjaji", new { id = grupaId });

            if (dogadjaj.CreatorId != uid && !isAdmin)
                return RedirectToAction("Dogadjaji", new { id = grupaId });

            _context.Dogadjaji.Remove(dogadjaj);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dogadjaji", new { id = grupaId });
        }

        [HttpPost]
        public async Task<IActionResult> DogadjajGlasaj(long grupaId, long dogadjajId, GlasOptions glas)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == grupaId);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid))
                return RedirectToAction("Index", "Groups");

            var dogadjaj = await _context.Dogadjaji
                .FirstOrDefaultAsync(d => d.Id == dogadjajId && d.GrupaId == grupaId);

            if (dogadjaj == null)
                return RedirectToAction("Dogadjaji", new { id = grupaId });

            var groupUser = group.Members.First(m => m.UserId == uid);

            var postojeciGlas = await _context.DogadjajGlasovi
                .FirstOrDefaultAsync(g => g.DogadjajId == dogadjajId && g.UserId == groupUser.UserId);

            if (postojeciGlas == null)
            {
                _context.DogadjajGlasovi.Add(new DogadjajGlas
                {
                    DogadjajId = dogadjajId,
                    UserId = groupUser.UserId,
                    GlasOption = glas
                });
            }
            else
            {
                postojeciGlas.GlasOption = glas;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Dogadjaji", new { id = grupaId });
        }


        //Shopping lista
        public async Task<IActionResult> ShoppingLista(long id, string? errorMessage = null)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid))
                return RedirectToAction("Index", "Groups");

            bool isAdmin = group.Members.Any(m => m.IsAdmin && m.UserId == uid);

            var shoppingListaItems = await _context.ShoppingListaItems
                .Where(s => s.GroupId == id)
                .Select(i => new ShoppingListaItemDto
                {
                    Id = i.Id,
                    NabavioUserId = i.NabavioUserId,
                    TrazioUserId = i.TrazioUserId,
                    NabavljenoUtc = i.NabavljenoUtc,
                    TrazenoUtc = i.TrazenoUtc,
                    Naziv = i.Naziv,
                    CanDelete = isAdmin || uid == i.TrazioUserId
                })
                .ToListAsync();

            ShoppingListaViewModel vm = new ShoppingListaViewModel
            {
                GrupaId = id,
                Items = shoppingListaItems.Where(sli => sli.NabavioUserId == null).ToList(),
                Nabavljeni = shoppingListaItems.Where(sli => sli.NabavioUserId != null).ToList(),
                ErrorMessage = errorMessage
            };

            var usernames = group.Members
                .Select(m => new { Id = m.UserId, Username = m.User.Username ?? m.User.Email, Nickname = m.Nickname })
                .ToDictionary(m => m.Id);

            foreach (var item in vm.Items)
            {
                if (usernames.ContainsKey(item.TrazioUserId))
                {
                    item.TrazioUserUsername = usernames[item.TrazioUserId].Username;
                    item.TrazioUserNickname = usernames[item.TrazioUserId].Nickname;
                }
            }
            foreach (var item in vm.Nabavljeni)
            {
                if (usernames.ContainsKey(item.TrazioUserId))
                {
                    item.TrazioUserUsername = usernames[item.TrazioUserId].Username;
                    item.TrazioUserNickname = usernames[item.TrazioUserId].Nickname;
                }

                if (usernames.ContainsKey(item.NabavioUserId!.Value))
                {
                    item.NabavioUserUsername = usernames[item.NabavioUserId.Value].Username;
                    item.NabavioUserNickname = usernames[item.NabavioUserId.Value].Nickname;
                }
            }

            ViewData["TrenutnaGrupaId"] = group.Id;
            ViewData["TrenutnaGrupaNaziv"] = group.Name;

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> DodajItem(long grupaId, string naziv)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(g => g.Id == grupaId);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid))
                return RedirectToAction("Index", "Groups");

            if (!string.IsNullOrWhiteSpace(naziv) && naziv.Length > 30)
                return RedirectToAction("ShoppingLista", new { id = grupaId, errorMessage = "Ime stavke ne sme biti duže od 30 karaktera!" });

            ShoppingListaItem shoppingListaItem = new ShoppingListaItem
            {
                GroupId = grupaId,
                TrazioUserId = uid,
                Naziv = naziv,
                TrazenoUtc = DateTime.UtcNow
            };
            await _context.ShoppingListaItems.AddAsync(shoppingListaItem);
            await _context.SaveChangesAsync();

            return RedirectToAction("ShoppingLista", new { id = grupaId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteShoppingItem(long grupaId, long shoppingItemId)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(g => g.Id == grupaId);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid))
                return RedirectToAction("Index", "Groups");

            var existingItem = await _context.ShoppingListaItems.FindAsync(shoppingItemId);
            if (existingItem == null)
                return RedirectToAction("ShoppingLista", new { id = grupaId });

            _context.ShoppingListaItems.Remove(existingItem);
            await _context.SaveChangesAsync();

            return RedirectToAction("ShoppingLista", new { id = grupaId });
        }

        [HttpPost]
        public async Task<IActionResult> NabaviShoppingItem(long grupaId, long shoppingItemId)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(g => g.Id == grupaId);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid))
                return RedirectToAction("Index", "Groups");

            var existingItem = await _context.ShoppingListaItems.FindAsync(shoppingItemId);
            if (existingItem == null)
                return RedirectToAction("ShoppingLista", new { id = grupaId });

            existingItem.NabavioUserId = uid;
            existingItem.NabavljenoUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction("ShoppingLista", new { id = grupaId });
        }

        // Chat
        public async Task<IActionResult> Chat(long id)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid))
                return RedirectToAction("Index", "Groups");

            Dictionary<long, string?> nicknames = group.Members.ToDictionary(m => m.UserId, m => m.Nickname);

            var chat = await _context.Chats
                .Where(g => g.GrupaId == id)
                .Select(c => new ChatMessagesDto
                {
                    SentAtUtc = c.SentAtUtc,
                    Id = c.Id,
                    Poruka = c.Poruka,
                    SentById = c.SentById
                })
                .OrderBy(m => m.SentAtUtc)
                .ToListAsync();

            List<long> userIds = chat.Select(c => c.SentById).Distinct().ToList();
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id) || u.MemberGroups.Any(mg => mg.GroupId == group.Id))
                .Select(u => new
                {
                    Id = u.Id,
                    Username = u.Username,
                    Nickname = nicknames.ContainsKey(u.Id) ? nicknames[u.Id] : null,
                    Slika = u.Slika,
                    SlikaExtension = u.SlikaExtension
                })
                .ToListAsync();
            List<ChatUserInfoDto> userInfos = new List<ChatUserInfoDto>();

            foreach (var user in users)
            {
                if (user != null)
                {
                    string? base64 = null;
                    if (user.Slika != null)
                        base64 = Convert.ToBase64String(user.Slika);
                    userInfos.Add(new ChatUserInfoDto
                    {
                        Base64Image = base64,
                        Username = user.Username,
                        Nickname = user.Nickname,
                        UserId = user.Id,
                    });
                }
                else
                {
                    userInfos.Add(new ChatUserInfoDto
                    {
                        Base64Image = null,
                        Username = "Obrisan nalog",
                        UserId = -1,
                    });
                }
            }

            ChatViewModel vm = new ChatViewModel
            {
                GrupaId = id,
                UserInfo = userInfos,
                Poruke = chat
            };

            ViewData["TrenutnaGrupaId"] = group.Id;
            ViewData["TrenutnaGrupaNaziv"] = group.Name;

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> PosaljiPoruku(long grupaId, string poruka)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(g => g.Id == grupaId);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid))
                return RedirectToAction("Index", "Groups");

            await _context.Chats.AddAsync(new Chat
            {
                SentAtUtc = DateTime.UtcNow,
                GrupaId = grupaId,
                Poruka = poruka,
                SentById = uid
            });
            await _context.SaveChangesAsync();
            return RedirectToAction("Chat", new { id = grupaId });
        }

        // Settings
        public async Task<IActionResult> Settings(long grupaId, string? errorMessage = null)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(g => g.Id == grupaId);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid))
                return RedirectToAction("Index", "Groups");

            bool isAdmin = group.Members.Any(m => m.UserId == uid && m.IsAdmin);

            var vm = new SettingsViewModel
            {
                GrupaId = grupaId,
                IsAdmin = isAdmin,
                NazivGrupe = group.Name,
                Base64Slika = (group.Slika == null || group.Slika.Length == 0) ? null : Convert.ToBase64String(group.Slika),
                ErrorMessage = errorMessage,
                Users = group.Members
                    .OrderBy(m => m.User.Username ?? m.User.Email)
                    .Select(m => new UserGroupSettingsDto
                    {
                        Id = m.UserId,
                        Username = m.User.Username ?? m.User.Email,
                        Nickname = m.Nickname,
                        IsAdmin = m.IsAdmin,
                        Base64Image = m.User.Slika != null ? Convert.ToBase64String(m.User.Slika) : null
                    })
                    .ToList()
            };

            ViewData["TrenutnaGrupaId"] = group.Id;
            ViewData["TrenutnaGrupaNaziv"] = group.Name;

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateNazivGrupe(long grupaId, string nazivGrupe)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == grupaId);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid && m.IsAdmin))
                return RedirectToAction("Settings", new { grupaId, errorMessage = "Samo admin može menjati naziv grupe." });

            if (string.IsNullOrWhiteSpace(nazivGrupe))
                return RedirectToAction("Settings", new { grupaId, errorMessage = "Naziv je obavezan." });

            if (nazivGrupe.Trim().Length > 30)
                return RedirectToAction("Settings", new { grupaId, errorMessage = "Naziv grupe ne sme da bude duži od 30 karaktera." });

            group.Name = nazivGrupe.Trim();
            await _context.SaveChangesAsync();

            return RedirectToAction("Settings", new { grupaId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSlikuGrupe(long grupaId, IFormFile? slika)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == grupaId);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid))
                return RedirectToAction("Index", "Groups");

            if (slika == null || slika.Length == 0)
            {
                group.Slika = null;
                await _context.SaveChangesAsync();
                return RedirectToAction("Settings", new { grupaId });
            }

            using var ms = new MemoryStream();
            await slika.CopyToAsync(ms);
            group.Slika = ms.ToArray();

            await _context.SaveChangesAsync();
            return RedirectToAction("Settings", new { grupaId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateNickname(long grupaId, long userId, string? nickname)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == grupaId);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid))
                return RedirectToAction("Index", "Groups");

            if (nickname != null && nickname.Trim().Length > 30)
            {
                return RedirectToAction("Settings", new { grupaId, errorMessage = "Nadimak ne sme biti duži od 30 karaktera." });
            }

            var gu = group.Members.FirstOrDefault(m => m.UserId == userId);
            if (gu == null)
                return RedirectToAction("Settings", new { grupaId });

            gu.Nickname = null;
            if (!string.IsNullOrEmpty(nickname))
                gu.Nickname = nickname.Trim();
            await _context.SaveChangesAsync();

            return RedirectToAction("Settings", new { grupaId });
        }

        [HttpPost]
        public async Task<IActionResult> IzbaciClana(long grupaId, long userId)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == grupaId);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid && m.IsAdmin) || uid == userId)
                return RedirectToAction("Settings", new { grupaId, errorMessage = "Samo admin može izbaciti člana." });

            var gu = await _context.GroupsUsers
                .FirstOrDefaultAsync(x => x.GroupId == grupaId && x.UserId == userId);
            if (gu != null)
            {
                _context.GroupsUsers.Remove(gu);
                var shoppingListaToRemove = await _context.ShoppingListaItems.Where(s => s.TrazioUserId == gu.UserId && s.GroupId == grupaId).ToListAsync();
                _context.RemoveRange(shoppingListaToRemove);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Settings", new { grupaId });
        }

        [HttpPost]
        public async Task<IActionResult> SetAdmin(long grupaId, long userId)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == grupaId);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            if (!group.Members.Any(m => m.UserId == uid && m.IsAdmin))
                return RedirectToAction("Settings", new { grupaId, errorMessage = "Samo admin može dodeliti/ukloniti admin privilegije." });

            var noviAdmin = group.Members.FirstOrDefault(m => m.UserId == userId);
            if (noviAdmin == null)
                return RedirectToAction("Settings", new { grupaId });

            noviAdmin.IsAdmin = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Settings", new { grupaId });
        }

        [HttpPost]
        public async Task<IActionResult> NapustiGrupu(long grupaId)
        {
            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == grupaId);

            if (group == null)
                return RedirectToAction("Index", "Groups");

            var groupUser = await _context.GroupsUsers.FirstOrDefaultAsync(gu => gu.GroupId == grupaId && gu.UserId == uid);
            if (groupUser == null)
                return RedirectToAction("Index", "Groups");

            if (group.Members.Count == 1)
            {
                _context.Groups.Remove(group);
            }
            else if (groupUser.IsAdmin && group.Members.Where(m => m.IsAdmin).Count() == 1)
            {
                var najranijiUser = await _context.GroupsUsers
                    .OrderBy(gu => gu.JoinedAtUtc)
                    .FirstOrDefaultAsync(m => m.IsAdmin == false && m.GroupId == grupaId);
                if (najranijiUser == null)
                    return RedirectToAction("Settings", new { grupaId, errorMessage = "Došlo je do greške u radu servera." });
                najranijiUser.IsAdmin = true;
                _context.GroupsUsers.Remove(groupUser);
            }
            else
            {
                _context.GroupsUsers.Remove(groupUser);
            }

            var shoppingListaToRemove = await _context.ShoppingListaItems.Where(s => s.TrazioUserId == uid && s.GroupId == grupaId).ToListAsync();
            _context.RemoveRange(shoppingListaToRemove);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Groups");
        }
    }
}
