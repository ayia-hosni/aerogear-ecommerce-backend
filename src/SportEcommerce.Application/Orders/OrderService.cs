using Microsoft.EntityFrameworkCore;
using SportEcommerce.Application.Abstractions;
using SportEcommerce.Application.Common;
using SportEcommerce.Domain.Entities;

namespace SportEcommerce.Application.Orders;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(Guid userId, CreateOrderRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrderDto>> ListAsync(Guid userId, CancellationToken cancellationToken);
    Task<OrderDto> GetAsync(Guid userId, Guid orderId, CancellationToken cancellationToken);
}

internal sealed class OrderService(IAppDbContext db, TimeProvider clock) : IOrderService
{
    public async Task<OrderDto> CreateAsync(Guid userId, CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var requested = request.Items
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

        var productIds = requested.Keys.ToList();
        var products = await db.Products
            .Where(p => p.IsActive && productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var missing = productIds.Except(products.Select(p => p.Id)).FirstOrDefault();
        if (missing != Guid.Empty)
        {
            throw new NotFoundException(nameof(Product), missing);
        }

        var address = request.ShippingAddress;
        var order = new Order
        {
            Number = GenerateNumber(),
            UserId = userId,
            ShippingAddress = new ShippingAddress
            {
                FullName = address.FullName.Trim(),
                Line1 = address.Line1.Trim(),
                Line2 = address.Line2?.Trim(),
                City = address.City.Trim(),
                PostalCode = address.PostalCode.Trim(),
                Country = address.Country.Trim()
            }
        };

        foreach (var product in products)
        {
            var quantity = requested[product.Id];
            if (!product.HasStockFor(quantity))
            {
                throw new BusinessRuleException($"Only {product.Stock} unit(s) of '{product.Name}' are available.");
            }

            product.Reserve(quantity);
            order.AddItem(product, quantity);
        }

        db.Orders.Add(order);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("Stock changed while placing the order. Please review your cart and try again.");
        }

        return OrderMapping.ToDto(order);
    }

    public async Task<IReadOnlyList<OrderDto>> ListAsync(Guid userId, CancellationToken cancellationToken)
    {
        var orders = await db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(OrderMapping.ToDto).ToList();
    }

    public async Task<OrderDto> GetAsync(Guid userId, Guid orderId, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), orderId);

        return OrderMapping.ToDto(order);
    }

    private string GenerateNumber() =>
        $"AG-{clock.GetUtcNow():yyMMdd}-{Random.Shared.Next(100000, 999999)}";
}
