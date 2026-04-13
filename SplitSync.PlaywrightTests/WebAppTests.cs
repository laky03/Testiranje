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



    //[Test]
    //public async Task LoginPageLoads()
    //{
    //    if (PageWithSettings == null)
    //    {
    //        Assert.Fail("Greška, stranica ne postoji.");
    //        return;
    //    }

    //    await PageWithSettings.GotoAsync("http://localhost:5118/Account/Login");
    //    await Expect(PageWithSettings).ToHaveTitleAsync(TitleRegex());
    //    await Expect(PageWithSettings.GetByText("Prijava")).ToBeVisibleAsync();

    //    await PageWithSettings.ScreenshotAsync(new()
    //    {
    //        FullPage = true,
    //        Path = "../../../Images/LoginPage.png"
    //    });
    //}

    //[Test]
    //public async Task LoginFailsWithInvalidCredentials()
    //{
    //    if (PageWithSettings == null)
    //    {
    //        Assert.Fail("Greška, stranica ne postoji.");
    //        return;
    //    }

    //    await PageWithSettings.GotoAsync("http://localhost:5118/Account/Login");

    //    await PageWithSettings.Locator("input[placeholder='Username ili Email']")
    //        .FillAsync("pogresanuser");

    //    await PageWithSettings.Locator("input[placeholder='Lozinka']")
    //        .FillAsync("pogresnalozinka");

    //    await PageWithSettings.GetByRole(AriaRole.Button, new() { Name = "Prijavi se" })
    //        .ClickAsync();

    //    await Expect(PageWithSettings.GetByText("Pogrešni kredencijali."))
    //        .ToBeVisibleAsync();

    //    await PageWithSettings.ScreenshotAsync(new()
    //    {
    //        FullPage = true,
    //        Path = "../../../Images/LoginFail.png"
    //    });
    //}

    //[Test]
    //public async Task LoginSucceedsWithValidCredentials()
    //{
    //    if (PageWithSettings == null)
    //    {
    //        Assert.Fail("Greška, stranica ne postoji.");
    //        return;
    //    }

    //    await PageWithSettings.GotoAsync("http://localhost:5118/Account/Login");

    //    await PageWithSettings.Locator("input[placeholder='Username ili Email']")
    //        .FillAsync("BogdanFaks");

    //    await PageWithSettings.Locator("input[placeholder='Lozinka']")
    //        .FillAsync("bogdan003");

    //    await PageWithSettings.GetByRole(AriaRole.Button, new() { Name = "Prijavi se" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);

    //    await Expect(PageWithSettings).Not.ToHaveURLAsync(new Regex(".*/Account/Login$"));
    //    await Expect(PageWithSettings.GetByText("Logout")).ToBeVisibleAsync();
    //    await Expect(PageWithSettings.GetByText("Grupe")).ToBeVisibleAsync();

    //    await PageWithSettings.ScreenshotAsync(new()
    //    {
    //        FullPage = true,
    //        Path = "../../../Images/LoginSuccess.png"
    //    });
    //}



    //[Test]
    //public async Task LogoutWorksAfterSuccessfulLogin()
    //{
    //    if (PageWithSettings == null)
    //    {
    //        Assert.Fail("Greška, stranica ne postoji.");
    //        return;
    //    }

    //    await PageWithSettings.GotoAsync("http://localhost:5118/Account/Login");

    //    await PageWithSettings.Locator("input[placeholder='Username ili Email']")
    //        .FillAsync("BogdanFaks");

    //    await PageWithSettings.Locator("input[placeholder='Lozinka']")
    //        .FillAsync("bogdan003");

    //    await PageWithSettings.GetByRole(AriaRole.Button, new() { Name = "Prijavi se" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);

    //    await PageWithSettings.Locator(".app-menu-icon").First.ClickAsync();
    //    await PageWithSettings.GetByText("Logout").ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);
    //    await Expect(PageWithSettings.GetByText("Prijava")).ToBeVisibleAsync();

    //    await PageWithSettings.ScreenshotAsync(new()
    //    {
    //        FullPage = true,
    //        Path = "../../../Images/LogoutSuccess.png"
    //    });
    //}

    //[Test]
    //public async Task CreateNewGroupWithoutImage()
    //{
    //    if (PageWithSettings == null)
    //    {
    //        Assert.Fail("Greška, stranica ne postoji.");
    //        return;
    //    }

    //    var groupName = $"PW Grupa {DateTime.Now:yyyyMMddHHmmss}";

    //    await PageWithSettings.GotoAsync("http://localhost:5118/Account/Login");

    //    await PageWithSettings.Locator("input[placeholder='Username ili Email']")
    //        .FillAsync("BogdanFaks");

    //    await PageWithSettings.Locator("input[placeholder='Lozinka']")
    //        .FillAsync("bogdan003");

    //    await PageWithSettings.GetByRole(AriaRole.Button, new() { Name = "Prijavi se" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);

    //    await PageWithSettings.Locator(".app-menu-icon").First.ClickAsync();
    //    await PageWithSettings.GetByRole(AriaRole.Link, new() { Name = "Grupe" }).ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);
    //    await Expect(PageWithSettings.GetByRole(AriaRole.Heading, new() { Name = "Moje grupe" }))
    //        .ToBeVisibleAsync();

    //    await PageWithSettings.GetByRole(AriaRole.Link, new() { Name = "Nova grupa" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);
    //    await Expect(PageWithSettings.GetByRole(AriaRole.Heading, new() { Name = "Napravi grupu" }))
    //        .ToBeVisibleAsync();

    //    await PageWithSettings.Locator("#Name").FillAsync(groupName);

    //    await PageWithSettings.GetByRole(AriaRole.Button, new() { Name = "Napravi grupu" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);

    //    await Expect(PageWithSettings).Not.ToHaveURLAsync(new Regex(".*/Groups/Create$"));
    //    await Expect(PageWithSettings.GetByRole(AriaRole.Heading, new() { Name = groupName }))
    //        .ToBeVisibleAsync();

    //    await PageWithSettings.ScreenshotAsync(new()
    //    {
    //        FullPage = true,
    //        Path = "../../../Images/CreateGroupSuccess.png"
    //    });
    //}


    //[Test]
    //public async Task CreateGroupFailsWithoutName()
    //{
    //    if (PageWithSettings == null)
    //    {
    //        Assert.Fail("Greška, stranica ne postoji.");
    //        return;
    //    }

    //    await PageWithSettings.GotoAsync("http://localhost:5118/Account/Login");

    //    await PageWithSettings.Locator("input[placeholder='Username ili Email']")
    //        .FillAsync("BogdanFaks");

    //    await PageWithSettings.Locator("input[placeholder='Lozinka']")
    //        .FillAsync("bogdan003");

    //    await PageWithSettings.GetByRole(AriaRole.Button, new() { Name = "Prijavi se" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);

    //    await PageWithSettings.Locator(".app-menu-icon").First.ClickAsync();
    //    await PageWithSettings.GetByRole(AriaRole.Link, new() { Name = "Grupe" }).ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);

    //    await PageWithSettings.GetByRole(AriaRole.Link, new() { Name = "Nova grupa" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);
    //    await Expect(PageWithSettings.GetByRole(AriaRole.Heading, new() { Name = "Napravi grupu" }))
    //        .ToBeVisibleAsync();

    //    await PageWithSettings.GetByRole(AriaRole.Button, new() { Name = "Napravi grupu" })
    //        .ClickAsync();

    //    await Expect(PageWithSettings).ToHaveURLAsync(new Regex(".*/Groups/Create$"));
    //    Assert.That(
    //        await PageWithSettings.Locator("#Name").EvaluateAsync<bool>("el => !el.checkValidity()"),
    //        Is.True
    //    );

    //    await PageWithSettings.ScreenshotAsync(new()
    //    {
    //        FullPage = true,
    //        Path = "../../../Images/CreateGroupFailNoName.png"
    //    });
    //}

    //[Test]
    //public async Task CreateNewBillForCurrentUser()
    //{
    //    if (PageWithSettings == null)
    //    {
    //        Assert.Fail("Greška, stranica ne postoji.");
    //        return;
    //    }

    //    var groupName = $"PW Grupa {DateTime.Now:yyyyMMddHHmmss}";
    //    var billName = $"PW Racun {DateTime.Now:yyyyMMddHHmmss}";
    //    var amount = new Random().Next(100, 5000).ToString();

    //    await PageWithSettings.GotoAsync("http://localhost:5118/Account/Login");

    //    await PageWithSettings.Locator("input[placeholder='Username ili Email']")
    //        .FillAsync("BogdanFaks");

    //    await PageWithSettings.Locator("input[placeholder='Lozinka']")
    //        .FillAsync("bogdan003");

    //    await PageWithSettings.GetByRole(AriaRole.Button, new() { Name = "Prijavi se" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);

    //    await PageWithSettings.Locator(".app-menu-icon").First.ClickAsync();
    //    await PageWithSettings.GetByRole(AriaRole.Link, new() { Name = "Grupe" }).ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);
    //    await PageWithSettings.GetByRole(AriaRole.Link, new() { Name = "Nova grupa" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);
    //    await PageWithSettings.Locator("#Name").FillAsync(groupName);

    //    await PageWithSettings.GetByRole(AriaRole.Button, new() { Name = "Napravi grupu" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);
    //    await Expect(PageWithSettings.GetByRole(AriaRole.Heading, new() { Name = groupName }))
    //        .ToBeVisibleAsync();

    //    await PageWithSettings.GetByRole(AriaRole.Link, new() { Name = "Računi" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);

    //    await PageWithSettings.GetByRole(AriaRole.Link, new() { Name = "Novi račun" })
    //        .First
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);
    //    await Expect(PageWithSettings.GetByRole(AriaRole.Heading, new() { Name = "Novi račun" }))
    //        .ToBeVisibleAsync();

    //    await PageWithSettings.Locator("input[name='Naziv']").FillAsync(billName);
    //    await PageWithSettings.Locator("select[name='Clanovi[0].UserId']").SelectOptionAsync("1");
    //    await PageWithSettings.Locator("input[name='Clanovi[0].Iznos']").FillAsync(amount);
    //    await PageWithSettings.Locator("input[name='Clanovi[0].DeoRacuna']").FillAsync(amount);

    //    await PageWithSettings.GetByRole(AriaRole.Button, new() { Name = "Sačuvaj" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);

    //    await Expect(PageWithSettings.GetByText(billName)).ToBeVisibleAsync();

    //    await PageWithSettings.ScreenshotAsync(new()
    //    {
    //        FullPage = true,
    //        Path = "../../../Images/CreateBillSuccess.png"
    //    });
    //}

    //[Test]
    //public async Task CreateNewEventWithoutImage()
    //{
    //    if (PageWithSettings == null)
    //    {
    //        Assert.Fail("Greška, stranica ne postoji.");
    //        return;
    //    }

    //    var groupName = $"PW Grupa {DateTime.Now:yyyyMMddHHmmss}";
    //    var eventName = $"PW Dogadjaj {DateTime.Now:yyyyMMddHHmmss}";
    //    var eventDescription = "Playwright test opis događaja";
    //    var eventLocation = "Nis";
    //    var eventTime = DateTime.Now.AddDays(1).ToString("yyyy-MM-ddTHH:mm");

    //    await PageWithSettings.GotoAsync("http://localhost:5118/Account/Login");

    //    await PageWithSettings.Locator("input[placeholder='Username ili Email']")
    //        .FillAsync("BogdanFaks");

    //    await PageWithSettings.Locator("input[placeholder='Lozinka']")
    //        .FillAsync("bogdan003");

    //    await PageWithSettings.GetByRole(AriaRole.Button, new() { Name = "Prijavi se" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);

    //    await PageWithSettings.Locator(".app-menu-icon").First.ClickAsync();
    //    await PageWithSettings.GetByRole(AriaRole.Link, new() { Name = "Grupe" }).ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);
    //    await PageWithSettings.GetByRole(AriaRole.Link, new() { Name = "Nova grupa" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);
    //    await PageWithSettings.Locator("#Name").FillAsync(groupName);

    //    await PageWithSettings.GetByRole(AriaRole.Button, new() { Name = "Napravi grupu" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);
    //    await Expect(PageWithSettings.GetByRole(AriaRole.Heading, new() { Name = groupName }))
    //        .ToBeVisibleAsync();

    //    await PageWithSettings.GetByRole(AriaRole.Link, new() { Name = "Događaji" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);
    //    await Expect(PageWithSettings.GetByRole(AriaRole.Heading, new() { Name = "Događaji" }))
    //        .ToBeVisibleAsync();

    //    await PageWithSettings.GetByRole(AriaRole.Link, new() { Name = "Novi događaj" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);
    //    await Expect(PageWithSettings.GetByRole(AriaRole.Heading, new() { Name = "Novi događaj" }))
    //        .ToBeVisibleAsync();

    //    await PageWithSettings.Locator("input[name='Naziv']").FillAsync(eventName);
    //    await PageWithSettings.Locator("textarea[name='Opis']").FillAsync(eventDescription);
    //    await PageWithSettings.Locator("input[name='Lokacija']").FillAsync(eventLocation);
    //    await PageWithSettings.Locator("input[name='VremeDogadjaja']").FillAsync(eventTime);

    //    await PageWithSettings.GetByRole(AriaRole.Button, new() { Name = "Kreiraj" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);

    //    await Expect(PageWithSettings.GetByText(eventName)).ToBeVisibleAsync();
    //    await Expect(PageWithSettings.GetByText(eventLocation)).ToBeVisibleAsync();

    //    await PageWithSettings.ScreenshotAsync(new()
    //    {
    //        FullPage = true,
    //        Path = "../../../Images/CreateEventSuccess.png"
    //    });
    //}

    //[Test]
    //public async Task CreateNewEventWithoutImage()
    //{
    //    if (PageWithSettings == null)
    //    {
    //        Assert.Fail("Greška, stranica ne postoji.");
    //        return;
    //    }

    //    var groupName = $"PW Grupa {DateTime.Now:yyyyMMddHHmmss}";
    //    var eventName = $"PW Dogadjaj {DateTime.Now:yyyyMMddHHmmss}";
    //    var eventDescription = "Playwright test opis događaja";
    //    var eventLocation = "Nis";
    //    var eventTime = DateTime.Now.AddDays(1).ToString("yyyy-MM-ddTHH:mm");

    //    await PageWithSettings.GotoAsync("http://localhost:5118/Account/Login");

    //    await PageWithSettings.Locator("input[placeholder='Username ili Email']")
    //        .FillAsync("BogdanFaks");

    //    await PageWithSettings.Locator("input[placeholder='Lozinka']")
    //        .FillAsync("bogdan003");

    //    await PageWithSettings.GetByRole(AriaRole.Button, new() { Name = "Prijavi se" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);

    //    await PageWithSettings.Locator(".app-menu-icon").First.ClickAsync();
    //    await PageWithSettings.GetByRole(AriaRole.Link, new() { Name = "Grupe" }).ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);
    //    await PageWithSettings.GetByRole(AriaRole.Link, new() { Name = "Nova grupa" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);
    //    await PageWithSettings.Locator("#Name").FillAsync(groupName);

    //    await PageWithSettings.GetByRole(AriaRole.Button, new() { Name = "Napravi grupu" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);
    //    await Expect(PageWithSettings.GetByRole(AriaRole.Heading, new() { Name = groupName }))
    //        .ToBeVisibleAsync();

    //    await PageWithSettings.GetByRole(AriaRole.Link, new() { Name = "Događaji" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);
    //    await Expect(PageWithSettings.GetByRole(AriaRole.Heading, new() { Name = "Događaji" }))
    //        .ToBeVisibleAsync();

    //    await PageWithSettings.GetByRole(AriaRole.Link, new() { Name = "Novi događaj" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);
    //    await Expect(PageWithSettings.GetByRole(AriaRole.Heading, new() { Name = "Novi događaj" }))
    //        .ToBeVisibleAsync();

    //    await PageWithSettings.Locator("input[name='Naziv']").FillAsync(eventName);
    //    await PageWithSettings.Locator("textarea[name='Opis']").FillAsync(eventDescription);
    //    await PageWithSettings.Locator("input[name='Lokacija']").FillAsync(eventLocation);
    //    await PageWithSettings.Locator("input[name='VremeDogadjaja']").FillAsync(eventTime);

    //    await PageWithSettings.GetByRole(AriaRole.Button, new() { Name = "Kreiraj" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);

    //    await Expect(PageWithSettings.GetByText(eventName)).ToBeVisibleAsync();
    //    await Expect(PageWithSettings.GetByText(eventLocation)).ToBeVisibleAsync();

    //    await PageWithSettings.ScreenshotAsync(new()
    //    {
    //        FullPage = true,
    //        Path = "../../../Images/CreateEventSuccess.png"
    //    });
    //}

    //[Test]
    //public async Task MarkShoppingItemAsPurchased()
    //{
    //    if (PageWithSettings == null)
    //    {
    //        Assert.Fail("Greška, stranica ne postoji.");
    //        return;
    //    }

    //    var groupName = $"PW Grupa {DateTime.Now:yyyyMMddHHmmss}";
    //    var itemName = $"PW Stavka {DateTime.Now:yyyyMMddHHmmss}";

    //    await PageWithSettings.GotoAsync("http://localhost:5118/Account/Login");

    //    await PageWithSettings.Locator("input[placeholder='Username ili Email']")
    //        .FillAsync("BogdanFaks");

    //    await PageWithSettings.Locator("input[placeholder='Lozinka']")
    //        .FillAsync("bogdan003");

    //    await PageWithSettings.GetByRole(AriaRole.Button, new() { Name = "Prijavi se" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);

    //    await PageWithSettings.Locator(".app-menu-icon").First.ClickAsync();
    //    await PageWithSettings.GetByRole(AriaRole.Link, new() { Name = "Grupe" }).ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);
    //    await PageWithSettings.GetByRole(AriaRole.Link, new() { Name = "Nova grupa" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);
    //    await PageWithSettings.Locator("#Name").FillAsync(groupName);

    //    await PageWithSettings.GetByRole(AriaRole.Button, new() { Name = "Napravi grupu" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);
    //    await Expect(PageWithSettings.GetByRole(AriaRole.Heading, new() { Name = groupName }))
    //        .ToBeVisibleAsync();

    //    await PageWithSettings.GetByRole(AriaRole.Link, new() { Name = "Shopping lista", Exact = true })
    //        .Last
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);

    //    await PageWithSettings.Locator("input[name='naziv']").FillAsync(itemName);

    //    await PageWithSettings.GetByRole(AriaRole.Button, new() { Name = "Dodaj" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);
    //    await Expect(PageWithSettings.GetByText(itemName)).ToBeVisibleAsync();

    //    await PageWithSettings.GetByRole(AriaRole.Button, new() { Name = "Nabavljeno" })
    //        .ClickAsync();

    //    await PageWithSettings.WaitForLoadStateAsync(LoadState.NetworkIdle);

    //    await Expect(PageWithSettings.GetByText("Nabavljeno")).ToBeVisibleAsync();
    //    await Expect(PageWithSettings.GetByText(itemName)).ToBeVisibleAsync();

    //    await PageWithSettings.ScreenshotAsync(new()
    //    {
    //        FullPage = true,
    //        Path = "../../../Images/ShoppingItemPurchased.png"
    //    });
    //}

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