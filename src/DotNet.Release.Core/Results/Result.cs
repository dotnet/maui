namespace DotNet.Release.Core;

/// <summary>
/// Stable identifiers for every way a release can fail closed.
/// </summary>
/// <remarks>
/// Codes exist so failures can be documented, searched for in build logs and asserted in
/// tests. The current pipeline throws ad-hoc prose, which cannot be any of those things.
/// </remarks>
public static class ErrorCodes
{
    public const string RepositoryNotAllowed = "REPO_NOT_ALLOWED";
    public const string RepositoryUnparseable = "REPO_UNPARSEABLE";
    public const string WorkloadMismatch = "WORKLOAD_MISMATCH";

    public const string BarBuildNotFound = "BAR_BUILD_NOT_FOUND";
    public const string BarBuildNotUnique = "BAR_BUILD_NOT_UNIQUE";
    public const string BarRepositoryMismatch = "BAR_REPO_MISMATCH";
    public const string BarCommitMismatch = "BAR_COMMIT_MISMATCH";
    public const string BarChannelMissing = "BAR_CHANNEL_MISSING";
    public const string BarMirrorNameInvalid = "BAR_MIRROR_NAME_INVALID";

    public const string PackageMalformed = "PACKAGE_MALFORMED";
    public const string PackageDuplicateFileName = "PACKAGE_DUPLICATE_FILENAME";
    public const string PackageDuplicateIdentity = "PACKAGE_DUPLICATE_IDENTITY";
    public const string PackageSetEmpty = "PACKAGE_SET_EMPTY";
    public const string PackageSetNotFound = "PACKAGE_SET_NOT_FOUND";
    public const string PackageSetMismatch = "PACKAGE_SET_MISMATCH";
    public const string ManifestInNonWorkload = "MANIFEST_IN_NON_WORKLOAD";

    public const string WorkloadBandUnresolved = "WORKLOAD_BAND_UNRESOLVED";
    public const string WorkloadBandAmbiguous = "WORKLOAD_BAND_AMBIGUOUS";
    public const string WorkloadSetNotConfigured = "WORKLOAD_SET_NOT_CONFIGURED";

    public const string FilterUnmatched = "FILTER_UNMATCHED";

    public const string PlanSchemaInvalid = "PLAN_SCHEMA_INVALID";
    public const string PlanHashMismatch = "PLAN_HASH_MISMATCH";
    public const string PackageHashMismatch = "PACKAGE_HASH_MISMATCH";
    public const string PackageFileMissing = "PACKAGE_FILE_MISSING";
    public const string PackageFileUnexpected = "PACKAGE_FILE_UNEXPECTED";

    public const string PolicyInvalid = "POLICY_INVALID";
}

/// <summary>A single fail-closed condition, carrying a stable <see cref="Code"/>.</summary>
public sealed record ReleaseError(string Code, string Message)
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
public sealed class Result<T>
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
