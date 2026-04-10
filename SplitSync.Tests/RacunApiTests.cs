using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitSync.Controllers;
using SplitSync.Data;
using SplitSync.Entities;

namespace SplitSync.Tests;

[TestFixture]
public class RacunApiTests
{
    private AppDbContext _context = null!;
    private RacuniApiController _controller = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _controller = new RacuniApiController(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }
    //GETALL--------------------------------------------------------------------
    [Test]
    public async Task GetAll_Returns_Ok_Result_When_Racuni_Exist()
    {
        _context.Racuns.Add(new Racun
        {
            GroupId = 1,
            Naziv = "Racun 1",
            Iznos = 1000,
            CreatorUserId = 1,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var result = await _controller.GetAll();

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetAll_Returns_Empty_List_When_No_Racuni_Exist()
    {
        var result = await _controller.GetAll();

        var okResult = result.Result as OkObjectResult;
        var racuni = okResult?.Value as IEnumerable<Racun>;

        Assert.That(okResult, Is.Not.Null);
        Assert.That(racuni, Is.Empty);
    }

    [Test]
    public async Task GetAll_Returns_All_Racuni_When_Multiple_Racuni_Exist()
    {
        _context.Racuns.Add(new Racun
        {
            GroupId = 1,
            Naziv = "Racun 1",
            Iznos = 1000,
            CreatorUserId = 1,
            CreatedAtUtc = DateTime.UtcNow
        });

        _context.Racuns.Add(new Racun
        {
            GroupId = 2,
            Naziv = "Racun 2",
            Iznos = 2000,
            CreatorUserId = 2,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var result = await _controller.GetAll();

        var okResult = result.Result as OkObjectResult;
        var racuni = (okResult?.Value as IEnumerable<Racun>)?.ToList();

        Assert.That(okResult, Is.Not.Null);
        Assert.That(racuni, Is.Not.Null);
        Assert.That(racuni!.Count, Is.EqualTo(2));
    }

    //GETID------------------------------------------------------------------------
    [Test]
    public async Task GetById_Returns_Ok_Result_When_Racun_Exists()
    {
        var racun = new Racun
        {
            GroupId = 1,
            Naziv = "Racun 1",
            Iznos = 1000,
            CreatorUserId = 1,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Racuns.Add(racun);
        await _context.SaveChangesAsync();

        var result = await _controller.GetById(racun.Id);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetById_Returns_NotFound_When_Racun_Does_Not_Exist()
    {
        var result = await _controller.GetById(999);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task GetById_Returns_Correct_Racun_When_Racun_Exists()
    {
        var racun = new Racun
        {
            GroupId = 1,
            Naziv = "Racun 1",
            Iznos = 1000,
            CreatorUserId = 1,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Racuns.Add(racun);
        await _context.SaveChangesAsync();

        var result = await _controller.GetById(racun.Id);

        var okResult = result.Result as OkObjectResult;
        var returnedRacun = okResult?.Value as Racun;

        Assert.That(okResult, Is.Not.Null);
        Assert.That(returnedRacun, Is.Not.Null);
        Assert.That(returnedRacun!.Id, Is.EqualTo(racun.Id));
        Assert.That(returnedRacun.Naziv, Is.EqualTo("Racun 1"));
    }
    //POST---------------------------------------------------------------------
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

        _context.Groups.Add(new Group
        {
            Name = "Test grupa",
            OwnerUserId = 1,
            CreatedAtUtc = DateTime.UtcNow,
            DefaultValuta = "RSD"
        });

        await _context.SaveChangesAsync();

        var user = _context.Users.First();
        var group = _context.Groups.First();

        var request = new CreateRacunRequest
        {
            GroupId = group.Id,
            Naziv = "Racun test",
            Iznos = 2500,
            CreatorUserId = user.Id
        };

        var result = await _controller.Create(request);

        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        Assert.That(_context.Racuns.Count(), Is.EqualTo(1));
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

        var request = new CreateRacunRequest
        {
            GroupId = 999,
            Naziv = "Racun test",
            Iznos = 2500,
            CreatorUserId = user.Id
        };

        var result = await _controller.Create(request);

        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Create_Returns_BadRequest_When_Creator_Does_Not_Exist()
    {
        _context.Groups.Add(new Group
        {
            Name = "Test grupa",
            OwnerUserId = 1,
            CreatedAtUtc = DateTime.UtcNow,
            DefaultValuta = "RSD"
        });

        await _context.SaveChangesAsync();

        var group = _context.Groups.First();

        var request = new CreateRacunRequest
        {
            GroupId = group.Id,
            Naziv = "Racun test",
            Iznos = 2500,
            CreatorUserId = 999
        };

        var result = await _controller.Create(request);

        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    //PUT---------------------------------------------------------------------

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

        var racun = new Racun
        {
            GroupId = group.Id,
            Naziv = "Stari racun",
            Iznos = 1000,
            CreatorUserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Racuns.Add(racun);
        await _context.SaveChangesAsync();

        var request = new UpdateRacunRequest
        {
            GroupId = group.Id,
            Naziv = "Novi racun",
            Iznos = 2000,
            CreatorUserId = user.Id
        };

        var result = await _controller.Update(racun.Id, request);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        Assert.That(racun.Naziv, Is.EqualTo("Novi racun"));
        Assert.That(racun.Iznos, Is.EqualTo(2000));
    }

    [Test]
    public async Task Update_Returns_NotFound_When_Racun_Does_Not_Exist()
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

        var request = new UpdateRacunRequest
        {
            GroupId = group.Id,
            Naziv = "Novi racun",
            Iznos = 2000,
            CreatorUserId = user.Id
        };

        var result = await _controller.Update(999, request);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Update_Returns_BadRequest_When_Group_Does_Not_Exist()
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

        var racun = new Racun
        {
            GroupId = 1,
            Naziv = "Stari racun",
            Iznos = 1000,
            CreatorUserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Racuns.Add(racun);
        await _context.SaveChangesAsync();

        var request = new UpdateRacunRequest
        {
            GroupId = 999,
            Naziv = "Novi racun",
            Iznos = 2000,
            CreatorUserId = user.Id
        };

        var result = await _controller.Update(racun.Id, request);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
    //DELETE---------------------------------------------------------------------

    [Test]
    public async Task Delete_Returns_NoContent_When_Racun_Exists()
    {
        var racun = new Racun
        {
            GroupId = 1,
            Naziv = "Racun test",
            Iznos = 2500,
            CreatorUserId = 1,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Racuns.Add(racun);
        await _context.SaveChangesAsync();

        var result = await _controller.Delete(racun.Id);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task Delete_Returns_NotFound_When_Racun_Does_Not_Exist()
    {
        var result = await _controller.Delete(999);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Delete_Removes_Racun_From_Database_When_Racun_Exists()
    {
        var racun = new Racun
        {
            GroupId = 1,
            Naziv = "Racun test",
            Iznos = 2500,
            CreatorUserId = 1,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Racuns.Add(racun);
        await _context.SaveChangesAsync();

        await _controller.Delete(racun.Id);

        var deletedRacun = await _context.Racuns.FindAsync(racun.Id);

        Assert.That(deletedRacun, Is.Null);
    }




}