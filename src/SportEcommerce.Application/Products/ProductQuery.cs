using System.ComponentModel.DataAnnotations;

namespace SportEcommerce.Application.Products;

public enum ProductSort
{
    Newest,
    PriceAscending,
    PriceDescending,
    TopRated
}

public sealed class ProductQuery
{
    public string? Category { get; init; }

    [MaxLength(100)]
    public string? Search { get; init; }

    public bool? Featured { get; init; }

    public ProductSort Sort { get; init; } = ProductSort.Newest;

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 50)]
    public int PageSize { get; init; } = 12;
}
