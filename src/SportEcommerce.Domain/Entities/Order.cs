using SportEcommerce.Domain.Common;
using SportEcommerce.Domain.Enums;

namespace SportEcommerce.Domain.Entities;

public class Order : Entity
{
    public required string Number { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public required ShippingAddress ShippingAddress { get; set; }
    public decimal Total { get; private set; }
    public List<OrderItem> Items { get; } = [];

    public void AddItem(Product product, int quantity)
    {
        var existing = Items.Find(i => i.ProductId == product.Id);
        if (existing is not null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = quantity
            });
        }

        Total = Items.Sum(i => i.UnitPrice * i.Quantity);
    }
}
