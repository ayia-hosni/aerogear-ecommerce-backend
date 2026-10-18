using SportEcommerce.Domain.Entities;

namespace SportEcommerce.Application.Orders;

internal static class OrderMapping
{
    public static OrderDto ToDto(Order order) => new(
        order.Id,
        order.Number,
        order.Status,
        order.Total,
        order.CreatedAt,
        new ShippingAddressDto(
            order.ShippingAddress.FullName,
            order.ShippingAddress.Line1,
            order.ShippingAddress.Line2,
            order.ShippingAddress.City,
            order.ShippingAddress.PostalCode,
            order.ShippingAddress.Country),
        order.Items
            .Select(i => new OrderLineDto(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.LineTotal))
            .ToList());
}
