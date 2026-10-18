using SportEcommerce.Domain.Common;

namespace SportEcommerce.Domain.Entities;

public class OrderItem : Entity
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public required string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}
