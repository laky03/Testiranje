namespace Complete.Controllers;

[ApiController]
[Route("[controller]")]
public class GradController : ControllerBase
{
    public RestoraniContext Context { get; set; }

    public GradController(RestoraniContext c)
    {
        Context = c;
    }

    [HttpPost("DodajGrad")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DodajGrad([FromBody]Grad grad)
    {
        try
        {
            if (grad.BrojStanovnika <= 0)
            {
                return BadRequest("Nemoguće dodati grad bez stanovnika.");
            }

            if (string.IsNullOrWhiteSpace(grad.Naziv))
            {
                return BadRequest("Grad mora da ima naziv.");
            }

            if (grad.Povrsina <= 0)
            {
                return BadRequest("Grad mora da ima površinu.");
            }

            await Context.Gradovi.AddAsync(grad);
            await Context.SaveChangesAsync();
            return Ok($"Dodat je grad sa ID: {grad.ID} i nazivom: {grad.Naziv}.");
        }
        catch (Exception e)
        {
            return BadRequest(e.ToExceptionString());
        }
    }

    [HttpPut("IzmeniGrad/{id}/{naziv}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> IzmeniGrad(int id, string naziv)
    {
        try
        {
            var stariGrad = await Context.Gradovi.FindAsync(id);

            if (stariGrad != null)
            {
                stariGrad.Naziv = naziv;
                Context.Gradovi.Update(stariGrad);
                await Context.SaveChangesAsync();

                return Ok($"Uspešno izmenjen grad sa ID: {id}. Novi naziv je: {naziv}.");
            }
            else
            {
                return BadRequest("Grad nije pronađen.");
            }
        }
        catch (Exception e)
        {
            return BadRequest(e.ToExceptionString());
        }
    }

    [HttpPut("IzmeniGradFromBody")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> IzmeniGradFromBody([FromBody]Grad grad)
    {
        try
        {
            if (grad == null)
            {
                return BadRequest("Grad nije pronađen.");
            }

            if (grad.BrojStanovnika <= 0 || 
                string.IsNullOrWhiteSpace(grad.Naziv) || 
                grad.Povrsina <= 0 || 
                grad.ID <= 0)
            {
                return BadRequest("Podaci grada nisu validni.");
            }

            Context.Gradovi.Update(grad);
            await Context.SaveChangesAsync();

            return Ok($"Uspešno izmenjen grad sa ID: {grad.ID}. Novi naziv je: {grad.Naziv}.");
        }
        catch (Exception e)
        {
            return BadRequest(e.ToExceptionString());
        }
    }

    [HttpDelete("IzbrisiGrad/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> IzbrisiGrad(int id)
    {
        try
        {
            var gradZaBrisanje = await Context.Gradovi.FindAsync(id);

            if (gradZaBrisanje != null)
            {
                string naziv = gradZaBrisanje.Naziv;
                Context.Gradovi.Remove(gradZaBrisanje);
                await Context.SaveChangesAsync();

                return Ok($"Grad sa Nazivom: {naziv} je uspešno obrisan.");
            }
            else
            {
                return BadRequest("Ne postoji grad sa zadatim ID-jem.");
            }
        }
        catch (Exception e)
        {
            return BadRequest(e.ToExceptionString());
        }
    }

    [Produces("text/xml")]
    [HttpGet("PreuzmiGradXML/{naziv}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PreuzmiGradXML(string naziv)
    {
        var gradovi = await Context
            .Gradovi
            .Where(p => p.Naziv == naziv)
            .ToListAsync();
        return Ok(gradovi);
    }

    [HttpGet("PreuzmiGrad/{naziv}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PreuzmiGrad(string naziv)
    {
        var gradovi = await Context
            .Gradovi
            .Where(p => p.Naziv == naziv)
            .ToListAsync();
        return Ok(gradovi);
    }

    [HttpGet("VratiGradoviInfo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VratiGradoviInfo()
    {
        try
        {
            // Druga opcija, bez korišćenja anonimnih tipova je da u fajlu kreiramo klasu ili record
            // public record GradPodaci(int Identifikator, string Naziv);
            // a onda da vratimo objekte tog tipa
            // Select(p => new GradPodaci(p.ID, p.Naziv))...

            // Uvek bi trebalo da se podaci tako i šalju, da se klase modela ne koriste za slanje i prijem podataka
            // sa klijenta, već da se kreiraju nove klase za to. Mi ovako radimo samo zbog jednostavnosti
            // i ograničenom vremenu na ispitu

            var gradovi = await Context
                .Gradovi
                .Select(p => new
                {
                    Identifikator = p.ID,
                    p.Naziv,
                    p.BrojStanovnika,   // Nije neophodno
                    p.Povrsina          // Takođe
                })
                .ToListAsync();
            return Ok(gradovi);
        }
        catch (Exception e)
        {
            return BadRequest(e.ToExceptionString());
        }
    }
}