using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace SplitSync.PlaywrightTests;

[TestFixture]
public partial class WebAppTests : PageTest
{
    private IBrowser? BrowserWithSettings { get; set; }
    private IPage? PageWithSettings { get; set; }

    [GeneratedRegex("Prijava|SplitSync")]
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
            }
        });
    }

    [Test]
    public async Task LoginPageLoads()
    {
        if (PageWithSettings == null)
        {
            Assert.Fail("Greška, stranica ne postoji.");
            return;
        }

        await PageWithSettings.GotoAsync("http://localhost:5118/Account/Login");
        await Expect(PageWithSettings).ToHaveTitleAsync(TitleRegex());
        await Expect(PageWithSettings.GetByText("Prijava")).ToBeVisibleAsync();

        await PageWithSettings.ScreenshotAsync(new()
        {
            FullPage = true,
            Path = "../../../Images/LoginPage.png"
        });
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