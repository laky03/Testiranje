namespace PlaywrightTests;

[TestFixture]
public partial class WebAppTests : PageTest
{
    private IBrowser? BrowserWithSettings { get; set; }
    private IPage? PageWithSettings { get; set; }

    [GeneratedRegex("Restorani")]
    private static partial Regex TitleRegex();

    [SetUp]
    public async Task Setup()
    {
        BrowserWithSettings = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            SlowMo = 1000
        });

        PageWithSettings = await BrowserWithSettings.NewPageAsync(new BrowserNewPageOptions()
        {
            ViewportSize = new()
            {
                Width = 1280,
                Height = 720
            },
            ScreenSize = new()
            {
                Width = 1280,
                Height = 720
            },
            RecordVideoSize = new()
            {
                Width = 1280,
                Height = 720
            },
            RecordVideoDir = "../../../Videos"
        });
    }

    [Test]
    public async Task CheckTitle()
    {
        if (PageWithSettings == null)
        {
            Assert.Fail("Greška, stranica ne postoji.");
            return;
        }

        await PageWithSettings.GotoAsync("http://127.0.0.1:8000/");
        await Expect(PageWithSettings).ToHaveTitleAsync(TitleRegex());

        await PageWithSettings.ScreenshotAsync(new()
        {
            FullPage = true,
            Path = "../../../Images/Screenshot1.png"
        });

        await PageWithSettings.Locator("select").First.SelectOptionAsync("1");

        await PageWithSettings.ScreenshotAsync(new()
        {
            FullPage = true,
            Path = "../../../Images/Screenshot2.png"
        });
    }

    [Test]
    public async Task PrikazTest()
    {
        if (PageWithSettings == null)
        {

            Assert.Fail("Greška, stranica ne postoji.");
            return;
        }

        await PageWithSettings.GotoAsync("http://127.0.0.1:8000/");
        await PageWithSettings.ScreenshotAsync(new PageScreenshotOptions()
        {
            FullPage = true,
            Path = "../../../Images/Screenshot3.png"
        });

        await PageWithSettings.Locator("select").First.SelectOptionAsync("1");
        await PageWithSettings.Locator(".meni-button").First.ClickAsync();
        string? text = await PageWithSettings
            .Locator(".menu-list>li.food")
            .Filter(new()
            {
                HasText = "882"
            })
            .Locator("span")
            .Nth(0)
            .TextContentAsync();
        Assert.That(text ?? "", Is.EqualTo("Rižoto (882 kcal)"));
    }

    [Test]
    public async Task TestLokacije()
    {
        if (PageWithSettings == null)
        {
            Assert.Fail("Greška, stranica ne postoji.");
            return;
        }

        await PageWithSettings.GotoAsync("http://127.0.0.1:8000/");

        await PageWithSettings.Locator("select").First.SelectOptionAsync("1");
        await PageWithSettings.Locator("input[type='number']").First.FillAsync("33");
        await PageWithSettings.Locator("input[type='number']").Nth(1).FillAsync("44");
        await PageWithSettings.Locator("input[type='number']").Nth(2).FillAsync("1201000");
        await PageWithSettings.Locator(".tip-hrane").SelectOptionAsync("Italijanska");
        await PageWithSettings.Locator(".filter-button").ClickAsync();
        await PageWithSettings
            .Locator(".meni-button")
            .Filter(new()
            {
                HasText = "Meni pića"
            })
            .First
            .ClickAsync();

        await Expect(PageWithSettings.GetByText("Sok (244 kcal)")).ToBeVisibleAsync();

        await PageWithSettings.ScreenshotAsync(new() { Path = "../../../Images/Screenshot4.png" });
    }

    [TearDown]
    public async Task Teardown()
    {
        if (BrowserWithSettings != null)
        {
            await BrowserWithSettings.DisposeAsync();
            BrowserWithSettings = null;
        }

        if (PageWithSettings != null)
        {
            await PageWithSettings.CloseAsync();
            PageWithSettings = null;
        }
    }
}
