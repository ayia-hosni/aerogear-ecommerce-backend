using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SportEcommerce.Infrastructure.Persistence;

namespace SportEcommerce.Tests.Support;

public sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public TestDatabase()
    {
        _connection.Open();
        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);

    public ValueTask DisposeAsync() => _connection.DisposeAsync();
}
