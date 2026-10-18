namespace SportEcommerce.Application.Common;

public abstract class AppException(string message) : Exception(message);

public sealed class NotFoundException(string resource, object key)
    : AppException($"{resource} '{key}' was not found.");

public sealed class ConflictException(string message) : AppException(message);

public sealed class BusinessRuleException(string message) : AppException(message);

public sealed class InvalidCredentialsException() : AppException("Invalid email or password.");
