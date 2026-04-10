using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitSync.Controllers;
using SplitSync.Data;
using SplitSync.Entities;

namespace SplitSync.Tests;

[TestFixture]
public class GroupApiTests
{
    private AppDbContext _context = null!;
    private GroupsApiController _controller = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _controller = new GroupsApiController(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }


    //GETALL----------------------------------------------------------------------GETALL
    [Test]
    public async Task GetAll_Returns_Ok_Result_When_Groups_Exist()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "GetAll_Returns_Ok_Result")
            .Options;//koristimo bazu u memoriji umesto postgresql bazu 

        using var context = new AppDbContext(options);

        context.Groups.Add(new Group
        {
            Name = "Test grupa",
            OwnerUserId = 1,
            CreatedAtUtc = DateTime.UtcNow,
            DefaultValuta = "RSD"
        });

        await context.SaveChangesAsync();//sacuvava u test bazu

        var _controller = new GroupsApiController(context);//prosledjujes context 

        var result = await _controller.GetAll();

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());//proveravaš da je kontroler vratio Ok(...)
    }
    [Test]
    public async Task GetAll_Returns_Empty_List_When_No_Groups_Exist()
    {
        var result = await _controller.GetAll();

        var okResult = result.Result as OkObjectResult;
        var groups = okResult?.Value as IEnumerable<Group>;

        Assert.That(okResult, Is.Not.Null);
        Assert.That(groups, Is.Empty);
    }
    [Test]
    public async Task GetAll_Returns_All_Groups_When_Multiple_Groups_Exist()//ako u bazi ima vise grupe da li getall vraca sve te grupe
    {
        _context.Groups.Add(new Group
        {
            Name = "Grupa 1",
            OwnerUserId = 1,
            CreatedAtUtc = DateTime.UtcNow,
            DefaultValuta = "RSD"
        });

        _context.Groups.Add(new Group
        {
            Name = "Grupa 2",
            OwnerUserId = 2,
            CreatedAtUtc = DateTime.UtcNow,
            DefaultValuta = "EUR"
        });

        await _context.SaveChangesAsync();

        var result = await _controller.GetAll();

        var okResult = result.Result as OkObjectResult;
        var groups = (okResult?.Value as IEnumerable<Group>)?.ToList();

        Assert.That(okResult, Is.Not.Null);
        Assert.That(groups, Is.Not.Null);
        Assert.That(groups!.Count, Is.EqualTo(2));
    }


    //GETID----------------------------------------------------------------------GETID

    [Test]
    public async Task GetById_Returns_Correct_Group_When_Group_Exists()
    {
        var group = new Group
        {
            Name = "Test grupa",
            OwnerUserId = 1,
            CreatedAtUtc = DateTime.UtcNow,
            DefaultValuta = "RSD"
        };

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();//dodaj grupu u bazu

        var result = await _controller.GetById(group.Id);//trazimo grupu po id

        var okResult = result.Result as OkObjectResult;
        var returnedGroup = okResult?.Value as Group;

        Assert.That(okResult, Is.Not.Null);
        Assert.That(returnedGroup, Is.Not.Null);
        Assert.That(returnedGroup!.Id, Is.EqualTo(group.Id));
        Assert.That(returnedGroup.Name, Is.EqualTo("Test grupa"));
    }
    [Test]
    public async Task GetById_Returns_Ok_Result_When_Group_Exists()
    {
        var group = new Group
        {
            Name = "Test grupa",
            OwnerUserId = 1,
            CreatedAtUtc = DateTime.UtcNow,
            DefaultValuta = "RSD"
        };

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        var result = await _controller.GetById(group.Id);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetById_Returns_NotFound_When_Group_Does_Not_Exist()
    {
        var result = await _controller.GetById(999);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    //POST----------------------------------------------------------------------POST


    [Test]
    public async Task Create_Returns_CreatedAtAction_When_Data_Is_Valid()//
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

        var owner = _context.Users.First();

        var request = new CreateGroupRequest
        {
            Name = "Nova grupa",
            OwnerUserId = owner.Id,
            DefaultValuta = "RSD"
        };

        var result = await _controller.Create(request);

        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        Assert.That(_context.Groups.Count(), Is.EqualTo(1));
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

        var owner = _context.Users.First();

        var request = new CreateGroupRequest
        {
            Name = "",
            OwnerUserId = owner.Id,
            DefaultValuta = "RSD"
        };

        var result = await _controller.Create(request);

        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Create_Returns_BadRequest_When_Owner_Does_Not_Exist()
    {
        var request = new CreateGroupRequest
        {
            Name = "Nova grupa",
            OwnerUserId = 999,
            DefaultValuta = "RSD"
        };

        var result = await _controller.Create(request);

        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    //PUT------------------------------------------------------------------------PUT
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
        });//pravi novog usera

        var group = new Group
        {
            Name = "Stara grupa",
            OwnerUserId = 1,
            CreatedAtUtc = DateTime.UtcNow,
            DefaultValuta = "RSD"
        };//pravi novu grupu

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();//sacuvava u bazu

        var owner = _context.Users.First();

        var request = new UpdateGroupRequest
        {
            Name = "Nova grupa",
            OwnerUserId = owner.Id,
            DefaultValuta = "EUR"
        };//salje request da apdejtuje 

        var result = await _controller.Update(group.Id, request);//pozoves update

        Assert.That(result, Is.InstanceOf<NoContentResult>());//uspesno sam izmenio ali ne vraca nista
        Assert.That(group.Name, Is.EqualTo("Nova grupa"));//da li je ime nove grupe "nova grupa"
        Assert.That(group.DefaultValuta, Is.EqualTo("EUR"));
    }

    [Test]
    public async Task Update_Returns_NotFound_When_Group_Does_Not_Exist()
    {
        _context.Users.Add(new User
        {
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = "hash",
            IsEmailVerified = true,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();//sacuvava u bazu

        var owner = _context.Users.First();

        var request = new UpdateGroupRequest
        {
            Name = "Nova grupa",
            OwnerUserId = owner.Id,
            DefaultValuta = "EUR"
        };//salje request da apdejtuje 

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

        var group = new Group
        {
            Name = "Stara grupa",
            OwnerUserId = 1,
            CreatedAtUtc = DateTime.UtcNow,
            DefaultValuta = "RSD"
        };

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        var owner = _context.Users.First();

        var request = new UpdateGroupRequest
        {
            Name = "",
            OwnerUserId = owner.Id,
            DefaultValuta = "EUR"
        };

        var result = await _controller.Update(group.Id, request);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Delete_Returns_NoContent_When_Group_Exists()
    {
        var group = new Group
        {
            Name = "Test grupa",
            OwnerUserId = 1,
            CreatedAtUtc = DateTime.UtcNow,
            DefaultValuta = "RSD"
        };

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        var result = await _controller.Delete(group.Id);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task Delete_Returns_NotFound_When_Group_Does_Not_Exist()
    {
        var result = await _controller.Delete(999);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Delete_Removes_Group_From_Database_When_Group_Exists()
    {
        var group = new Group
        {
            Name = "Test grupa",
            OwnerUserId = 1,
            CreatedAtUtc = DateTime.UtcNow,
            DefaultValuta = "RSD"
        };

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        await _controller.Delete(group.Id);
        var deletedGroup = await _context.Groups.FindAsync(group.Id);

        Assert.That(deletedGroup, Is.Null);
    }



}