using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitSync.Controllers;
using SplitSync.Data;
using SplitSync.Entities;

namespace SplitSync.Tests;

[TestFixture]
public class DogadjajApiTests
{
    private AppDbContext _context = null!;
    private DogadjajiApiController _controller = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _controller = new DogadjajiApiController(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }
    //GETALL-----------------------------------------------------
    [Test]
    public async Task GetAll_Returns_Ok_Result_When_Dogadjaji_Exist()
    {
        _context.Dogadjaji.Add(new Dogadjaj
        {
            GrupaId = 1,
            CreatorId = 1,
            Naziv = "Rodjendan",
            Opis = "Proslava",
            Lokacija = "Novi Sad",
            VremeDogadjaja = DateTime.UtcNow.AddDays(1),
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var result = await _controller.GetAll();

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetAll_Returns_Empty_List_When_No_Dogadjaji_Exist()
    {
        var result = await _controller.GetAll();

        var okResult = result.Result as OkObjectResult;
        var dogadjaji = okResult?.Value as IEnumerable<Dogadjaj>;

        Assert.That(okResult, Is.Not.Null);
        Assert.That(dogadjaji, Is.Empty);
    }

    [Test]
    public async Task GetAll_Returns_All_Dogadjaji_When_Multiple_Dogadjaji_Exist()
    {
        _context.Dogadjaji.Add(new Dogadjaj
        {
            GrupaId = 1,
            CreatorId = 1,
            Naziv = "Rodjendan",
            Opis = "Proslava",
            Lokacija = "Novi Sad",
            VremeDogadjaja = DateTime.UtcNow.AddDays(1),
            CreatedAtUtc = DateTime.UtcNow
        });

        _context.Dogadjaji.Add(new Dogadjaj
        {
            GrupaId = 2,
            CreatorId = 2,
            Naziv = "Izlet",
            Opis = "Planina",
            Lokacija = "Zlatibor",
            VremeDogadjaja = DateTime.UtcNow.AddDays(2),
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var result = await _controller.GetAll();

        var okResult = result.Result as OkObjectResult;
        var dogadjaji = (okResult?.Value as IEnumerable<Dogadjaj>)?.ToList();

        Assert.That(okResult, Is.Not.Null);
        Assert.That(dogadjaji, Is.Not.Null);
        Assert.That(dogadjaji!.Count, Is.EqualTo(2));
    }
    //GETBYID-----------------------------------------------------
    [Test]
    public async Task GetById_Returns_Ok_Result_When_Dogadjaj_Exists()
    {
        var dogadjaj = new Dogadjaj
        {
            GrupaId = 1,
            CreatorId = 1,
            Naziv = "Rodjendan",
            Opis = "Proslava",
            Lokacija = "Novi Sad",
            VremeDogadjaja = DateTime.UtcNow.AddDays(1),
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Dogadjaji.Add(dogadjaj);
        await _context.SaveChangesAsync();

        var result = await _controller.GetById(dogadjaj.Id);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetById_Returns_NotFound_When_Dogadjaj_Does_Not_Exist()
    {
        var result = await _controller.GetById(999);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task GetById_Returns_Correct_Dogadjaj_When_Dogadjaj_Exists()
    {
        var dogadjaj = new Dogadjaj
        {
            GrupaId = 1,
            CreatorId = 1,
            Naziv = "Rodjendan",
            Opis = "Proslava",
            Lokacija = "Novi Sad",
            VremeDogadjaja = DateTime.UtcNow.AddDays(1),
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Dogadjaji.Add(dogadjaj);
        await _context.SaveChangesAsync();

        var result = await _controller.GetById(dogadjaj.Id);

        var okResult = result.Result as OkObjectResult;
        var returnedDogadjaj = okResult?.Value as Dogadjaj;

        Assert.That(okResult, Is.Not.Null);
        Assert.That(returnedDogadjaj, Is.Not.Null);
        Assert.That(returnedDogadjaj!.Id, Is.EqualTo(dogadjaj.Id));
        Assert.That(returnedDogadjaj.Naziv, Is.EqualTo("Rodjendan"));
    }
    //POST-----------------------------------------------------
    [Test]
    public async Task Create_Returns_CreatedAtAction_When_Data_Is_Valid()
    {
        _context.Users.Add(new User
        {
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = "hash",
            IsEmailVerified = true,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var user = _context.Users.First();

        _context.Groups.Add(new Group
        {
            Name = "Test grupa",
            OwnerUserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow,
            DefaultValuta = "RSD"
        });

        await _context.SaveChangesAsync();

        var group = _context.Groups.First();

        var request = new CreateDogadjajRequest
        {
            GrupaId = group.Id,
            CreatorId = user.Id,
            Naziv = "Rodjendan",
            Opis = "Proslava",
            Lokacija = "Novi Sad",
            VremeDogadjaja = DateTime.UtcNow.AddDays(1)
        };

        var result = await _controller.Create(request);

        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        Assert.That(_context.Dogadjaji.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task Create_Returns_BadRequest_When_Name_Is_Empty()
    {
        _context.Users.Add(new User
        {
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = "hash",
            IsEmailVerified = true,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var user = _context.Users.First();

        _context.Groups.Add(new Group
        {
            Name = "Test grupa",
            OwnerUserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow,
            DefaultValuta = "RSD"
        });

        await _context.SaveChangesAsync();

        var group = _context.Groups.First();

        var request = new CreateDogadjajRequest
        {
            GrupaId = group.Id,
            CreatorId = user.Id,
            Naziv = "",
            Opis = "Proslava",
            Lokacija = "Novi Sad",
            VremeDogadjaja = DateTime.UtcNow.AddDays(1)
        };

        var result = await _controller.Create(request);

        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Create_Returns_BadRequest_When_Group_Does_Not_Exist()
    {
        _context.Users.Add(new User
        {
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = "hash",
            IsEmailVerified = true,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var user = _context.Users.First();

        var request = new CreateDogadjajRequest
        {
            GrupaId = 999,
            CreatorId = user.Id,
            Naziv = "Rodjendan",
            Opis = "Proslava",
            Lokacija = "Novi Sad",
            VremeDogadjaja = DateTime.UtcNow.AddDays(1)
        };

        var result = await _controller.Create(request);

        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    //PUT-----------------------------------------------------
    [Test]
    public async Task Update_Returns_NoContent_When_Data_Is_Valid()
    {
        _context.Users.Add(new User
        {
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = "hash",
            IsEmailVerified = true,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var user = _context.Users.First();

        _context.Groups.Add(new Group
        {
            Name = "Test grupa",
            OwnerUserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow,
            DefaultValuta = "RSD"
        });

        await _context.SaveChangesAsync();

        var group = _context.Groups.First();

        var dogadjaj = new Dogadjaj
        {
            GrupaId = group.Id,
            CreatorId = user.Id,
            Naziv = "Stari dogadjaj",
            Opis = "Stari opis",
            Lokacija = "Novi Sad",
            VremeDogadjaja = DateTime.UtcNow.AddDays(1),
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Dogadjaji.Add(dogadjaj);
        await _context.SaveChangesAsync();

        var request = new UpdateDogadjajRequest
        {
            GrupaId = group.Id,
            CreatorId = user.Id,
            Naziv = "Novi dogadjaj",
            Opis = "Novi opis",
            Lokacija = "Beograd",
            VremeDogadjaja = DateTime.UtcNow.AddDays(2)
        };

        var result = await _controller.Update(dogadjaj.Id, request);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        Assert.That(dogadjaj.Naziv, Is.EqualTo("Novi dogadjaj"));
        Assert.That(dogadjaj.Opis, Is.EqualTo("Novi opis"));
        Assert.That(dogadjaj.Lokacija, Is.EqualTo("Beograd"));
    }

    [Test]
    public async Task Update_Returns_NotFound_When_Dogadjaj_Does_Not_Exist()
    {
        _context.Users.Add(new User
        {
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = "hash",
            IsEmailVerified = true,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var user = _context.Users.First();

        _context.Groups.Add(new Group
        {
            Name = "Test grupa",
            OwnerUserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow,
            DefaultValuta = "RSD"
        });

        await _context.SaveChangesAsync();

        var group = _context.Groups.First();

        var request = new UpdateDogadjajRequest
        {
            GrupaId = group.Id,
            CreatorId = user.Id,
            Naziv = "Novi dogadjaj",
            Opis = "Novi opis",
            Lokacija = "Beograd",
            VremeDogadjaja = DateTime.UtcNow.AddDays(2)
        };

        var result = await _controller.Update(999, request);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Update_Returns_BadRequest_When_Name_Is_Empty()
    {
        _context.Users.Add(new User
        {
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = "hash",
            IsEmailVerified = true,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var user = _context.Users.First();

        _context.Groups.Add(new Group
        {
            Name = "Test grupa",
            OwnerUserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow,
            DefaultValuta = "RSD"
        });

        await _context.SaveChangesAsync();

        var group = _context.Groups.First();

        var dogadjaj = new Dogadjaj
        {
            GrupaId = group.Id,
            CreatorId = user.Id,
            Naziv = "Stari dogadjaj",
            Opis = "Stari opis",
            Lokacija = "Novi Sad",
            VremeDogadjaja = DateTime.UtcNow.AddDays(1),
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Dogadjaji.Add(dogadjaj);
        await _context.SaveChangesAsync();

        var request = new UpdateDogadjajRequest
        {
            GrupaId = group.Id,
            CreatorId = user.Id,
            Naziv = "",
            Opis = "Novi opis",
            Lokacija = "Beograd",
            VremeDogadjaja = DateTime.UtcNow.AddDays(2)
        };

        var result = await _controller.Update(dogadjaj.Id, request);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    //DELETE-----------------------------------------------------
    [Test]
    public async Task Delete_Returns_NoContent_When_Dogadjaj_Exists()
    {
        var dogadjaj = new Dogadjaj
        {
            GrupaId = 1,
            CreatorId = 1,
            Naziv = "Rodjendan",
            Opis = "Proslava",
            Lokacija = "Novi Sad",
            VremeDogadjaja = DateTime.UtcNow.AddDays(1),
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Dogadjaji.Add(dogadjaj);
        await _context.SaveChangesAsync();

        var result = await _controller.Delete(dogadjaj.Id);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task Delete_Returns_NotFound_When_Dogadjaj_Does_Not_Exist()
    {
        var result = await _controller.Delete(999);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Delete_Removes_Dogadjaj_From_Database_When_Dogadjaj_Exists()
    {
        var dogadjaj = new Dogadjaj
        {
            GrupaId = 1,
            CreatorId = 1,
            Naziv = "Rodjendan",
            Opis = "Proslava",
            Lokacija = "Novi Sad",
            VremeDogadjaja = DateTime.UtcNow.AddDays(1),
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Dogadjaji.Add(dogadjaj);
        await _context.SaveChangesAsync();

        await _controller.Delete(dogadjaj.Id);

        var deletedDogadjaj = await _context.Dogadjaji.FindAsync(dogadjaj.Id);

        Assert.That(deletedDogadjaj, Is.Null);
    }

}