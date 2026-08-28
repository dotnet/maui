namespace DotNet.Release;

/// <summary>Stable identifiers for release validation failures.</summary>
internal static class ErrorCodes
{
    public const string RepositoryNotAllowed = "REPO_NOT_ALLOWED";
    public const string RepositoryUnparseable = "REPO_UNPARSEABLE";
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
