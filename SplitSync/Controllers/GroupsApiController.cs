using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using SplitSync.Data;
using SplitSync.Entities;

namespace SplitSync.Controllers
{
    [ApiController]
    [Route("api/groups")]
    public class GroupsApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GroupsApiController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Group>>> GetAll()
        {
            var groups = await _context.Groups.ToListAsync();
            return Ok(groups);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Group>> GetById(long id)
        {
            var group = await _context.Groups.FindAsync(id);

            if (group == null)
                return NotFound();

            return Ok(group);
        }
        [HttpPost]
        public async Task<ActionResult<Group>> Create(CreateGroupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Naziv grupe je obavezan.");

            var ownerExists = await _context.Users.AnyAsync(u => u.Id == request.OwnerUserId);
            if (!ownerExists)
                return BadRequest("Owner user ne postoji.");

            var group = new Group
            {
                Name = request.Name.Trim(),
                OwnerUserId = request.OwnerUserId,
                DefaultValuta = request.DefaultValuta,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            _context.GroupsUsers.Add(new GroupsUsers
            {
                GroupId = group.Id,
                UserId = request.OwnerUserId,
                IsAdmin = true,
                JoinedAtUtc = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = group.Id }, new
            {
                group.Id,
                group.Name,
                group.OwnerUserId,
                group.DefaultValuta,
                group.CreatedAtUtc
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, UpdateGroupRequest request)
        {
            var group = await _context.Groups.FindAsync(id);

            if (group == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Naziv grupe je obavezan.");

            var ownerExists = await _context.Users.AnyAsync(u => u.Id == request.OwnerUserId);
            if (!ownerExists)
                return BadRequest("Owner user ne postoji.");

            group.Name = request.Name.Trim();
            group.OwnerUserId = request.OwnerUserId;
            group.DefaultValuta = request.DefaultValuta;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var group = await _context.Groups.FindAsync(id);

            if (group == null)
                return NotFound();

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
    public class UpdateGroupRequest
    {
        public string Name { get; set; } = string.Empty;
        public long OwnerUserId { get; set; }
        public string DefaultValuta { get; set; } = "RSD";
    }
    public class CreateGroupRequest
    {
        public string Name { get; set; } = string.Empty;
        public long OwnerUserId { get; set; }
        public string DefaultValuta { get; set; } = "RSD";
    }
}
