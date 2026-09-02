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
internal static class StagedSetIntegrity
{
    /// <summary>
    /// Validates the staging directory against the plan before pruning: every
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
    /// and anything added later — are simply never observed, so
    /// adding one requires no allow-list update. The invariant remains scoped to every
    /// <c>.nupkg</c> file in the directory tree.
    /// </para>
    /// </param>
    public static void ValidateStaged(ReleasePackageSet set,
        IReadOnlyDictionary<string, string> observed) =>
        Validate(set, observed, _ => PackageDisposition.Pending);

    /// <summary>
    /// Validates the staging directory after pruning, using the recorded
    /// dispositions.
    /// </summary>
    public static void ValidateFiltered(ReleasePackageSet set, IReadOnlyDictionary<string, string> observed, PruneReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var dispositions = report.Decisions.ToDictionary(
            d => d.FileName,
            d => d.Disposition, StringComparer.OrdinalIgnoreCase);

        Validate(set, observed,
            fileName => dispositions.TryGetValue(fileName, out var d) ? d : PackageDisposition.Pending);
    }

    private static void Validate(ReleasePackageSet set, IReadOnlyDictionary<string, string> observed, Func<string, PackageDisposition> dispositionOf)
    {
        ArgumentNullException.ThrowIfNull(set);
        ArgumentNullException.ThrowIfNull(observed);

        var errors = new List<string>();
        var planned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var package in set.Packages)
        {
            planned.Add(package.FileName);

            var shouldBePresent = dispositionOf(package.FileName) == PackageDisposition.Pending;
            var isPresent = observed.TryGetValue(package.FileName, out var actualHash);

            if (shouldBePresent && !isPresent)
            {
                errors.Add($"'{set.Name}' expects '{package.FileName}' to be staged for publication, " + "but it is not present.");
                continue;
            }

            if (!shouldBePresent && isPresent)
            {
                errors.Add($"'{package.FileName}' was withheld from publication but is still staged; " + "it would be pushed by the publish glob.");
                continue;
            }

            if (isPresent && !string.Equals(actualHash, package.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"'{package.FileName}' has hash '{actualHash}' but the plan recorded " +
                    $"'{package.Sha256}'. It is not the file that was validated.");
            }
        }

        // Anything in the directory the plan does not mention would be published unreviewed.
        foreach (var fileName in observed.Keys.Where(f => !planned.Contains(f)).Order(StringComparer.Ordinal))
        {
            errors.Add($"Staging directory for '{set.Name}' contains '{fileName}', which the release " + "plan does not list.");
        }

        if (errors.Count > 0)
        {
            throw new DotNetReleaseException(errors);
        }
    }
}
