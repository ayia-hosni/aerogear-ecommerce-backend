using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportEcommerce.Domain.Entities;

namespace SportEcommerce.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.FullName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.FirebaseUid).HasMaxLength(128);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.FirebaseUid).IsUnique();
    }
}
