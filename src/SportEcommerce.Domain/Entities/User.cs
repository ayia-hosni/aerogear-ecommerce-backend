using SportEcommerce.Domain.Common;

namespace SportEcommerce.Domain.Entities;

public class User : Entity
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    /// <summary>Null for accounts created through a social provider and never given a password.</summary>
    public string? PasswordHash { get; set; }
    public string? FirebaseUid { get; set; }
    public ICollection<Order> Orders { get; } = [];
}
