using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitSync.Data;
using SplitSync.Entities;

namespace SplitSync.Controllers
{
    [ApiController]
    [Route("api/shopping-items")]
    public class ShoppingItemsApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ShoppingItemsApiController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShoppingListaItem>>> GetAll()
        {
            var items = await _context.ShoppingListaItems.ToListAsync();
            return Ok(items);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ShoppingListaItem>> GetById(long id)
        {
            var item = await _context.ShoppingListaItems.FindAsync(id);

            if (item == null)
                return NotFound();

            return Ok(item);
        }
        [HttpPost]
        public async Task<ActionResult<ShoppingListaItem>> Create(CreateShoppingItemRequest request)
        {
            var item = new ShoppingListaItem
            {
                GroupId = request.GroupId,
                TrazioUserId = request.TrazioUserId,
                Naziv = request.Naziv,
                TrazenoUtc = DateTime.UtcNow
            };

            _context.ShoppingListaItems.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, UpdateShoppingItemRequest request)
        {
            var item = await _context.ShoppingListaItems.FindAsync(id);

            if (item == null)
                return NotFound();

            item.Naziv = request.Naziv;
            item.NabavioUserId = request.NabavioUserId;

            if (request.NabavioUserId.HasValue)
                item.NabavljenoUtc = DateTime.UtcNow;
            else
                item.NabavljenoUtc = null;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var item = await _context.ShoppingListaItems.FindAsync(id);

            if (item == null)
                return NotFound();

            _context.ShoppingListaItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
    public class CreateShoppingItemRequest
    {
        public long GroupId { get; set; }
        public long TrazioUserId { get; set; }
        public string Naziv { get; set; } = string.Empty;
    }
    public class UpdateShoppingItemRequest
    {
        public string Naziv { get; set; } = string.Empty;
        public long? NabavioUserId { get; set; }
    }
}