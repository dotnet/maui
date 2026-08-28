namespace DotNet.Release;

/// <summary>A single fail-closed condition, carrying a stable <see cref="Code"/>.</summary>
internal sealed record ReleaseError(string Code, string Message)
{
    public override string ToString() => $"{Code}: {Message}";
}

/// <summary>
/// The outcome of a pure policy decision: either a value, or one or more errors.
/// </summary>
/// <remarks>
/// Errors are returned rather than thrown so that validation can report every problem it
/// found in one pass, and so tests can assert on codes without exception plumbing.
/// </remarks>
internal sealed class Result<T>
{
    private readonly T? _value;

    private Result(T? value, IReadOnlyList<ReleaseError> errors)
    {
        _value = value;
        Errors = errors;
    }

    public IReadOnlyList<ReleaseError> Errors { get; }

    public bool IsSuccess => Errors.Count == 0;

    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Result is a failure: {string.Join("; ", Errors)}");

    public static Result<T> Success(T value) => new(value, []);

    public static Result<T> Failure(string code, string message) =>
        new(default, [new ReleaseError(code, message)]);

    public static Result<T> Failure(IReadOnlyList<ReleaseError> errors)
    {
        if (errors.Count == 0)
        {
            throw new ArgumentException("A failure must carry at least one error.", nameof(errors));
        }

        return new Result<T>(default, errors);
    }

    /// <summary>Re-types an existing failure without restating its errors.</summary>
    public Result<TOther> ToFailure<TOther>() => Result<TOther>.Failure(Errors);

    public bool HasError(string code) => Errors.Any(e => e.Code == code);
}
