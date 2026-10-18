using System.ComponentModel.DataAnnotations;

namespace SportEcommerce.Application.Auth;

public sealed record RegisterRequest(
    [Required, StringLength(100, MinimumLength = 2)] string FullName,
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(100, MinimumLength = 8),
     RegularExpression(@"^(?=.*\p{L})(?=.*\d).+$", ErrorMessage = "Password must contain at least one letter and one number.")] string Password);

public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public sealed record FirebaseLoginRequest([Required] string IdToken);

public sealed record UserDto(Guid Id, string FullName, string Email);

public sealed record AuthResponse(string AccessToken, DateTime ExpiresAt, UserDto User);
