namespace FrangoFrito.Application.Common;

public enum ApplicationResultStatus
{
    Success,
    NotFound,
    Conflict,
    ValidationError,
    Unauthorized,
    Forbidden
}

public sealed record ApplicationError(string Title, string? Detail = null);

public class ApplicationResult
{
    protected ApplicationResult(
        ApplicationResultStatus status,
        ApplicationError? error = null,
        IReadOnlyDictionary<string, string[]>? validationErrors = null)
    {
        Status = status;
        Error = error;
        ValidationErrors = validationErrors;
    }

    public ApplicationResultStatus Status { get; }
    public ApplicationError? Error { get; }
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; }
    public bool Succeeded => Status == ApplicationResultStatus.Success;

    public static ApplicationResult Success() => new(ApplicationResultStatus.Success);

    public static ApplicationResult NotFound() => new(ApplicationResultStatus.NotFound);

    public static ApplicationResult Conflict(string title, string detail) =>
        new(ApplicationResultStatus.Conflict, new ApplicationError(title, detail));

    public static ApplicationResult Validation(IReadOnlyDictionary<string, string[]> errors) =>
        new(ApplicationResultStatus.ValidationError, validationErrors: errors);

    public static ApplicationResult Unauthorized(string title) =>
        new(ApplicationResultStatus.Unauthorized, new ApplicationError(title));

    public static ApplicationResult Forbidden() => new(ApplicationResultStatus.Forbidden);
}

public sealed class ApplicationResult<T> : ApplicationResult
{
    private ApplicationResult(
        ApplicationResultStatus status,
        T? value = default,
        ApplicationError? error = null,
        IReadOnlyDictionary<string, string[]>? validationErrors = null)
        : base(status, error, validationErrors)
    {
        Value = value;
    }

    public T? Value { get; }

    public static ApplicationResult<T> Success(T value) =>
        new(ApplicationResultStatus.Success, value);

    public new static ApplicationResult<T> NotFound() =>
        new(ApplicationResultStatus.NotFound);

    public new static ApplicationResult<T> Conflict(string title, string detail) =>
        new(ApplicationResultStatus.Conflict, error: new ApplicationError(title, detail));

    public new static ApplicationResult<T> Validation(IReadOnlyDictionary<string, string[]> errors) =>
        new(ApplicationResultStatus.ValidationError, validationErrors: errors);

    public new static ApplicationResult<T> Unauthorized(string title) =>
        new(ApplicationResultStatus.Unauthorized, error: new ApplicationError(title));

    public new static ApplicationResult<T> Forbidden() =>
        new(ApplicationResultStatus.Forbidden);
}
