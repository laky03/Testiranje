using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitSync.Controllers;
using SplitSync.Data;
using SplitSync.Entities;

namespace SplitSync.Tests;

[TestFixture]
public class ShoppingListaItemApiTests
{
    private AppDbContext _context = null!;
    private ShoppingItemsApiController _controller = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _controller = new ShoppingItemsApiController(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }


    //GETALL---------------------------------------------------------------
    [Test]
    public async Task GetAll_Returns_Ok_Result_When_Items_Exist()
    {
        _context.ShoppingListaItems.Add(new ShoppingListaItem
        {
            GroupId = 1,
            TrazioUserId = 1,
            Naziv = "Mleko",
            TrazenoUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var result = await _controller.GetAll();

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetAll_Returns_Empty_List_When_No_Items_Exist()
    {
        var result = await _controller.GetAll();

        var okResult = result.Result as OkObjectResult;
        var items = okResult?.Value as IEnumerable<ShoppingListaItem>;

        Assert.That(okResult, Is.Not.Null);
        Assert.That(items, Is.Empty);
    }
    [Test]
    public async Task GetAll_Returns_All_Items_When_Multiple_Items_Exist()
    {
        _context.ShoppingListaItems.Add(new ShoppingListaItem
        {
            GroupId = 1,
            TrazioUserId = 1,
            Naziv = "Mleko",
            TrazenoUtc = DateTime.UtcNow
        });

        _context.ShoppingListaItems.Add(new ShoppingListaItem
        {
            GroupId = 1,
            TrazioUserId = 2,
            Naziv = "Hleb",
            TrazenoUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var result = await _controller.GetAll();

        var okResult = result.Result as OkObjectResult;
        var items = (okResult?.Value as IEnumerable<ShoppingListaItem>)?.ToList();

        Assert.That(okResult, Is.Not.Null);
        Assert.That(items, Is.Not.Null);
        Assert.That(items!.Count, Is.EqualTo(2));//proveravas da li je vratio dve liste 2 itema
    }

    //GETID----------------------------------------------------------------------------------------------
    [Test]
    public async Task GetById_Returns_Ok_Result_When_Item_Exists()
    {
        var item = new ShoppingListaItem
        {
            GroupId = 1,
            TrazioUserId = 1,
            Naziv = "Mleko",
            TrazenoUtc = DateTime.UtcNow
        };

        _context.ShoppingListaItems.Add(item);
        await _context.SaveChangesAsync();

        var result = await _controller.GetById(item.Id);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetById_Returns_NotFound_When_Item_Does_Not_Exist()
    {
        var result = await _controller.GetById(999);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task GetById_Returns_Correct_Item_When_Item_Exists()
    {
        var item = new ShoppingListaItem
        {
            GroupId = 1,
            TrazioUserId = 1,
            Naziv = "Mleko",
            TrazenoUtc = DateTime.UtcNow
        };

        _context.ShoppingListaItems.Add(item);
        await _context.SaveChangesAsync();

        var result = await _controller.GetById(item.Id);

        var okResult = result.Result as OkObjectResult;
        var returnedItem = okResult?.Value as ShoppingListaItem;

        Assert.That(okResult, Is.Not.Null);
        Assert.That(returnedItem, Is.Not.Null);
        Assert.That(returnedItem!.Id, Is.EqualTo(item.Id));
        Assert.That(returnedItem.Naziv, Is.EqualTo("Mleko"));
    }
    //POST---------------------------------------------------------------------------------
    [Test]
    public async Task Create_Returns_CreatedAtAction_When_Data_Is_Valid()
    {
        var request = new CreateShoppingItemRequest
        {
            GroupId = 1,
            TrazioUserId = 1,
            Naziv = "Mleko"
        };

        var result = await _controller.Create(request);

        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
    }

    [Test]
    public async Task Create_Adds_New_Item_To_Database_When_Data_Is_Valid()//proveradva da li kreira item
    {
        var request = new CreateShoppingItemRequest
        {
            GroupId = 1,
            TrazioUserId = 1,
            Naziv = "Mleko"
        };

        await _controller.Create(request);

        Assert.That(_context.ShoppingListaItems.Count(), Is.EqualTo(1));//da li postoji item, da li se kreirao 
    }

    [Test]
    public async Task Create_Saves_Correct_Item_Data_When_Data_Is_Valid()//proverava da li su podaci upisani u bazu
    {
        var request = new CreateShoppingItemRequest
        {
            GroupId = 1,
            TrazioUserId = 1,
            Naziv = "Mleko"
        };

        await _controller.Create(request);//upisuje u bazu 

        var item = _context.ShoppingListaItems.First();//vadi iz baze

        Assert.That(item.GroupId, Is.EqualTo(1));
        Assert.That(item.TrazioUserId, Is.EqualTo(1));
        Assert.That(item.Naziv, Is.EqualTo("Mleko"));//proverava
    }

    //PUT---------------------------------------------------------------------------PUT
    [Test]
    public async Task Update_Returns_NoContent_When_Item_Exists()
    {
        var item = new ShoppingListaItem
        {
            GroupId = 1,
            TrazioUserId = 1,
            Naziv = "Mleko",
            TrazenoUtc = DateTime.UtcNow
        };

        _context.ShoppingListaItems.Add(item);
        await _context.SaveChangesAsync();

        var request = new UpdateShoppingItemRequest
        {
            Naziv = "Hleb",
            NabavioUserId = 2
        };

        var result = await _controller.Update(item.Id, request);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task Update_Returns_NotFound_When_Item_Does_Not_Exist()
    {
        var request = new UpdateShoppingItemRequest
        {
            Naziv = "Hleb",
            NabavioUserId = 2
        };

        var result = await _controller.Update(999, request);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Update_Changes_Item_Data_When_Item_Exists()
    {
        var item = new ShoppingListaItem
        {
            GroupId = 1,
            TrazioUserId = 1,
            Naziv = "Mleko",
            TrazenoUtc = DateTime.UtcNow
        };

        _context.ShoppingListaItems.Add(item);
        await _context.SaveChangesAsync();

        var request = new UpdateShoppingItemRequest
        {
            Naziv = "Hleb",
            NabavioUserId = 2
        };

        await _controller.Update(item.Id, request);

        var updatedItem = await _context.ShoppingListaItems.FindAsync(item.Id);

        Assert.That(updatedItem, Is.Not.Null);
        Assert.That(updatedItem!.Naziv, Is.EqualTo("Hleb"));
        Assert.That(updatedItem.NabavioUserId, Is.EqualTo(2));
        Assert.That(updatedItem.NabavljenoUtc, Is.Not.Null);
    }
    //DELETE---------------------------------------------------------------------
    [Test]
    public async Task Delete_Returns_NoContent_When_Item_Exists()
    {
        var item = new ShoppingListaItem
        {
            GroupId = 1,
            TrazioUserId = 1,
            Naziv = "Mleko",
            TrazenoUtc = DateTime.UtcNow
        };

        _context.ShoppingListaItems.Add(item);
        await _context.SaveChangesAsync();

        var result = await _controller.Delete(item.Id);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task Delete_Returns_NotFound_When_Item_Does_Not_Exist()
    {
        var result = await _controller.Delete(999);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Delete_Removes_Item_From_Database_When_Item_Exists()
    {
        var item = new ShoppingListaItem
        {
            GroupId = 1,
            TrazioUserId = 1,
            Naziv = "Mleko",
            TrazenoUtc = DateTime.UtcNow
        };

        _context.ShoppingListaItems.Add(item);
        await _context.SaveChangesAsync();

        await _controller.Delete(item.Id);

        var deletedItem = await _context.ShoppingListaItems.FindAsync(item.Id);

        Assert.That(deletedItem, Is.Null);
    }
}