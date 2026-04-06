using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitSync.Data;
using SplitSync.Entities;
using SplitSync.Models;

namespace SplitSync.Controllers
{
    [ApiController]
    [Route("api")]
    public class ApiController : Controller
    {
        private AppDbContext _context { get; set; }

        public ApiController(AppDbContext context)
        {
            _context = context;
        }

        private long CurrentUserId()
        {
            return long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        }

        [Authorize]
        [HttpGet("profile-image")]
        public async Task<IActionResult> GetProfilePicture()
        {
            var uid = CurrentUserId();
            var slikaIzBaze = await _context.Users
                .Select(u => new { u.Id, u.Slika, u.SlikaExtension })
                .FirstOrDefaultAsync(u => u.Id == uid);

            if (slikaIzBaze?.Slika == null || slikaIzBaze.Slika.Length == 0)
                return NoContent();

            string contentType = "image/jpeg";
            if (slikaIzBaze.SlikaExtension == "png" || slikaIzBaze.SlikaExtension == ".png")
                contentType = "image/png";

            return File(slikaIzBaze.Slika, contentType);
        }

        [Authorize]
        [HttpGet("nove-poruke")]
        public async Task<ActionResult<List<ChatMessagesDto>>> GetNewPoruke(
        [FromQuery] long groupId,
        [FromQuery] DateTime? odUtc)
        {
            if (groupId <= 0)
                return new List<ChatMessagesDto>();

            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                return new List<ChatMessagesDto>();

            if (!group.Members.Any(m => m.UserId == uid))
                return new List<ChatMessagesDto>();

            var msgs = new List<ChatMessagesDto>();

            if (odUtc != null)
            {
                msgs = await _context.Chats
                    .AsNoTracking()
                    .Where(c => c.GrupaId == groupId && c.SentAtUtc > odUtc)
                    .OrderBy(c => c.SentAtUtc)
                    .Select(c => new ChatMessagesDto
                    {
                        Id = c.Id,
                        SentById = c.SentById,
                        Poruka = c.Poruka,
                        SentAtUtc = c.SentAtUtc
                    })
                    .ToListAsync();
            }
            else
            {
                msgs = await _context.Chats
                    .AsNoTracking()
                    .Where(c => c.GrupaId == groupId)
                    .OrderBy(c => c.SentAtUtc)
                    .Select(c => new ChatMessagesDto
                    {
                        Id = c.Id,
                        SentById = c.SentById,
                        Poruka = c.Poruka,
                        SentAtUtc = c.SentAtUtc
                    })
                    .ToListAsync();
            }

            return Ok(msgs);
        }

        [Authorize]
        [HttpGet("novi-dogadjaji")]
        public async Task<ActionResult<List<DogadjajiDto>>> GetNoviDogadjaji(
        [FromQuery] long groupId,
        [FromQuery] DateTime? odUtc)
        {
            if (groupId <= 0)
                return new List<DogadjajiDto>();

            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                return new List<DogadjajiDto>();

            if (!group.Members.Any(m => m.UserId == uid))
                return new List<DogadjajiDto>();

            Dictionary<long, string?> nicknames = group.Members.ToDictionary(m => m.UserId, m => m.Nickname);

            var dogadjaji = new List<DogadjajiDto>();

            if (odUtc != null)
            {
                var dogadjajiFromDb = await _context.Dogadjaji
                    .AsNoTracking()
                    .Where(d => d.GrupaId == groupId && d.VremeDogadjaja > odUtc)
                    .Include(d => d.Creator)
                    .Include(d => d.Glasovi)
                        .ThenInclude(gl => gl.User)
                    .OrderByDescending(d => d.VremeDogadjaja)
                    .ToListAsync();

                dogadjaji = dogadjajiFromDb.Select(d => new DogadjajiDto
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
                }).ToList();
            }
            else
            {
                var dogadjajiFromDb = await _context.Dogadjaji
                    .AsNoTracking()
                    .Where(d => d.GrupaId == groupId)
                    .Include(d => d.Creator)
                    .Include(d => d.Glasovi)
                        .ThenInclude(gl => gl.User)
                    .OrderByDescending(d => d.VremeDogadjaja)
                    .ToListAsync();

                dogadjaji = dogadjajiFromDb.Select(d => new DogadjajiDto
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
                }).ToList();
            }

