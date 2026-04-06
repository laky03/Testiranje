using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitSync.Data;
using SplitSync.Entities;

namespace SplitSync.Controllers
{
    [ApiController]
    [Route("api/dogadjaji")]
    public class DogadjajiApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DogadjajiApiController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Dogadjaj>>> GetAll()
        {
            var dogadjaji = await _context.Dogadjaji.ToListAsync();
            return Ok(dogadjaji);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Dogadjaj>> GetById(long id)
        {
            var dogadjaj = await _context.Dogadjaji.FindAsync(id);

            if (dogadjaj == null)
                return NotFound();

            return Ok(dogadjaj);
        }
        [HttpPost]
        public async Task<ActionResult<Dogadjaj>> Create(CreateDogadjajRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Naziv))
                return BadRequest("Naziv događaja je obavezan.");

            var groupExists = await _context.Groups.AnyAsync(g => g.Id == request.GrupaId);
            if (!groupExists)
                return BadRequest("Grupa ne postoji.");

            var creatorExists = await _context.Users.AnyAsync(u => u.Id == request.CreatorId);
            if (!creatorExists)
                return BadRequest("Creator user ne postoji.");

            var dogadjaj = new Dogadjaj
            {
                GrupaId = request.GrupaId,
                CreatorId = request.CreatorId,
                Naziv = request.Naziv.Trim(),
                Opis = request.Opis,
                Lokacija = request.Lokacija,
                VremeDogadjaja = request.VremeDogadjaja,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Dogadjaji.Add(dogadjaj);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = dogadjaj.Id }, dogadjaj);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, UpdateDogadjajRequest request)
        {
            var dogadjaj = await _context.Dogadjaji.FindAsync(id);

            if (dogadjaj == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(request.Naziv))
                return BadRequest("Naziv događaja je obavezan.");

            var groupExists = await _context.Groups.AnyAsync(g => g.Id == request.GrupaId);
            if (!groupExists)
                return BadRequest("Grupa ne postoji.");

            var creatorExists = await _context.Users.AnyAsync(u => u.Id == request.CreatorId);
            if (!creatorExists)
                return BadRequest("Creator user ne postoji.");

            dogadjaj.GrupaId = request.GrupaId;
            dogadjaj.CreatorId = request.CreatorId;
            dogadjaj.Naziv = request.Naziv.Trim();
            dogadjaj.Opis = request.Opis;
            dogadjaj.Lokacija = request.Lokacija;
            dogadjaj.VremeDogadjaja = request.VremeDogadjaja;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var dogadjaj = await _context.Dogadjaji.FindAsync(id);

            if (dogadjaj == null)
                return NotFound();

            _context.Dogadjaji.Remove(dogadjaj);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
    public class CreateDogadjajRequest
    {
        public long GrupaId { get; set; }
        public long CreatorId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string? Opis { get; set; }
        public string? Lokacija { get; set; }
        public DateTime VremeDogadjaja { get; set; }
    }

    public class UpdateDogadjajRequest
    {
        public long GrupaId { get; set; }
        public long CreatorId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string? Opis { get; set; }
        public string? Lokacija { get; set; }
        public DateTime VremeDogadjaja { get; set; }
    }
}