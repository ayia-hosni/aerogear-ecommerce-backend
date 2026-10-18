using Microsoft.EntityFrameworkCore;
using SportEcommerce.Application.Common;
using SportEcommerce.Application.Orders;
using SportEcommerce.Domain.Entities;
using SportEcommerce.Domain.Enums;
using SportEcommerce.Infrastructure.Persistence;
using SportEcommerce.Tests.Support;

namespace SportEcommerce.Tests.Orders;

public sealed class OrderServiceTests : IAsyncDisposable
{
    private static readonly ShippingAddressDto Address =
        new("Jane Doe", "1 Main St", null, "Cairo", "11511", "Egypt");

    private readonly TestDatabase _database = new();
    private readonly Guid _userId;
    private readonly Guid _productId;

    public OrderServiceTests()
    {
        using var db = _database.CreateContext();
        var user = new User { FullName = "Jane Doe", Email = "jane@example.com", PasswordHash = "hash" };
        var product = new Product
        {
            Name = "Trail Shoe",
            Slug = "trail-shoe",
            Summary = "Grippy",
            ImageUrl = "https://example.com/shoe.png",
            Price = 100m,
            Stock = 5,
            Theme = ProductTheme.Blue,
            Category = new Category { Name = "Running", Slug = "running" }
        };
        db.Users.Add(user);
        db.Products.Add(product);
        db.SaveChanges();
        _userId = user.Id;
        _productId = product.Id;
    }

    [Fact]
    public async Task CreateAsync_SnapshotsPricesAndDecrementsStock()
    {
        await using var db = _database.CreateContext();
        var service = CreateService(db);

        var order = await service.CreateAsync(
            _userId,
            new CreateOrderRequest([new OrderItemRequest(_productId, 2), new OrderItemRequest(_productId, 1)], Address),
            CancellationToken.None);

        Assert.Equal(300m, order.Total);
        Assert.Single(order.Items);
        Assert.Equal(3, order.Items[0].Quantity);
        Assert.Equal(OrderStatus.Pending, order.Status);

        await using var verify = _database.CreateContext();
        Assert.Equal(2, await verify.Products.Where(p => p.Id == _productId).Select(p => p.Stock).SingleAsync());
    }

    [Fact]
    public async Task CreateAsync_RejectsQuantityAboveStock()
    {
        await using var db = _database.CreateContext();
        var service = CreateService(db);

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(
            _userId,
            new CreateOrderRequest([new OrderItemRequest(_productId, 6)], Address),
            CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_RejectsUnknownProduct()
    {
        await using var db = _database.CreateContext();
        var service = CreateService(db);

        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateAsync(
            _userId,
            new CreateOrderRequest([new OrderItemRequest(Guid.NewGuid(), 1)], Address),
            CancellationToken.None));
    }

    [Fact]
    public async Task GetAsync_HidesOrdersOfOtherUsers()
    {
        await using var db = _database.CreateContext();
        var service = CreateService(db);
        var order = await service.CreateAsync(
            _userId,
            new CreateOrderRequest([new OrderItemRequest(_productId, 1)], Address),
            CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetAsync(Guid.NewGuid(), order.Id, CancellationToken.None));
    }

    public ValueTask DisposeAsync() => _database.DisposeAsync();

    private static IOrderService CreateService(AppDbContext db) =>
        new ServiceFactory(db).Orders;
}
