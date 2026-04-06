using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitSync.Data;
using SplitSync.Entities;
using SplitSync.Models;

namespace SplitSync.Controllers
{
    [Authorize]
    public class GroupsController : Controller
    {
        private readonly AppDbContext _context;

        public GroupsController(AppDbContext context)
        {
            _context = context;
        }

        private long CurrentUserId()
        {
            return long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        }

        public async Task<IActionResult> Index()
        {
            var userId = CurrentUserId();

            var groups = await _context.Groups
                .Where(g => _context.GroupsUsers.Any(gu => gu.GroupId == g.Id && gu.UserId == userId))
                .Select(g => new GroupItem
                {
                    Id = g.Id,
                    Name = g.Name,
                    ImageBase64 = (g.Slika != null && g.Slika.Length > 0) ? Convert.ToBase64String(g.Slika) : null,
                    IsOwner = g.OwnerUserId == userId
                })
                .OrderBy(x => x.Name)
                .ToListAsync();

            var invitations = await _context.GroupInvitations
                .Include(i => i.Group)
                .Include(i => i.InvitedByUser)
                .Where(i => i.InvitedUserId == userId)
                .OrderByDescending(i => i.Id)
                .Select(i => new InviteItem
                {
                    InvitationId = i.Id,
                    GroupId = i.GroupId,
                    GroupName = i.Group.Name,
                    InvitedBy = i.InvitedByUser.Username ?? i.InvitedByUser.Email,
                    CreatedAtUtc = i.CreatedAtUtc
                })
                .ToListAsync();

            return View(new GroupsIndexViewModel { Groups = groups, Invitations = invitations });
        }

        public IActionResult Create()
        {
            return View(new GroupCreateViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupCreateViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Name))
            {
                vm.ErrorMessage = "Naziv je obavezan.";
                return View(vm);
            }

            if (vm.Name.Trim().Length > 30)
            {
                vm.ErrorMessage = "Naziv grupe ne sme da bude duži od 30 karaktera.";
                return View(vm);
            }

            var userId = CurrentUserId();

            var group = new Group
            {
                Name = vm.Name.Trim(),
                OwnerUserId = userId,
                CreatedAtUtc = DateTime.UtcNow
            };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            _context.GroupsUsers.Add(new GroupsUsers
            {
                GroupId = group.Id,
                UserId = userId,
                IsAdmin = true,
                JoinedAtUtc = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            if (vm.ImageUpload != null && vm.ImageUpload.Length > 0)
            {
                using var ms = new MemoryStream();
                await vm.ImageUpload.CopyToAsync(ms);
                group.Slika = ms.ToArray();
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Group", new { id = group.Id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptInvite(long invitationId)
        {
            var userId = CurrentUserId();

            var inv = await _context.GroupInvitations.FirstOrDefaultAsync(i => i.Id == invitationId);
            if (inv == null || inv.InvitedUserId != userId)
                return RedirectToAction(nameof(Index));

            var exists = await _context.GroupsUsers.AnyAsync(gu => gu.GroupId == inv.GroupId && gu.UserId == userId);
            if (!exists)
            {
                _context.GroupsUsers.Add(new GroupsUsers
                {
                    GroupId = inv.GroupId,
                    UserId = userId,
                    IsAdmin = false,
                    JoinedAtUtc = DateTime.UtcNow
                });
            }

            _context.GroupInvitations.Remove(inv);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Group", new { id = inv.GroupId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineInvite(long invitationId)
        {
            var uid = CurrentUserId();

            var inv = await _context.GroupInvitations.FirstOrDefaultAsync(i => i.Id == invitationId);
            if (inv == null || inv.InvitedUserId != uid)
                return RedirectToAction(nameof(Index));

            _context.GroupInvitations.Remove(inv);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
