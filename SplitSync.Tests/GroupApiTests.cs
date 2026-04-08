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
    public async Task GetAll_Returns_Ok_Result()
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

}