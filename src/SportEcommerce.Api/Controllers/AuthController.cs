using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SportEcommerce.Api.Extensions;
using SportEcommerce.Application.Auth;

namespace SportEcommerce.Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting(SecurityExtensions.AuthRateLimit)]
public sealed class AuthController(IAuthService auth, ISocialLoginService social) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await auth.RegisterAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Me), response);
    }

    [HttpPost("login")]
    public Task<AuthResponse> Login(LoginRequest request, CancellationToken cancellationToken) =>
        auth.LoginAsync(request, cancellationToken);

    [HttpPost("firebase")]
    public Task<AuthResponse> Firebase(FirebaseLoginRequest request, CancellationToken cancellationToken) =>
        social.FirebaseLoginAsync(request, cancellationToken);

    [Authorize]
    [DisableRateLimiting]
    [HttpGet("me")]
    public Task<UserDto> Me(CancellationToken cancellationToken) =>
        auth.GetProfileAsync(User.GetUserId(), cancellationToken);
}
