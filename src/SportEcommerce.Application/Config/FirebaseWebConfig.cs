namespace SportEcommerce.Application.Config;

/// <summary>The public Firebase web settings the browser needs to start a sign-in.</summary>
public sealed record FirebaseWebConfig(string ApiKey, string AuthDomain, string ProjectId, string AppId);
