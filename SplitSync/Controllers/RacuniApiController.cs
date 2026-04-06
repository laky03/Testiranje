using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitSync.Data;
using SplitSync.Entities;

namespace SplitSync.Controllers
{
    [ApiController]
    [Route("api/racuni")]
    public class RacuniApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RacuniApiController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Racun>>> GetAll()
        {
            var racuni = await _context.Racuns.ToListAsync();
            return Ok(racuni);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Racun>> GetById(long id)
        {
            var racun = await _context.Racuns.FindAsync(id);

            if (racun == null)
                return NotFound();

            return Ok(racun);
        }
        [HttpPost]
        public async Task<ActionResult<Racun>> Create(CreateRacunRequest request)
        {
            var groupExists = await _context.Groups.AnyAsync(g => g.Id == request.GroupId);
            if (!groupExists)
                return BadRequest("Grupa ne postoji.");

            if (request.CreatorUserId.HasValue)
            {
                var creatorExists = await _context.Users.AnyAsync(u => u.Id == request.CreatorUserId.Value);
                if (!creatorExists)
                    return BadRequest("Creator user ne postoji.");
            }

            var racun = new Racun
            {
                GroupId = request.GroupId,
                Naziv = request.Naziv,
                Iznos = request.Iznos,
                CreatorUserId = request.CreatorUserId,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Racuns.Add(racun);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = racun.Id }, racun);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, UpdateRacunRequest request)
        {
            var racun = await _context.Racuns.FindAsync(id);

            if (racun == null)
                return NotFound();

            var groupExists = await _context.Groups.AnyAsync(g => g.Id == request.GroupId);
            if (!groupExists)
                return BadRequest("Grupa ne postoji.");

            if (request.CreatorUserId.HasValue)
            {
                var creatorExists = await _context.Users.AnyAsync(u => u.Id == request.CreatorUserId.Value);
                if (!creatorExists)
                    return BadRequest("Creator user ne postoji.");
            }

            racun.GroupId = request.GroupId;
            racun.Naziv = request.Naziv;
            racun.Iznos = request.Iznos;
            racun.CreatorUserId = request.CreatorUserId;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var racun = await _context.Racuns.FindAsync(id);

            if (racun == null)
                return NotFound();

            _context.Racuns.Remove(racun);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
    public class CreateRacunRequest
    {
        public long GroupId { get; set; }
        public string? Naziv { get; set; }
        public double Iznos { get; set; }
        public long? CreatorUserId { get; set; }
    }

    public class UpdateRacunRequest
    {
        public long GroupId { get; set; }
        public string? Naziv { get; set; }
        public double Iznos { get; set; }
        public long? CreatorUserId { get; set; }
    }
}
