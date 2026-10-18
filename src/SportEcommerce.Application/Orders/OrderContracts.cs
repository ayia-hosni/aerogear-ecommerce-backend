using System.ComponentModel.DataAnnotations;
using SportEcommerce.Domain.Enums;

namespace SportEcommerce.Application.Orders;

public sealed record OrderItemRequest(
    [Required] Guid ProductId,
    [Range(1, 20)] int Quantity);

public sealed record ShippingAddressDto(
    [Required, StringLength(100)] string FullName,
    [Required, StringLength(200)] string Line1,
    [StringLength(200)] string? Line2,
    [Required, StringLength(100)] string City,
    [Required, StringLength(20)] string PostalCode,
    [Required, StringLength(100)] string Country);

public sealed record CreateOrderRequest(
    [Required, MinLength(1), MaxLength(50)] IReadOnlyList<OrderItemRequest> Items,
    [Required] ShippingAddressDto ShippingAddress);

public sealed record OrderLineDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);

public sealed record OrderDto(
    Guid Id,
    string Number,
    OrderStatus Status,
    decimal Total,
    DateTime CreatedAt,
    ShippingAddressDto ShippingAddress,
    IReadOnlyList<OrderLineDto> Items);
