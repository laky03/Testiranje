namespace PlaywrightTests;

[TestFixture]
public class APITests : PlaywrightTest
{
    private IAPIRequestContext? Request = null;

    [SetUp]
    public async Task Setup()
    {
        var headers = new Dictionary<string, string>
        {
            { "Accept", "application/json" },
            { "Content-Type", "application/json" }
        };
        
        Request = await Playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = "http://localhost:5138/",
            ExtraHTTPHeaders = headers,
            IgnoreHTTPSErrors = true
        });
    }

    [Test]
    public async Task PreuzmiGradoveTest()
    {
        if (Request == null)
        {
            Assert.Fail("Greška u kontekstu.");
            return;
        }

        var gradovi = await Request.GetAsync("Grad/VratiGradoviInfo");

        if (gradovi.Status != 200)
        {
            Assert.Fail($"Code: {gradovi.Status} - {gradovi.StatusText}");
            return;
        }

        var obj = await gradovi.JsonAsync();

        var o = obj.GetValueOrDefault().EnumerateArray().FirstOrDefault();
        if (o.TryGetProperty("naziv", out var naziv) &&
            o.TryGetProperty("brojStanovnika", out var brojStanovnika) &&
            o.TryGetProperty("povrsina", out var povrsina))
        {
            Assert.Multiple(() =>
            {
                Assert.That(naziv.GetString(), Is.EquivalentTo("Niš"));
                Assert.That(brojStanovnika.GetInt32(), Is.GreaterThanOrEqualTo(100000));
                Assert.That(povrsina.GetInt32(), Is.GreaterThan(1000));
            });
        }
        else
        {
            Assert.Fail("Nije pronađen ključ.");
        }
    }

    [TestCase("Kragujevac", 1000, 10000)]
    public async Task DodajGradTest(string nazivGrada, int povrsinaGrada, int brojStanovnikaGrada)
    {
        if (Request == null)
        {
            Assert.Fail("Greška u kontekstu.");
            return;
        }

        var result = await Request.PostAsync("Grad/DodajGrad", new APIRequestContextOptions()
        {
            DataObject = new
            {
                naziv = nazivGrada,
                povrsina = povrsinaGrada,
                brojStanovnika = brojStanovnikaGrada
            }
        });

        if (result.Status != 200)
        {
            Assert.Fail($"Code: {result.Status} - {result.StatusText}");
            return;
        }

        var text = await result.TextAsync();

        Assert.That(text?.Contains("\"Dodat je grad sa ID:") ?? false);
    }

    [TestCase("Kragujevac")]
    [TestCase("Niš")]
    public async Task PreuzmiGradoveCountTest(string nazivGrada)
    {
        if (Request == null)
        {
            Assert.Fail("Greška u kontekstu.");
            return;
        }

        var res = await Request.GetAsync($"/Grad/PreuzmiGrad/{nazivGrada}");

        if (res.Status != 200)
        {
            Assert.Fail($"Code: {res.Status} - {res.StatusText}");
        }

        var json = await res.JsonAsync();
        var length = json?.GetArrayLength();
        Assert.That(length, Is.GreaterThanOrEqualTo(1));
    }

    [TearDown]
    public async Task End()
    {
        if (Request != null)
        {
            await Request.DisposeAsync();
            Request = null;
        }
    }
}
