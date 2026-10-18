using Microsoft.EntityFrameworkCore;
using SportEcommerce.Domain.Entities;

namespace SportEcommerce.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<User> Users { get; }
    DbSet<Order> Orders { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
