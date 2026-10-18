using Microsoft.EntityFrameworkCore;
using SportEcommerce.Domain.Entities;
using SportEcommerce.Domain.Enums;

namespace SportEcommerce.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Categories.AnyAsync(cancellationToken))
        {
            await UpdateCatalogAsync(db, cancellationToken);
            return;
        }

        var categories = new[]
        {
            new Category { Name = "Running", Slug = "running", SortOrder = 1 },
            new Category { Name = "Basketball", Slug = "basketball", SortOrder = 2 },
            new Category { Name = "Football", Slug = "football", SortOrder = 3 },
            new Category { Name = "Training", Slug = "training", SortOrder = 4 },
            new Category { Name = "Tennis", Slug = "tennis", SortOrder = 5 }
        }.ToDictionary(c => c.Slug);

        db.Categories.AddRange(categories.Values);
        var products = BuildProducts(categories).ToList();
        foreach (var product in products)
        {
            ApplyOptions(product);
        }

        db.Products.AddRange(products);
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Brings a database seeded by an older version up to date: products that are missing are added, pictures with a
    /// background are swapped for cut-outs, and empty colours and sizes are filled in. Anything already set is kept.
    /// </summary>
    private static async Task UpdateCatalogAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var categories = await db.Categories.ToDictionaryAsync(c => c.Slug, cancellationToken);
        var existing = await db.Products.ToListAsync(cancellationToken);
        var known = existing.Select(p => p.Slug).ToHashSet();

        var added = BuildProducts(categories).Where(p => !known.Contains(p.Slug)).ToList();
        db.Products.AddRange(added);

        foreach (var product in existing.Concat(added))
        {
            ReplaceBackgroundPhotos(product);
            ApplyOptions(product);
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    // Resized and squared copy served by the frontend (the original is a 4976 px, 1.7 MB file).
    private const string GlovesImage = "/products/boxing-gloves.png";

    // Only pictures with a transparent background work in the stage, the cards and the recolouring.
    private static readonly Dictionary<string, string> CutOutImages = new()
    {
        ["trackfit-pro-watch"] = "https://pngimg.com/d/apple_watch_PNG66.png",
        ["pro-training-gloves"] = GlovesImage
    };

    // Pictures that were used before and looked wrong; a catalogue still holding one is moved to the current picture.
    private static readonly HashSet<string> RetiredImages = ["https://pngimg.com/d/boxing_gloves_PNG102641.png"];

    private static bool IsPhoto(string url) => url.Contains("images.unsplash.com", StringComparison.OrdinalIgnoreCase);

    private static void ReplaceBackgroundPhotos(Product product)
    {
        if ((IsPhoto(product.ImageUrl) || RetiredImages.Contains(product.ImageUrl)) && CutOutImages.TryGetValue(product.Slug, out var cutOut))
        {
            product.ImageUrl = cutOut;
        }

        if (product.Gallery.Any(IsPhoto))
        {
            product.Gallery = product.Gallery.Where(url => !IsPhoto(url)).ToList();
        }
    }

    private static readonly string[] ShoeSizes = ["39", "40", "41", "42", "43", "44", "45"];
    private static readonly string[] ApparelSizes = ["S", "M", "L", "XL"];

    // A colour is the product photo with its hue rotated, which only reads as a real colourway on strongly
    // single-coloured pictures; the others (white, black, tan) get sizes only.
    private static readonly Dictionary<string, (string[] Sizes, string[] SoldOut, ProductColor[] Colors)> Options = new()
    {
        ["aerosprint-vapor-2"] = (ShoeSizes, ["41"],
        [
            new("Volt", "#c6f01f", 0), new("Ember", "#ef5a2b", -62), new("Cobalt", "#2f6fd2", 150), new("Violet", "#8b4fd0", 215)
        ]),
        ["court-king-high-og"] = (ShoeSizes, ["44"], []),
        ["strike-elite-fg"] = (["38", "39", "40", "41", "42", "43"], ["40"], []),
        ["official-match-ball"] = ([], [],
        [
            new("Volt", "#d4f01f", 0), new("Coral", "#ef6a4a", -70), new("Aqua", "#27c4c0", 110), new("Violet", "#8b4fd0", 200)
        ]),
        ["hex-cast-dumbbell"] = (["10 kg", "15 kg", "20 kg", "25 kg"], ["25 kg"], []),
        ["aero-compression-top"] = (ApparelSizes, ["S"], []),
        ["hydroflask-32oz"] = (["21 oz", "32 oz", "40 oz"], [],
        [
            new("Ice", "#5aa7e8", 0), new("Mint", "#46c9a0", -60), new("Coral", "#ef6a4a", 150), new("Violet", "#8b4fd0", 50)
        ]),
        ["aero-performance-cap"] = (["S/M", "L/XL"], [], []),
        ["pro-training-gloves"] = (["10 oz", "12 oz", "14 oz", "16 oz"], ["10 oz"],
        [
            new("Crimson", "#d63a35", 0), new("Cobalt", "#2f6fd2", 220), new("Violet", "#8b4fd0", 280)
        ]),
        ["pro-carbon-racket"] = (["L1", "L2", "L3", "L4"], ["L1"],
        [
            new("Crimson", "#d63a35", 0), new("Cobalt", "#2f6fd2", 220), new("Lime", "#8cc63f", 80)
        ]),
        ["court-elite-basketball"] = (["Size 5", "Size 6", "Size 7"], [],
        [
            new("Classic", "#e8742a", 0), new("Cobalt", "#2f6fd2", 190), new("Forest", "#2f9e5e", 120)
        ]),
        ["aero-fleece-hoodie"] = (ApparelSizes, ["S"],
        [
            new("Amber", "#f1b21f", 0), new("Coral", "#ef6a4a", -40), new("Teal", "#2aa8a0", 130), new("Violet", "#7c4fd0", 225)
        ]),
        ["olympic-barbell-set"] = (["60 kg", "100 kg", "140 kg"], ["140 kg"], [])
    };

    private static void ApplyOptions(Product product)
    {
        if (!Options.TryGetValue(product.Slug, out var options))
        {
            return;
        }

        if (product.Sizes.Count == 0 && options.Sizes.Length > 0)
        {
            product.Sizes = options.Sizes.ToList();
            product.SoldOutSizes = options.SoldOut.ToList();
        }

        if (product.Colors.Count == 0 && options.Colors.Length > 0)
        {
            product.Colors = options.Colors.ToList();
        }
    }

    private static IEnumerable<Product> BuildProducts(IReadOnlyDictionary<string, Category> categories)
    {
        var start = DateTime.UtcNow;
        var order = 0;

        Product Create(
            string category, string name, string slug, string? series, string summary, string? badge,
            decimal price, int stock, double rating, int reviews, string image, string[] gallery,
            string[] tags, ProductTheme theme, bool featured) => new()
        {
            Name = name,
            Slug = slug,
            Series = series,
            Summary = summary,
            Badge = badge,
            Price = price,
            Stock = stock,
            Rating = rating,
            ReviewCount = reviews,
            ImageUrl = image,
            Gallery = gallery.ToList(),
            Tags = tags.ToList(),
            Theme = theme,
            IsFeatured = featured,
            Category = categories[category],
            CreatedAt = start.AddMinutes(-order++)
        };

        yield return Create("running", "AeroSprint Vapor 2.0", "aerosprint-vapor-2", "Pro Elite Series",
            "Carbon plated racing shoe built for explosive speed.", "New Arrival", 275.00m, 40, 4.9, 1200,
            "https://pngimg.com/d/running_shoes_PNG5816.png",
            [],
            ["Carbon Plate", "Ultra Light"], ProductTheme.Blue, true);

        yield return Create("basketball", "Court King High OG", "court-king-high-og", "Retro Collection",
            "Premium leather high-top with full ankle support.", "Limited Ed", 190.00m, 25, 5.0, 3400,
            "https://pngimg.com/d/running_shoes_PNG5822.png",
            [],
            ["Premium Leather", "Ankle Support"], ProductTheme.Dark, true);

        yield return Create("football", "Strike Elite FG", "strike-elite-fg", "Pitch Control",
            "Firm-ground boot with a composite upper for precise touch.", "Best Seller", 79.95m, 60, 4.8, 850,
            "https://pngimg.com/d/football_boots_PNG1.png",
            [],
            ["Size 7", "Composite"], ProductTheme.Green, true);

        yield return Create("training", "Pro Elite Backpack", "pro-elite-backpack", "Urban Series",
            "40L waterproof backpack for the gym and the commute.", "Trending", 110.00m, 80, 4.9, 2200,
            "https://pngimg.com/d/backpack_PNG6355.png",
            [],
            ["Waterproof", "40L Cap"], ProductTheme.Amber, true);

        yield return Create("tennis", "Official Match Ball", "official-match-ball", "Pro Equipment",
            "Graphite composite match ball with consistent spin response.", "Pro Choice", 220.00m, 35, 4.9, 640,
            "https://pngimg.com/d/tennis_PNG10417.png",
            [],
            ["Graphite", "Spin Effect", "300g"], ProductTheme.Purple, true);

        yield return Create("training", "Hex Cast Dumbbell", "hex-cast-dumbbell", "Home Gym",
            "Rubber coated cast iron hex dumbbell.", "Strength", 65.00m, 120, 4.6, 3000,
            "https://pngimg.com/d/dumbbell_PNG16388.png", [],
            ["Cast Iron", "Rubber Coated", "20 kg"], ProductTheme.Slate, true);

        yield return Create("running", "TrackFit Pro Watch", "trackfit-pro-watch", null,
            "GPS, Heart Rate, VO2 Max", null, 299.00m, 50, 4.9, 980,
            "https://pngimg.com/d/apple_watch_PNG66.png", [], [], ProductTheme.Blue, false);

        yield return Create("training", "Aero Compression Top", "aero-compression-top", null,
            "Sweat-wicking, Muscle support", null, 45.00m, 200, 4.6, 540,
            "https://pngimg.com/d/tshirt_PNG5449.png", [], [], ProductTheme.Blue, false);

        yield return Create("training", "Elite Training Duffle", "elite-training-duffle", null,
            "40L capacity, water-resistant", null, 65.00m, 90, 4.8, 720,
            "https://pngimg.com/d/backpack_PNG6355.png", [], [], ProductTheme.Blue, false);

        yield return Create("training", "HydroFlask 32oz", "hydroflask-32oz", null,
            "Vacuum insulated stainless", null, 35.00m, 300, 4.9, 1500,
            "https://pngimg.com/d/water_bottle_PNG98947.png", [], [], ProductTheme.Blue, false);

        yield return Create("running", "Aero Performance Cap", "aero-performance-cap", null,
            "Breathable mesh, UV protect", null, 24.00m, 250, 4.7, 410,
            "https://pngimg.com/d/cap_PNG5675.png", [], [], ProductTheme.Blue, false);

        yield return Create("training", "Pro Training Gloves", "pro-training-gloves", null,
            "14oz synthetic leather protection", null, 55.00m, 110, 4.8, 630,
            GlovesImage, [], [], ProductTheme.Blue, false);

        yield return Create("tennis", "Pro Carbon Racket", "pro-carbon-racket", "Match Point",
            "Lightweight graphite frame with a large sweet spot.", "New Arrival", 189.00m, 45, 4.7, 520,
            "https://pngimg.com/d/tennis_PNG10421.png", [],
            ["Graphite", "300g"], ProductTheme.Green, false);

        yield return Create("basketball", "Court Elite Basketball", "court-elite-basketball", "Indoor / Outdoor",
            "Composite leather ball with deep channels for grip.", "Best Seller", 39.00m, 150, 4.8, 1320,
            "https://pngimg.com/d/basketball_PNG102514.png", [],
            ["Composite Leather", "Deep Channel"], ProductTheme.Amber, false);

        yield return Create("training", "Aero Fleece Hoodie", "aero-fleece-hoodie", "Cold Weather",
            "Heavyweight fleece with a soft brushed inner.", null, 69.00m, 120, 4.7, 860,
            "https://pngimg.com/d/hoodie_PNG45.png", [],
            ["Fleece", "Relaxed Fit"], ProductTheme.Slate, false);

        yield return Create("running", "Velocity Sport Shades", "velocity-sport-shades", "Race Day",
            "Wrap-around mirrored lenses with full UV protection.", null, 85.00m, 80, 4.6, 410,
            "https://pngimg.com/d/sunglasses_PNG154.png", [],
            ["UV400", "Mirrored"], ProductTheme.Dark, false);

        yield return Create("training", "Olympic Barbell Set", "olympic-barbell-set", "Strength Lab",
            "Calibrated bumper plates on a 20 kg Olympic bar.", "Pro Choice", 349.00m, 20, 4.9, 280,
            "https://pngimg.com/d/barbell_PNG16356.png", [],
            ["20 kg Bar", "Bumper Plates"], ProductTheme.Purple, false);
    }
}
