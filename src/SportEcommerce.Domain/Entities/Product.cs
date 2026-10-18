using SportEcommerce.Domain.Common;
using SportEcommerce.Domain.Enums;

namespace SportEcommerce.Domain.Entities;

public class Product : Entity
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Series { get; set; }
    public required string Summary { get; set; }
    public string? Badge { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public required string ImageUrl { get; set; }
    public List<string> Gallery { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public List<string> Sizes { get; set; } = [];
    public List<string> SoldOutSizes { get; set; } = [];
    public List<ProductColor> Colors { get; set; } = [];
    public ProductTheme Theme { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public bool HasStockFor(int quantity) => quantity > 0 && Stock >= quantity;

    public void Reserve(int quantity)
    {
        if (!HasStockFor(quantity))
        {
            throw new InvalidOperationException($"Insufficient stock for '{Name}'.");
        }

        Stock -= quantity;
    }
}
