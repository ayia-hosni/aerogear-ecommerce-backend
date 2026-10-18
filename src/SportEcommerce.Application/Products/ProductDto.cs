using SportEcommerce.Domain.Entities;
using SportEcommerce.Domain.Enums;

namespace SportEcommerce.Application.Products;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Slug,
    string? Series,
    string Summary,
    string? Badge,
    decimal Price,
    int Stock,
    double Rating,
    int ReviewCount,
    string ImageUrl,
    IReadOnlyList<string> Gallery,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> Sizes,
    IReadOnlyList<string> SoldOutSizes,
    IReadOnlyList<ProductColor> Colors,
    ProductTheme Theme,
    bool IsFeatured,
    string CategorySlug,
    string CategoryName);
