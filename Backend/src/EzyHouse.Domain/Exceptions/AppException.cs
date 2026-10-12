namespace EzyHouse.Domain.Exceptions;

public abstract class AppException(string message) : Exception(message)
{
    public abstract int StatusCode { get; }

    public abstract string Title { get; }
}

public sealed class ValidationFailedException(string message) : AppException(message)
{
    public override int StatusCode => 400;

    public override string Title => "Validation failed";
}

public sealed class ForbiddenException(string message) : AppException(message)
{
    public override int StatusCode => 403;

    public override string Title => "Not allowed";
}

public sealed class NotFoundException(string message) : AppException(message)
{
    public override int StatusCode => 404;

    public override string Title => "Not found";
}

public sealed class BusinessRuleException(string message) : AppException(message)
{
    public override int StatusCode => 409;

    public override string Title => "Breaks a business rule";
}
