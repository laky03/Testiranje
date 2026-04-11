namespace Complete.Controllers;

[ApiController]
[Route("[controller]")]
public class RestoranController : ControllerBase
{
    public RestoraniContext Context { get; set; }

    public RestoranController(RestoraniContext c)
    {
        Context = c;
    }

    [HttpPost("DodajRestoran/{idGrada}/{tipHrane}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DodajRestoran([FromBody]Restoran restoran, int idGrada, string tipHrane)
    {
        try
        {
            if (restoran.X <= 0 && restoran.Y <= 0)
            {
                return BadRequest("Nemoguće dodati restoran bez lokacije.");
            }

            if (string.IsNullOrWhiteSpace(restoran.Naziv))
            {
                return BadRequest("Restoran mora da ima naziv.");
            }

            var grad = await Context.Gradovi.FindAsync(idGrada);

            if (grad != null)
            {
                restoran.Grad = grad;

                restoran.TipHrane = new() { Tip = tipHrane };

                await Context.Restorani.AddAsync(restoran);
                await Context.SaveChangesAsync();
                return Ok($"Dodat je restoran sa ID: {restoran.ID} i nazivom: {restoran.Naziv}.");
            }
            else
            {
                return BadRequest("Grad ne postoji.");
            }
        }
        catch (Exception e)
        {
            return BadRequest(e.ToExceptionString());
        }
    }

    [HttpPut("PromeniTipHraneRestoranu/{idRestorana}/{tipHrane}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> PromeniTipHraneRestoranu(int idRestorana, string tipHrane)
    {
        try
        {
            var restoran = await Context
                .Restorani
                .Include(p => p.TipHrane)
                .Where(p => p.ID == idRestorana)
                .FirstOrDefaultAsync();

            if (restoran == null)
            {
                return BadRequest("Pogrešan id restorana.");
            }

            if (string.IsNullOrWhiteSpace(tipHrane))
            {
                return BadRequest("Tip hrane mora da ima vrednost.");
            }

            // Ovim se obezbeđujemo da nećemo da upišemo novi tip za isti restoran, kada
            // tip već postoji. Kada bi smo upisali novi, stari tip bi ostao kao vrsta
            // koja nije povezana ni sa jednim restoranom
            if (restoran.TipHrane != null && !string.IsNullOrWhiteSpace(restoran.TipHrane.Tip))
            {
                restoran.TipHrane.Tip = tipHrane;
            }
            else
            {
                restoran.TipHrane = new()
                {
                    Tip = tipHrane,
                    Restoran = restoran
                };
            }

            Context.Restorani.Update(restoran);
            await Context.SaveChangesAsync();

            return Ok("Uspešno dodat tip hrane restoranu.");
        }
        catch (Exception e)
        {
            return BadRequest(e.ToExceptionString());
        }
    }

    [HttpGet("PreuzmiRestoraneGrada/{idGrada}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PreuzmiRestoraneGrada(int idGrada)
    {
        try
        {
            var restorani = await Context
                .Gradovi
                .Where(p => p.ID == idGrada)
                .Include(p => p.Restorani)
                .Select(p => new 
                {
                    NazivGrada = p.Naziv,
                    // Ovde može za svaki restoran da se filtrira anonimnim tipom samo ono što 
                    // nam je potrebno, ali i ne mora
                    Restorani = p.Restorani
                })
                .ToListAsync();
            return Ok(restorani);
        }
        catch (Exception e)
        {
            return BadRequest(e.ToExceptionString());
        }
    }

    [HttpGet("PreuzmiRestoraneUBlizini/{x}/{y}/{udaljenost}/{tipHrane?}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> PreuzmiRestoraneUBlizini(double x, double y, double udaljenost, string? tipHrane)
    {
        try
        {
            // Računamo u stepenima, zato množimo sa 111120 da bi dobili metre
            var restorani = await Context.Restorani
                        .Include(p => p.TipHrane)
                        .Where(p =>
                            Math.Sqrt(
                                Math.Pow(p.X - x, 2) +
                                Math.Pow(p.Y - y, 2)) * 111120 < udaljenost)
                        .Where(p =>
                                string.IsNullOrWhiteSpace(tipHrane) ||
                                (p.TipHrane != null && p.TipHrane.Tip == tipHrane))
                        .Select(p => new
                        {
                            p.ID,
                            p.Naziv,
                            p.X,
                            p.Y,
                            p.Prihodi,
                            p.Rashodi,
                            p.Zarada,
                            p.ZbirOcena,
                            p.BrojOcena,
                            p.ProsecnaOcena,
                            TipHrane = p.TipHrane!.Tip
                        })
                        .ToListAsync();

            return Ok(restorani);
        }
        catch (Exception e)
        {
            return BadRequest(e.ToExceptionString());
        }
    }

    [HttpGet("NajbliziRestoran/{x}/{y}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> NajbliziRestoran(double x, double y)
    {
        try
        {
            // Ovde nema potrebe za metrima...
            var restoran = await Context.Restorani
                    .OrderBy(p =>
                        Math.Sqrt(
                            Math.Pow(p.X - x, 2) +
                            Math.Pow(p.Y - y, 2)))
                    .FirstOrDefaultAsync();

            return Ok(restoran);
        }
        catch (Exception e)
        {
            return BadRequest(e.ToExceptionString());
        }
    }

    [HttpGet("NajblizihNRestorana/{x}/{y}/{n}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> NajblizihNRestorana(double x, double y, int n)
    {
        try
        {
            // Ovde nema potrebe za metrima...
            var restoran = await Context.Restorani
                    .OrderBy(p =>
                        Math.Sqrt(
                            Math.Pow(p.X - x, 2) +
                            Math.Pow(p.Y - y, 2)))
                    .Take(n)
                    .ToListAsync();

            return Ok(restoran);
        }
        catch (Exception e)
        {
            return BadRequest(e.ToExceptionString());
        }
    }

    [HttpGet("TipoviRestorana")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> TipoviRestorana()
    {
        try
        {
            var tipovi = await Context.Restorani
                .Include(p => p.TipHrane)
                .Where(p => p.TipHrane != null)
                .Select(p => p.TipHrane!.Tip)
                .Distinct()
                .ToListAsync();
            return Ok(tipovi);
        }
        catch (Exception e)
        {
            return BadRequest(e.ToExceptionString());
        }
    }

    [HttpGet("TipoviRestoranaSaInformacijama")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> TipoviRestoranaSaInformacijama()
    {
        try
        {
            var tipovi = await Context.Restorani
                .Include(p => p.TipHrane)
                .Select(p => new
                {
                    p.ID,
                    p.Naziv,
                    p.TipHrane!.Tip
                })
                .ToListAsync();
            return Ok(tipovi);
        }
        catch (Exception e)
        {
            return BadRequest(e.ToExceptionString());
        }
    }

    [HttpGet("VratiRestoraneTipa")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> VratiRestoraneTipa([FromQuery] string[] tip)
    {
        try
        {
            var restorani = await Context.Restorani
                .Include(p => p.TipHrane)
                .Include(p => p.Grad)
                .Where(p => p.TipHrane != null && tip.Contains(p.TipHrane.Tip))
                .Select(p => new
                {
                    Grad = p.Grad!.Naziv,
                    p.ID,
                    p.Naziv,
                    p.Zarada,
                    p.ProsecnaOcena,
                    p.X,
                    p.Y
                })
                .ToListAsync();

            return Ok(restorani);
        }
        catch (Exception e)
        {
            return BadRequest(e.ToExceptionString());
        }
    }

    [HttpGet("VratiMenijeRestorana/{idRestorana}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> VratiMenijeRestorana(int idRestorana)
    {
        try
        {
            var menijiRestorana = await Context.Jelo
                .Include(p => p.Restoran)
                .Where(p => p.Restoran != null && p.Restoran.ID == idRestorana)
                .ToListAsync();
                
            return Ok(new
            {
                MeniJela = menijiRestorana.Where(p => p.DaLiJeJelo).ToList(),
                MeniPica = menijiRestorana.Where(p => !p.DaLiJeJelo).ToList()
            });
        }
        catch (Exception e)
        {
            return BadRequest(e.ToExceptionString());
        }
    }

    [HttpPost("OcenjivanjeRestorana/{idRestorana}/{ocena}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> OcenjivanjeRestorana(int idRestorana, double ocena)
    {
        try
        {
            if (ocena > 10 || ocena < 0)
            {
                return BadRequest("Vrednost ocene mora da bude između 0 i 10.");
            }

            var restoran = await Context.Restorani.FindAsync(idRestorana);

            if (restoran == null)
            {
                return BadRequest("Restoran sa zadatim ID-jem nije pronađen.");
            }

            restoran.ZbirOcena += ocena;
            restoran.BrojOcena++;

            await Context.SaveChangesAsync();

            return Ok("Ocena uspesno upisana.");
        }
        catch (Exception e)
        {
            return BadRequest(e.ToExceptionString());
        }
    }
}