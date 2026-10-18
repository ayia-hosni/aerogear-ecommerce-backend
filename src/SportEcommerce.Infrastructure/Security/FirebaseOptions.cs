namespace SportEcommerce.Infrastructure.Security;

/// <summary>
/// Firebase web app settings. None of these are secrets: the browser needs them to start a sign-in,
/// so the API hands them out at runtime instead of baking them into the frontend build.
/// </summary>
public sealed class FirebaseOptions
{
    public const string SectionName = "Firebase";

    public string ProjectId { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string AuthDomain { get; init; } = string.Empty;
    public string AppId { get; init; } = string.Empty;
}
