using SportEcommerce.Domain.Common;

namespace SportEcommerce.Domain.Entities;

public class Category : Entity
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public int SortOrder { get; set; }
    public ICollection<Product> Products { get; } = [];
}
