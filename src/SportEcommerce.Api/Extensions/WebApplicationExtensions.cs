using Microsoft.EntityFrameworkCore;
using SportEcommerce.Infrastructure.Persistence;

namespace SportEcommerce.Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        var settings = app.Configuration.GetSection("Database");
        var migrate = settings.GetValue<bool>("MigrateOnStartup");
        var seed = settings.GetValue<bool>("SeedOnStartup");

        if (!migrate && !seed)
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (migrate)
        {
            await db.Database.MigrateAsync();
        }

        if (seed)
        {
            await DbSeeder.SeedAsync(db);
        }
    }
}
