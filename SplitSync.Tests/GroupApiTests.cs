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

        var controller = new GroupsApiController(context);//prosledjujes context 

        var result = await controller.GetAll();

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




}