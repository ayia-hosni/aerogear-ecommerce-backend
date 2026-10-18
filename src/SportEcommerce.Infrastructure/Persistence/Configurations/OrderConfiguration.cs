using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportEcommerce.Domain.Entities;

namespace SportEcommerce.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Number).HasMaxLength(30).IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.Total).HasPrecision(18, 2);

        builder.HasIndex(o => o.Number).IsUnique();
        builder.HasIndex(o => new { o.UserId, o.CreatedAt });

        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.FullName).HasMaxLength(100).IsRequired();
            address.Property(a => a.Line1).HasMaxLength(200).IsRequired();
            address.Property(a => a.Line2).HasMaxLength(200);
            address.Property(a => a.City).HasMaxLength(100).IsRequired();
            address.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
            address.Property(a => a.Country).HasMaxLength(100).IsRequired();
        });
        builder.Navigation(o => o.ShippingAddress).IsRequired();
    }
}
