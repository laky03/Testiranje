using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace SplitSync.PlaywrightTests;

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
            BaseURL = "http://localhost:5118/",
            ExtraHTTPHeaders = headers,
            IgnoreHTTPSErrors = true
        });
    }

    [Test]
    public async Task PostGroup_Creates_New_Group()
    {
        if (Request == null)
        {
            Assert.Fail("Greška u API kontekstu.");
            return;
        }

        var groupName = "PW-Test-Group-" + DateTime.UtcNow.Ticks;

        var response = await Request.PostAsync("api/groups", new APIRequestContextOptions
        {
            DataObject = new
            {
                name = groupName,
                ownerUserId = 1,
                defaultValuta = "RSD"
            }
        });

        if (response.Status != 201)//post za uspesno kreiranje treba da vrati 201 ako ne vrati 201 onda nije kreiran uspesno
        {
            var text = await response.TextAsync();
            Assert.Fail($"Code: {response.Status} - {response.StatusText} - {text}");
            return;
        }

        var json = await response.JsonAsync();

        Assert.That(json, Is.Not.Null);//proveravas da si dobio neki JSON odgovor
    }


    [Test]
    public async Task GetGroups_Returns_Newly_Created_Group()
    {
        if (Request == null)
        {
            Assert.Fail("Greška u API kontekstu.");
            return;
        }

        var groupName = "PW-Test-Group-" + DateTime.UtcNow.Ticks;

        var postResponse = await Request.PostAsync("api/groups", new APIRequestContextOptions
        {
            DataObject = new
            {
                name = groupName,
                ownerUserId = 1,
                defaultValuta = "RSD"
            }
        });

        if (postResponse.Status != 201)
        {
            var postText = await postResponse.TextAsync();
            Assert.Fail($"POST failed: {postResponse.Status} - {postResponse.StatusText} - {postText}");
            return;
        }

        var getResponse = await Request.GetAsync("api/groups");

        if (getResponse.Status != 200)
        {
            var getText = await getResponse.TextAsync();
            Assert.Fail($"GET failed: {getResponse.Status} - {getResponse.StatusText} - {getText}");
            return;
        }

        var text = await getResponse.TextAsync();
        using var document = System.Text.Json.JsonDocument.Parse(text);
        var groups = document.RootElement.EnumerateArray().ToList();

        Assert.That(groups.Any(g =>
            g.TryGetProperty("name", out var name) &&
            name.GetString() == groupName), Is.True);
    }

    [Test]
    public async Task PutGroup_Updates_Existing_Group()
    {
        if (Request == null)
        {
            Assert.Fail("Greška u API kontekstu.");
            return;
        }

        var groupName = "PW-Test-Group-" + DateTime.UtcNow.Ticks;

        var postResponse = await Request.PostAsync("api/groups", new APIRequestContextOptions
        {
            DataObject = new
            {
                name = groupName,
                ownerUserId = 1,
                defaultValuta = "RSD"
            }
        });

        if (postResponse.Status != 201)
        {
            var postText = await postResponse.TextAsync();
            Assert.Fail($"POST failed: {postResponse.Status} - {postResponse.StatusText} - {postText}");
            return;
        }

        var postTextJson = await postResponse.TextAsync();
        using var postDocument = System.Text.Json.JsonDocument.Parse(postTextJson);

        if (!postDocument.RootElement.TryGetProperty("id", out var idProperty))
        {
            Assert.Fail("ID kreirane grupe nije pronađen.");
            return;
        }

        var groupId = idProperty.GetInt64();

        var updatedName = groupName + "-Updated";

        var putResponse = await Request.PutAsync($"api/groups/{groupId}", new APIRequestContextOptions
        {
            DataObject = new
            {
                name = updatedName,
                ownerUserId = 1,
                defaultValuta = "EUR"
            }
        });

        Assert.That(putResponse.Status, Is.EqualTo(204));

        var getResponse = await Request.GetAsync("api/groups");
        var getText = await getResponse.TextAsync();
        using var getDocument = System.Text.Json.JsonDocument.Parse(getText);
        var groups = getDocument.RootElement.EnumerateArray().ToList();

        Assert.That(groups.Any(g =>
            g.TryGetProperty("id", out var id) &&
            id.GetInt64() == groupId &&
            g.TryGetProperty("name", out var name) &&
            name.GetString() == updatedName), Is.True);
    }
    [Test]
    public async Task DeleteGroup_Removes_Existing_Group()
    {
        if (Request == null)
        {
            Assert.Fail("Greška u API kontekstu.");
            return;
        }

        var groupName = "PW-Test-Group-" + DateTime.UtcNow.Ticks;

        var postResponse = await Request.PostAsync("api/groups", new APIRequestContextOptions
        {
            DataObject = new
            {
                name = groupName,
                ownerUserId = 1,
                defaultValuta = "RSD"
            }
        });

        if (postResponse.Status != 201)
        {
            var postText = await postResponse.TextAsync();
            Assert.Fail($"POST failed: {postResponse.Status} - {postResponse.StatusText} - {postText}");
            return;
        }

        var postTextJson = await postResponse.TextAsync();
        using var postDocument = System.Text.Json.JsonDocument.Parse(postTextJson);

        if (!postDocument.RootElement.TryGetProperty("id", out var idProperty))
        {
            Assert.Fail("ID kreirane grupe nije pronađen.");
            return;
        }

        var groupId = idProperty.GetInt64();

        var deleteResponse = await Request.DeleteAsync($"api/groups/{groupId}");

        Assert.That(deleteResponse.Status, Is.EqualTo(204));

        var getResponse = await Request.GetAsync("api/groups");
        var getText = await getResponse.TextAsync();
        using var getDocument = System.Text.Json.JsonDocument.Parse(getText);
        var groups = getDocument.RootElement.EnumerateArray().ToList();

        Assert.That(groups.Any(g =>
            g.TryGetProperty("id", out var id) &&
            id.GetInt64() == groupId), Is.False);
    }

    //ShoppingListaItem-----------------------------------------------------------


    [Test]
    public async Task PostShoppingItem_Creates_New_Item()
    {
        if (Request == null)
        {
            Assert.Fail("Greška u API kontekstu.");
            return;
        }

        var itemName = "PW-Item-" + DateTime.UtcNow.Ticks;

        var response = await Request.PostAsync("api/shopping-items", new APIRequestContextOptions
        {
            DataObject = new
            {
                groupId = 1,
                trazioUserId = 1,
                naziv = itemName
            }
        });

        if (response.Status != 201)
        {
            var text = await response.TextAsync();
            Assert.Fail($"Code: {response.Status} - {response.StatusText} - {text}");
            return;
        }

        var jsonText = await response.TextAsync();
        using var document = System.Text.Json.JsonDocument.Parse(jsonText);

        Assert.That(document.RootElement.TryGetProperty("naziv", out var naziv), Is.True);
        Assert.That(naziv.GetString(), Is.EqualTo(itemName));
    }

    [Test]
    public async Task GetShoppingItems_Returns_Newly_Created_Item()
    {
        if (Request == null)
        {
            Assert.Fail("Greška u API kontekstu.");
            return;
        }

        var itemName = "PW-Item-" + DateTime.UtcNow.Ticks;

        var postResponse = await Request.PostAsync("api/shopping-items", new APIRequestContextOptions
        {
            DataObject = new
            {
                groupId = 1,
                trazioUserId = 1,
                naziv = itemName
            }
        });

        if (postResponse.Status != 201)
        {
            var postText = await postResponse.TextAsync();
            Assert.Fail($"POST failed: {postResponse.Status} - {postResponse.StatusText} - {postText}");
            return;
        }

        var getResponse = await Request.GetAsync("api/shopping-items");

        if (getResponse.Status != 200)
        {
            var getText = await getResponse.TextAsync();
            Assert.Fail($"GET failed: {getResponse.Status} - {getResponse.StatusText} - {getText}");
            return;
        }

        var text = await getResponse.TextAsync();
        using var document = System.Text.Json.JsonDocument.Parse(text);
        var items = document.RootElement.EnumerateArray().ToList();

        Assert.That(items.Any(i =>
            i.TryGetProperty("naziv", out var naziv) &&
            naziv.GetString() == itemName), Is.True);
    }

    [Test]
    public async Task PutShoppingItem_Updates_Existing_Item()
    {
        if (Request == null)
        {
            Assert.Fail("Greška u API kontekstu.");
            return;
        }

        var itemName = "PW-Item-" + DateTime.UtcNow.Ticks;

        var postResponse = await Request.PostAsync("api/shopping-items", new APIRequestContextOptions
        {
            DataObject = new
            {
                groupId = 1,
                trazioUserId = 1,
                naziv = itemName
            }
        });

        if (postResponse.Status != 201)
        {
            var postText = await postResponse.TextAsync();
            Assert.Fail($"POST failed: {postResponse.Status} - {postResponse.StatusText} - {postText}");
            return;
        }

        var postTextJson = await postResponse.TextAsync();
        using var postDocument = System.Text.Json.JsonDocument.Parse(postTextJson);

        if (!postDocument.RootElement.TryGetProperty("id", out var idProperty))
        {
            Assert.Fail("ID kreirane stavke nije pronađen.");
            return;
        }

        var itemId = idProperty.GetInt64();
        var updatedName = itemName + "-Updated";

        var putResponse = await Request.PutAsync($"api/shopping-items/{itemId}", new APIRequestContextOptions
        {
            DataObject = new
            {
                naziv = updatedName,
                nabavioUserId = 1
            }
        });

        Assert.That(putResponse.Status, Is.EqualTo(204));

        var getResponse = await Request.GetAsync($"api/shopping-items/{itemId}");

        if (getResponse.Status != 200)
        {
            var getText = await getResponse.TextAsync();
            Assert.Fail($"GET failed: {getResponse.Status} - {getResponse.StatusText} - {getText}");
            return;
        }

        var text = await getResponse.TextAsync();
        using var document = System.Text.Json.JsonDocument.Parse(text);

        Assert.That(document.RootElement.TryGetProperty("naziv", out var naziv), Is.True);
        Assert.That(naziv.GetString(), Is.EqualTo(updatedName));
    }
    [Test]
    public async Task DeleteShoppingItem_Removes_Existing_Item()
    {
        if (Request == null)
        {
            Assert.Fail("Greška u API kontekstu.");
            return;
        }

        var itemName = "PW-Item-" + DateTime.UtcNow.Ticks;

        var postResponse = await Request.PostAsync("api/shopping-items", new APIRequestContextOptions
        {
            DataObject = new
            {
                groupId = 1,
                trazioUserId = 1,
                naziv = itemName
            }
        });

        if (postResponse.Status != 201)
        {
            var postText = await postResponse.TextAsync();
            Assert.Fail($"POST failed: {postResponse.Status} - {postResponse.StatusText} - {postText}");
            return;
        }

        var postTextJson = await postResponse.TextAsync();
        using var postDocument = System.Text.Json.JsonDocument.Parse(postTextJson);

        if (!postDocument.RootElement.TryGetProperty("id", out var idProperty))
        {
            Assert.Fail("ID kreirane stavke nije pronađen.");
            return;
        }

        var itemId = idProperty.GetInt64();

        var deleteResponse = await Request.DeleteAsync($"api/shopping-items/{itemId}");

        Assert.That(deleteResponse.Status, Is.EqualTo(204));

        var getResponse = await Request.GetAsync($"api/shopping-items/{itemId}");

        Assert.That(getResponse.Status, Is.EqualTo(404));
    }

   // Racun-------------------------------------------------------------------------------------------------------

    [Test]
    public async Task PostRacun_Creates_New_Racun()
    {
        if (Request == null)
        {
            Assert.Fail("Greška u API kontekstu.");
            return;
        }

        var racunName = "PW-Racun-" + DateTime.UtcNow.Ticks;

        var response = await Request.PostAsync("api/racuni", new APIRequestContextOptions
        {
            DataObject = new
            {
                groupId = 1,
                naziv = racunName,
                iznos = 2500,
                creatorUserId = 1
            }
        });

        if (response.Status != 201)
        {
            var text = await response.TextAsync();
            Assert.Fail($"Code: {response.Status} - {response.StatusText} - {text}");
            return;
        }

        var jsonText = await response.TextAsync();
        using var document = System.Text.Json.JsonDocument.Parse(jsonText);

        Assert.That(document.RootElement.TryGetProperty("naziv", out var naziv), Is.True);
        Assert.That(naziv.GetString(), Is.EqualTo(racunName));
    }

    [Test]
    public async Task GetRacuni_Returns_Newly_Created_Racun()
    {
        if (Request == null)
        {
            Assert.Fail("Greška u API kontekstu.");
            return;
        }

        var racunName = "PW-Racun-" + DateTime.UtcNow.Ticks;

        var postResponse = await Request.PostAsync("api/racuni", new APIRequestContextOptions
        {
            DataObject = new
            {
                groupId = 1,
                naziv = racunName,
                iznos = 2500,
                creatorUserId = 1
            }
        });

        if (postResponse.Status != 201)
        {
            var postText = await postResponse.TextAsync();
            Assert.Fail($"POST failed: {postResponse.Status} - {postResponse.StatusText} - {postText}");
            return;
        }

        var getResponse = await Request.GetAsync("api/racuni");

        if (getResponse.Status != 200)
        {
            var getText = await getResponse.TextAsync();
            Assert.Fail($"GET failed: {getResponse.Status} - {getResponse.StatusText} - {getText}");
            return;
        }

        var text = await getResponse.TextAsync();
        using var document = System.Text.Json.JsonDocument.Parse(text);
        var racuni = document.RootElement.EnumerateArray().ToList();

        Assert.That(racuni.Any(r =>
            r.TryGetProperty("naziv", out var naziv) &&
            naziv.GetString() == racunName), Is.True);
    }
    [Test]
    public async Task PutRacun_Updates_Existing_Racun()
    {
        if (Request == null)
        {
            Assert.Fail("Greška u API kontekstu.");
            return;
        }

        var racunName = "PW-Racun-" + DateTime.UtcNow.Ticks;

        var postResponse = await Request.PostAsync("api/racuni", new APIRequestContextOptions
        {
            DataObject = new
            {
                groupId = 1,
                naziv = racunName,
                iznos = 2500,
                creatorUserId = 1
            }
        });

        if (postResponse.Status != 201)
        {
            var postText = await postResponse.TextAsync();
            Assert.Fail($"POST failed: {postResponse.Status} - {postResponse.StatusText} - {postText}");
            return;
        }

        var postTextJson = await postResponse.TextAsync();
        using var postDocument = System.Text.Json.JsonDocument.Parse(postTextJson);

        if (!postDocument.RootElement.TryGetProperty("id", out var idProperty))
        {
            Assert.Fail("ID kreiranog računa nije pronađen.");
            return;
        }

        var racunId = idProperty.GetInt64();
        var updatedName = racunName + "-Updated";

        var putResponse = await Request.PutAsync($"api/racuni/{racunId}", new APIRequestContextOptions
        {
            DataObject = new
            {
                groupId = 1,
                naziv = updatedName,
                iznos = 3200,
                creatorUserId = 1
            }
        });

        Assert.That(putResponse.Status, Is.EqualTo(204));

        var getResponse = await Request.GetAsync($"api/racuni/{racunId}");

        if (getResponse.Status != 200)
        {
            var getText = await getResponse.TextAsync();
            Assert.Fail($"GET failed: {getResponse.Status} - {getResponse.StatusText} - {getText}");
            return;
        }

        var text = await getResponse.TextAsync();
        using var document = System.Text.Json.JsonDocument.Parse(text);

        Assert.That(document.RootElement.TryGetProperty("naziv", out var naziv), Is.True);
        Assert.That(naziv.GetString(), Is.EqualTo(updatedName));
    }
    [Test]
    public async Task DeleteRacun_Removes_Existing_Racun()
    {
        if (Request == null)
        {
            Assert.Fail("Greška u API kontekstu.");
            return;
        }

        var racunName = "PW-Racun-" + DateTime.UtcNow.Ticks;

        var postResponse = await Request.PostAsync("api/racuni", new APIRequestContextOptions
        {
            DataObject = new
            {
                groupId = 1,
                naziv = racunName,
                iznos = 2500,
                creatorUserId = 1
            }
        });

        if (postResponse.Status != 201)
        {
            var postText = await postResponse.TextAsync();
            Assert.Fail($"POST failed: {postResponse.Status} - {postResponse.StatusText} - {postText}");
            return;
        }

        var postTextJson = await postResponse.TextAsync();
        using var postDocument = System.Text.Json.JsonDocument.Parse(postTextJson);

        if (!postDocument.RootElement.TryGetProperty("id", out var idProperty))
        {
            Assert.Fail("ID kreiranog računa nije pronađen.");
            return;
        }

        var racunId = idProperty.GetInt64();

        var deleteResponse = await Request.DeleteAsync($"api/racuni/{racunId}");

        Assert.That(deleteResponse.Status, Is.EqualTo(204));

        var getResponse = await Request.GetAsync($"api/racuni/{racunId}");

        Assert.That(getResponse.Status, Is.EqualTo(404));
    }

    //Dogadjaji-----------------------------------------------------------------------

    [Test]
    public async Task PostDogadjaj_Creates_New_Dogadjaj()
    {
        if (Request == null)
        {
            Assert.Fail("Greška u API kontekstu.");
            return;
        }

        var dogadjajName = "PW-Dogadjaj-" + DateTime.UtcNow.Ticks;

        var response = await Request.PostAsync("api/dogadjaji", new APIRequestContextOptions
        {
            DataObject = new
            {
                grupaId = 1,
                creatorId = 1,
                naziv = dogadjajName,
                opis = "Playwright test opis",
                lokacija = "Novi Sad",
                vremeDogadjaja = DateTime.UtcNow.AddDays(1).ToString("o")
            }
        });

        if (response.Status != 201)
        {
            var text = await response.TextAsync();
            Assert.Fail($"Code: {response.Status} - {response.StatusText} - {text}");
            return;
        }

        var jsonText = await response.TextAsync();
        using var document = System.Text.Json.JsonDocument.Parse(jsonText);

        Assert.That(document.RootElement.TryGetProperty("naziv", out var naziv), Is.True);
        Assert.That(naziv.GetString(), Is.EqualTo(dogadjajName));
    }

    [Test]
    public async Task GetDogadjaji_Returns_Newly_Created_Dogadjaj()
    {
        if (Request == null)
        {
            Assert.Fail("Greška u API kontekstu.");
            return;
        }

        var dogadjajName = "PW-Dogadjaj-" + DateTime.UtcNow.Ticks;

        var postResponse = await Request.PostAsync("api/dogadjaji", new APIRequestContextOptions
        {
            DataObject = new
            {
                grupaId = 1,
                creatorId = 1,
                naziv = dogadjajName,
                opis = "Playwright test opis",
                lokacija = "Novi Sad",
                vremeDogadjaja = DateTime.UtcNow.AddDays(1).ToString("o")
            }
        });

        if (postResponse.Status != 201)
        {
            var postText = await postResponse.TextAsync();
            Assert.Fail($"POST failed: {postResponse.Status} - {postResponse.StatusText} - {postText}");
            return;
        }

        var getResponse = await Request.GetAsync("api/dogadjaji");

        if (getResponse.Status != 200)
        {
            var getText = await getResponse.TextAsync();
            Assert.Fail($"GET failed: {getResponse.Status} - {getResponse.StatusText} - {getText}");
            return;
        }

        var text = await getResponse.TextAsync();
        using var document = System.Text.Json.JsonDocument.Parse(text);
        var dogadjaji = document.RootElement.EnumerateArray().ToList();

        Assert.That(dogadjaji.Any(d =>
            d.TryGetProperty("naziv", out var naziv) &&
            naziv.GetString() == dogadjajName), Is.True);
    }

    [Test]
    public async Task PutDogadjaj_Updates_Existing_Dogadjaj()
    {
        if (Request == null)
        {
            Assert.Fail("Greška u API kontekstu.");
            return;
        }

        var dogadjajName = "PW-Dogadjaj-" + DateTime.UtcNow.Ticks;

        var postResponse = await Request.PostAsync("api/dogadjaji", new APIRequestContextOptions
        {
            DataObject = new
            {
                grupaId = 1,
                creatorId = 1,
                naziv = dogadjajName,
                opis = "Playwright test opis",
                lokacija = "Novi Sad",
                vremeDogadjaja = DateTime.UtcNow.AddDays(1).ToString("o")
            }
        });

        if (postResponse.Status != 201)
        {
            var postText = await postResponse.TextAsync();
            Assert.Fail($"POST failed: {postResponse.Status} - {postResponse.StatusText} - {postText}");
            return;
        }

        var postTextJson = await postResponse.TextAsync();
        using var postDocument = System.Text.Json.JsonDocument.Parse(postTextJson);

        if (!postDocument.RootElement.TryGetProperty("id", out var idProperty))
        {
            Assert.Fail("ID kreiranog događaja nije pronađen.");
            return;
        }

        var dogadjajId = idProperty.GetInt64();
        var updatedName = dogadjajName + "-Updated";

        var putResponse = await Request.PutAsync($"api/dogadjaji/{dogadjajId}", new APIRequestContextOptions
        {
            DataObject = new
            {
                grupaId = 1,
                creatorId = 1,
                naziv = updatedName,
                opis = "Izmenjen opis",
                lokacija = "Beograd",
                vremeDogadjaja = DateTime.UtcNow.AddDays(2).ToString("o")
            }
        });

        Assert.That(putResponse.Status, Is.EqualTo(204));

        var getResponse = await Request.GetAsync($"api/dogadjaji/{dogadjajId}");

        if (getResponse.Status != 200)
        {
            var getText = await getResponse.TextAsync();
            Assert.Fail($"GET failed: {getResponse.Status} - {getResponse.StatusText} - {getText}");
            return;
        }

        var text = await getResponse.TextAsync();
        using var document = System.Text.Json.JsonDocument.Parse(text);

        Assert.That(document.RootElement.TryGetProperty("naziv", out var naziv), Is.True);
        Assert.That(naziv.GetString(), Is.EqualTo(updatedName));
    }

    [Test]
    public async Task DeleteDogadjaj_Removes_Existing_Dogadjaj()
    {
        if (Request == null)
        {
            Assert.Fail("Greška u API kontekstu.");
            return;
        }

        var dogadjajName = "PW-Dogadjaj-" + DateTime.UtcNow.Ticks;

        var postResponse = await Request.PostAsync("api/dogadjaji", new APIRequestContextOptions
        {
            DataObject = new
            {
                grupaId = 1,
                creatorId = 1,
                naziv = dogadjajName,
                opis = "Playwright test opis",
                lokacija = "Novi Sad",
                vremeDogadjaja = DateTime.UtcNow.AddDays(1).ToString("o")
            }
        });

        if (postResponse.Status != 201)
        {
            var postText = await postResponse.TextAsync();
            Assert.Fail($"POST failed: {postResponse.Status} - {postResponse.StatusText} - {postText}");
            return;
        }

        var postTextJson = await postResponse.TextAsync();
        using var postDocument = System.Text.Json.JsonDocument.Parse(postTextJson);

        if (!postDocument.RootElement.TryGetProperty("id", out var idProperty))
        {
            Assert.Fail("ID kreiranog događaja nije pronađen.");
            return;
        }

        var dogadjajId = idProperty.GetInt64();

        var deleteResponse = await Request.DeleteAsync($"api/dogadjaji/{dogadjajId}");

        Assert.That(deleteResponse.Status, Is.EqualTo(204));

        var getResponse = await Request.GetAsync($"api/dogadjaji/{dogadjajId}");

        Assert.That(getResponse.Status, Is.EqualTo(404));
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