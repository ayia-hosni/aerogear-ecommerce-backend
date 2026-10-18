using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportEcommerce.Domain.Entities;

namespace SportEcommerce.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Slug).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Series).HasMaxLength(100);
        builder.Property(p => p.Summary).HasMaxLength(500).IsRequired();
        builder.Property(p => p.Badge).HasMaxLength(50);
        builder.Property(p => p.ImageUrl).HasMaxLength(500).IsRequired();
        builder.Property(p => p.Price).HasPrecision(18, 2);
        builder.Property(p => p.Stock).IsConcurrencyToken();
        builder.Property(p => p.Theme).HasConversion<string>().HasMaxLength(20);

        builder.OwnsMany(p => p.Colors, colors => colors.ToJson());

        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => new { p.IsActive, p.IsFeatured });

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
