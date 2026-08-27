namespace DotNet.Release;

/// <summary>
/// Enforces the invariant that keeps the immutable plan and the mutable staging directory
/// consistent with each other.
/// </summary>
/// <remarks>
/// <para>
/// The invariant is: <c>for every package in the set, the file is present on disk if and
/// only if its disposition is <see cref="PackageDisposition.Pending"/></c>.
/// </para>
/// <para>
/// This is what makes the publish job fail closed. <c>1ES.PublishNuget@1</c> pushes whatever
/// its <c>packagesToPush</c> glob matches, so an unexpected file in the directory would be
/// published without ever having been validated, and a missing pending file would be
/// silently skipped.
/// </para>
/// </remarks>
public static class StagedSetIntegrity
{
    /// <summary>
    /// Validates the staging directory against the plan before <c>filter</c> has run: every
    /// planned file must be present with the hash recorded at stage time, and nothing else
    /// may be in the directory.
    /// </summary>
    /// <param name="set">The planned set.</param>
    /// <param name="observed">
    /// Observed <c>.nupkg</c> file names mapped to their SHA-256.
    /// <para>
    /// <b>Contract:</b> this must contain <i>only</i> the directory's <c>.nupkg</c> files.
    /// The unexpected-file rule is scoped by extension at the point of enumeration, never by
    /// allow-listing companion file names here. Companion files — <c>release-plan.json</c>,
    /// <c>release-set.json</c>, and anything added later — are simply never observed, so
    /// adding one can never require weakening this check. Allow-listing by name would work
    /// today and rot the moment a third file appeared, and the tempting fix at that point is
    /// to widen the filter, which silently disables the rule this exists to enforce.
    /// </para>
    /// </param>
    public static Result<bool> ValidateStaged(
        ReleasePackageSet set,
        IReadOnlyDictionary<string, string> observed) =>
        Validate(set, observed, _ => PackageDisposition.Pending);

    /// <summary>
    /// Validates the staging directory after <c>filter</c> has run, using the recorded
    /// dispositions.
    /// </summary>
    public static Result<bool> ValidateFiltered(
        ReleasePackageSet set,
        IReadOnlyDictionary<string, string> observed,
        FilterReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var dispositions = report.Decisions.ToDictionary(
            d => d.FileName,
            d => d.Disposition,
            StringComparer.OrdinalIgnoreCase);

        return Validate(
            set,
            observed,
            fileName => dispositions.TryGetValue(fileName, out var d) ? d : PackageDisposition.Pending);
    }

    private static Result<bool> Validate(
        ReleasePackageSet set,
        IReadOnlyDictionary<string, string> observed,
        Func<string, PackageDisposition> dispositionOf)
    {
        ArgumentNullException.ThrowIfNull(set);
        ArgumentNullException.ThrowIfNull(observed);

        var errors = new List<ReleaseError>();
        var planned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var package in set.Packages)
        {
            planned.Add(package.FileName);

            var shouldBePresent = dispositionOf(package.FileName) == PackageDisposition.Pending;
            var isPresent = observed.TryGetValue(package.FileName, out var actualHash);

            if (shouldBePresent && !isPresent)
            {
                errors.Add(new ReleaseError(
                    ErrorCodes.PackageFileMissing,
                    $"'{set.Name}' expects '{package.FileName}' to be staged for publication, " +
                    "but it is not present."));
                continue;
            }

            if (!shouldBePresent && isPresent)
            {
                errors.Add(new ReleaseError(
                    ErrorCodes.PackageFileUnexpected,
                    $"'{package.FileName}' was withheld from publication but is still staged; " +
                    "it would be pushed by the publish glob."));
                continue;
            }

            if (isPresent && !string.Equals(actualHash, package.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(new ReleaseError(
                    ErrorCodes.PackageHashMismatch,
                    $"'{package.FileName}' has hash '{actualHash}' but the plan recorded " +
                    $"'{package.Sha256}'. It is not the file that was validated."));
            }
        }

        // Anything in the directory the plan does not mention would be published unreviewed.
        foreach (var fileName in observed.Keys.Where(f => !planned.Contains(f)).Order(StringComparer.Ordinal))
        {
            errors.Add(new ReleaseError(
                ErrorCodes.PackageFileUnexpected,
                $"Staging directory for '{set.Name}' contains '{fileName}', which the release " +
                "plan does not list."));
        }

        return errors.Count > 0 ? Result<bool>.Failure(errors) : Result<bool>.Success(true);
    }
}

/// <summary>
/// Decides whether a release is fully indexed on NuGet.org. Pure: the caller polls.
/// </summary>
public static class VerificationEvaluator
{
    /// <summary>
    /// Returns the planned packages that are not yet resolvable on NuGet.org.
    /// </summary>
    /// <remarks>
    /// Every package in the plan is required, including ones <c>filter</c> removed: those
    /// were removed precisely because they were already live, so their absence here would
    /// mean that claim was wrong. An identity with no availability result is treated as
    /// missing rather than assumed present.
    /// </remarks>
    public static IReadOnlyList<PlannedPackage> GetMissing(
        IEnumerable<PlannedPackage> packages,
        IReadOnlyDictionary<string, bool> availability)
    {
        ArgumentNullException.ThrowIfNull(packages);
        ArgumentNullException.ThrowIfNull(availability);

        return [.. packages.Where(p =>
            !availability.TryGetValue(p.IdentityKey, out var isPublished) || !isPublished)];
    }

    /// <summary>True when every planned package is indexed.</summary>
    public static bool IsComplete(
        IEnumerable<PlannedPackage> packages,
        IReadOnlyDictionary<string, bool> availability) =>
        GetMissing(packages, availability).Count == 0;

    /// <summary>Formats the failure message used when the verification deadline expires.</summary>
    public static string DescribeMissing(IReadOnlyList<PlannedPackage> missing) =>
        $"The following packages are not available from NuGet.org: " +
        $"{string.Join(", ", missing.Select(p => $"{p.Id} {p.Version}").Order(StringComparer.Ordinal))}";
}
