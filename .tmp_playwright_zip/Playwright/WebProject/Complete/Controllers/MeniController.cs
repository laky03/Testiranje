namespace Complete.Controllers;

[ApiController]
[Route("[controller]")]
public class MeniController : ControllerBase
{
    public RestoraniContext Context { get; set; }

    public MeniController(RestoraniContext c)
    {
        Context = c;
    }

    [HttpPost("DodavanjeJela/{restoranID}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DodavanjeJela([FromBody] JeloSaSastojcima jeloSaSastojcima, int restoranID)
    {
        try
        {
            var restoran = await Context.Restorani
                .Include(p => p.Meni)
                .Where(p => p.ID == restoranID)
                .FirstOrDefaultAsync();

            Jelo jelo = jeloSaSastojcima.Jelo;
            jelo.Sastojci = [];

            if (restoran == null || restoran.Meni == null || jelo == null)
            {
                return BadRequest("Restoran nije pronađen ili jelo nije validno.");
            }

            foreach (var s in jeloSaSastojcima.Sastojci)
            {
                var sastojak = await Context.Sastojci.FindAsync(s.SastojakID);

                if (sastojak == null)
                {
                    return BadRequest($"Sastojak sa ID: {s.SastojakID} ne postoji.");
                }

                JeloSastojak js = new()
                {
                    Jelo = jelo,
                    Sastojak = sastojak,
                    Kolicina = s.Kolicina
                };

                await Context.Recept.AddAsync(js);
            }

            restoran.Meni.Add(jelo);
            jelo.Restoran = restoran;

            Context.Restorani.Update(restoran);
            await Context.SaveChangesAsync();
            return Ok("Jelo uspešno upisano.");
        }
        catch (Exception e)
        {
            return BadRequest(e.ToExceptionString());
        }
    }

    [HttpDelete("BrisanjeJela/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> BrisanjeJela(int id)
    {
        try
        {
            var jelo = await Context.Jelo
                .Include(p => p.Sastojci)
                .Where(p => p.ID == id)
                .FirstOrDefaultAsync();

            if (jelo == null)
            {
                return BadRequest("Jelo ne postoji.");
            }

            if (jelo.Sastojci != null)
            {
                // Da bi se obezbedili da su veze obrisane pre brisanja Jela
                // Cascade delete takođe može da se podesi
                // Ali on radi sa strane gde je strani ključ, u ovom slučaju JeloSastojak
                // Pa sa ove strane neće da radi
                foreach (var recept in jelo.Sastojci)
                {
                    Context.Recept.Remove(recept);
                }
            }

            Context.Jelo.Remove(jelo);
            await Context.SaveChangesAsync();

            return Ok($"Jelo sa ID: {id} obrisano.");
        }
        catch(Exception e)
        {
            return BadRequest(e.ToExceptionString());
        }
    }

    // GET ili PUT
    // Sa strane korisnika je GET
    // Sa strane restorana je PUT
    [HttpGet("NarucivanjeJela/{idJela}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> NarucivanjeJela(int idJela)
    {
        try
        {
            var jelo = await Context.Jelo
                .Include(p => p.Sastojci!)
                .ThenInclude(p => p.Sastojak)
                .Include(p => p.Restoran)
                .ThenInclude(p => p!.Magacin)
                .Where(p => p.ID == idJela)
                .FirstOrDefaultAsync();

            if (jelo == null)
            {
                return BadRequest("Jelo ne postoji.");
            }

            // Svi sastojci moraju da budu u magacinu restorana!
            if (!await Kuhinja.ProveriDaLiMozeDaSePripremi(Context, jelo))
            {
                return BadRequest("Žao nam je, trenutno ne možemo da pripremimo to jelo.");
            }

            if (await Kuhinja.PripremiJelo(Context, jelo, 2, 200))
            {
                return Ok("Jelo će uskoro biti pripremljeno. Prijatno!");
            }
            else
            {
                return BadRequest("Nažalost, imamo problema u kuhinji!");
            }
        }
        catch (Exception e)
        {
            return BadRequest(e.ToExceptionString());
        }
    }
}
