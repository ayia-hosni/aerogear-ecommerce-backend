using Microsoft.EntityFrameworkCore;
using SportEcommerce.Infrastructure.Persistence;
using SportEcommerce.Tests.Support;

namespace SportEcommerce.Tests.Persistence;

public sealed class DbSeederTests : IAsyncDisposable
{
    private readonly TestDatabase _database = new();

    [Fact]
    public async Task Seed_GivesProductsTheirColoursAndSizes()
    {
        await using var db = _database.CreateContext();

        await DbSeeder.SeedAsync(db);

        var shoe = await db.Products.AsNoTracking().SingleAsync(p => p.Slug == "aerosprint-vapor-2");
        Assert.Equal(4, shoe.Colors.Count);
        Assert.Contains("41", shoe.SoldOutSizes);
        Assert.All(shoe.SoldOutSizes, size => Assert.Contains(size, shoe.Sizes));
    }

    [Fact]
    public async Task Seed_AddsOptionsToProductsSeededBefore_WithoutTouchingEditedOnes()
    {
        await using var db = _database.CreateContext();
        await DbSeeder.SeedAsync(db);

        var old = await db.Products.SingleAsync(p => p.Slug == "official-match-ball");
        old.Colors = [];
        var edited = await db.Products.SingleAsync(p => p.Slug == "aerosprint-vapor-2");
        edited.Sizes = ["Only"];
        await db.SaveChangesAsync();

        await DbSeeder.SeedAsync(db);

        Assert.NotEmpty((await db.Products.AsNoTracking().SingleAsync(p => p.Slug == "official-match-ball")).Colors);
        Assert.Equal(["Only"], (await db.Products.AsNoTracking().SingleAsync(p => p.Slug == "aerosprint-vapor-2")).Sizes);
    }

    [Fact]
    public async Task Seed_UsesOnlyTransparentCutOutPictures()
    {
        await using var db = _database.CreateContext();

        await DbSeeder.SeedAsync(db);

        var products = await db.Products.AsNoTracking().ToListAsync();
        Assert.True(products.Count >= 17);
        Assert.All(products, product =>
        {
            Assert.DoesNotContain("unsplash", product.ImageUrl);
            Assert.Empty(product.Gallery);
        });
    }

    [Fact]
    public async Task Seed_UpgradesAnOlderCatalogue()
    {
        await using var db = _database.CreateContext();
        await DbSeeder.SeedAsync(db);

        // What a database seeded by the first version looked like: photos with backgrounds, and no racket yet.
        var watch = await db.Products.SingleAsync(p => p.Slug == "trackfit-pro-watch");
        watch.ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600";
        watch.Gallery = ["https://images.unsplash.com/photo-1?w=600", "https://pngimg.com/d/keep-me.png"];
        db.Products.Remove(await db.Products.SingleAsync(p => p.Slug == "pro-carbon-racket"));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await DbSeeder.SeedAsync(db);

        var upgraded = await db.Products.AsNoTracking().SingleAsync(p => p.Slug == "trackfit-pro-watch");
        Assert.DoesNotContain("unsplash", upgraded.ImageUrl);
        Assert.Equal(["https://pngimg.com/d/keep-me.png"], upgraded.Gallery);
        Assert.True(await db.Products.AnyAsync(p => p.Slug == "pro-carbon-racket"));
    }

    [Fact]
    public async Task Seed_MovesProductsOffPicturesThatWereRetired()
    {
        await using var db = _database.CreateContext();
        await DbSeeder.SeedAsync(db);

        var gloves = await db.Products.SingleAsync(p => p.Slug == "pro-training-gloves");
        gloves.ImageUrl = "https://pngimg.com/d/boxing_gloves_PNG102641.png";
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await DbSeeder.SeedAsync(db);

        var fixedGloves = await db.Products.AsNoTracking().SingleAsync(p => p.Slug == "pro-training-gloves");
        Assert.Equal("/products/boxing-gloves.png", fixedGloves.ImageUrl);
    }

    public ValueTask DisposeAsync() => _database.DisposeAsync();
}
