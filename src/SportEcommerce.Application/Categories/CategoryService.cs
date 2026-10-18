using Microsoft.EntityFrameworkCore;
using SportEcommerce.Application.Abstractions;

namespace SportEcommerce.Application.Categories;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> ListAsync(CancellationToken cancellationToken);
}

internal sealed class CategoryService(IAppDbContext db) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryDto>> ListAsync(CancellationToken cancellationToken) =>
        await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Slug))
            .ToListAsync(cancellationToken);
}
