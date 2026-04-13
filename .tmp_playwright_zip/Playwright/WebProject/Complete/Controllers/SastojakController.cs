namespace Complete.Controllers;

[ApiController]
[Route("[controller]")]
public class SastojakController : ControllerBase
{
    public RestoraniContext Context { get; set; }

    public SastojakController(RestoraniContext c)
    {
        Context = c;
    }

    [HttpPost("DodavanjeSastojka/{gramaPoPakovanju}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DodavanjeSastojka([FromBody] Sastojak sastojak, int gramaPoPakovanju)
    {
        try
        {
            if (sastojak.Cena <= 0 || sastojak.RokTrajanja < DateTime.Now)
            {
                return BadRequest("Sastojak je neispravan.");
            }

            sastojak.Cena = sastojak.Cena / gramaPoPakovanju;

            await Context.Sastojci.AddAsync(sastojak);
            await Context.SaveChangesAsync();

            return Ok("Sastojak uspesno dodat.");
        }
        catch (Exception e)
        {
            return BadRequest(e.ToExceptionString());
        }
    }

    [HttpPost("DodavanjeSastojkaMagacinuRestorana/{idSastojka}/{idRestorana}/{kolicina}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DodavanjeSastojkaMagacinuRestorana(int idSastojka, int idRestorana, int kolicina)
    {
        try
        {
            if (kolicina <= 0)
            {
                return BadRequest("Neispravna količina sastojka.");
            }

            var restoran = await Context.Restorani
                .Include(p => p.Magacin)
                .Where(p => p.ID == idRestorana)
                .FirstOrDefaultAsync();
            var sastojak = await Context.Sastojci.FindAsync(idSastojka);

            if (restoran == null || sastojak == null || restoran.Magacin == null)
            {
                return BadRequest("Sastojak ili restoran ili njegov magacin nisu pronadjeni.");
            }

            restoran.Magacin.Add(new Magacin()
            {
                Restoran = restoran,
                Sastojak = sastojak,
                Kolicina = kolicina
            });

            await Context.SaveChangesAsync();

            return Ok("Dodat sastojak u magacin.");
        }
        catch (Exception e)
        {
            return BadRequest(e.ToExceptionString());
        }
    }

    [HttpGet("CenaProizvoda/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CenaProizvoda(int id)
    {
        try
        {
            var jelo = await Context.Jelo
                .Include(p => p.Sastojci)!
                .ThenInclude(p => p.Sastojak)
                .Where(p => p.ID == id)
                .FirstOrDefaultAsync();

            double cena = 0;

            if (jelo == null || jelo.Sastojci == null)
            {
                return BadRequest("Ne postoji sastojak ili proizvod.");
            }

            foreach (var s in jelo.Sastojci)
            {
                if (s.Sastojak == null)
                {
                    return BadRequest("Sastojak ne postoji.");
                }

                cena += s.Kolicina * s.Sastojak.Cena;
            }

            return Ok(cena);
        }
        catch (Exception e)
        {
            return BadRequest(e.ToExceptionString());
        }
    }
}