            return Ok(dogadjaji);
        }

        [Authorize]
        [HttpGet("novi-racuni")]
        public async Task<ActionResult<List<RacuniApiDto>>> GetNoviRacuni(
        [FromQuery] long groupId,
        [FromQuery] DateTime? odUtc)
        {
            if (groupId <= 0)
                return new List<RacuniApiDto>();

            var uid = CurrentUserId();

            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                return new List<RacuniApiDto>();

            if (!group.Members.Any(m => m.UserId == uid))
                return new List<RacuniApiDto>();

            Dictionary<long, string?> nicknames = group.Members.ToDictionary(m => m.UserId, m => m.Nickname);

            var racuni = new List<RacuniApiDto>();

            if (odUtc != null)
            {
                var racuniFromDb = await _context.Racuns
                    .AsNoTracking()
                    .Where(r => r.GroupId == groupId && r.CreatedAtUtc > odUtc)
                    .Include(r => r.Items)
                        .ThenInclude(ri => ri.User)
                    .OrderByDescending(r => r.CreatedAtUtc)
                    .ToListAsync();

                racuni = racuniFromDb.Select(r => new RacuniApiDto
                {
                    Id = r.Id,
                    Naziv = r.Naziv ?? "",
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
                }).ToList();
            }
            else
            {
                var racuniFromDb = await _context.Racuns
                    .AsNoTracking()
                    .Where(r => r.GroupId == groupId)
                    .Include(r => r.Items)
                        .ThenInclude(ri => ri.User)
                    .OrderByDescending(r => r.CreatedAtUtc)
                    .ToListAsync();

                racuni = racuniFromDb.Select(r => new RacuniApiDto
                {
                    Id = r.Id,
                    Naziv = r.Naziv ?? "",
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
                }).ToList();
            }

            return Ok(racuni);
        }

        [Authorize]
        [HttpGet("nove-pozivnice")]
        public async Task<ActionResult<List<InvitationsDto>>> GetNovePozivnice(
        [FromQuery] DateTime? odUtc)
        {
            var uid = CurrentUserId();

            var invitations = new List<InvitationsDto>();

            if (odUtc != null)
            {
                var invitationsFromDb = await _context.GroupInvitations
                    .AsNoTracking()
                    .Include(i => i.Group)
                    .Include(i => i.InvitedByUser)
                    .Where(i => i.InvitedUserId == uid && i.CreatedAtUtc > odUtc)
                    .OrderByDescending(i => i.CreatedAtUtc)
                    .ToListAsync();

                invitations = invitationsFromDb.Select(i => new InvitationsDto
                {
                    InvitationId = i.Id,
                    GroupId = i.GroupId,
                    GroupName = i.Group.Name,
                    InvitedBy = i.InvitedByUser.Username ?? i.InvitedByUser.Email,
                    CreatedAtUtc = i.CreatedAtUtc
                }).ToList();
            }
            else
            {
                var invitationsFromDb = await _context.GroupInvitations
                    .AsNoTracking()
                    .Include(i => i.Group)
                    .Include(i => i.InvitedByUser)
                    .Where(i => i.InvitedUserId == uid)
                    .OrderByDescending(i => i.CreatedAtUtc)
                    .ToListAsync();

                invitations = invitationsFromDb.Select(i => new InvitationsDto
                {
                    InvitationId = i.Id,
                    GroupId = i.GroupId,
                    GroupName = i.Group.Name,
                    InvitedBy = i.InvitedByUser.Username ?? i.InvitedByUser.Email,
                    CreatedAtUtc = i.CreatedAtUtc
                }).ToList();
            }

            return Ok(invitations);
        }
    }
}
