namespace DotNet.Release;

/// <summary>
/// Decides which staged packages still need publishing. Pure: the caller performs the
/// NuGet.org queries and the file deletions.
/// </summary>
internal static class PrunePublishedPlanner
{
    /// <summary>
    /// Plans which files to prune from one package set.
    /// </summary>
    /// <param name="set">The set being published.</param>
    /// <param name="recoveryPatterns">
    /// Recovery filters naming packages already submitted by this release. NuGet.org can
    /// reserve a version and return HTTP 409 before it becomes visible through the
    /// flat-container API, so those packages must be withheld from a second push without
    /// waiting for visibility.
    /// </param>
    /// <param name="availability">Availability keyed by <see cref="PlannedPackage.IdentityKey"/>.</param>
    public static Result<PruneReport> Plan(
        ReleasePackageSet set,
        IReadOnlyList<string> recoveryPatterns,
        IReadOnlyDictionary<string, bool> availability)
    {
        ArgumentNullException.ThrowIfNull(set);
        ArgumentNullException.ThrowIfNull(recoveryPatterns);
        ArgumentNullException.ThrowIfNull(availability);

        // A recovery filter that matches nothing means the operator mistyped a package name,
        // and silently publishing the package they meant to withhold is the exact failure
        // the filter exists to prevent. So it is an error, not a no-op.
        var unmatched = recoveryPatterns
            .Where(pattern => !set.Packages.Any(p => PackageGlob.IsMatch(p.FileName, pattern)))
            .ToList();

        if (unmatched.Count > 0)
        {
            return Result<PruneReport>.Failure(
                ErrorCodes.FilterUnmatched,
                $"Recovery filters matched no expected packages in " +
                $"'{set.Name}': {string.Join(", ", unmatched)}.");
        }

        var decisions = new List<PruneDecision>(set.Packages.Count);
        var missingAvailability = new List<ReleaseError>();

        foreach (var package in set.Packages)
        {
            // Recovery filters are honoured before the feed is consulted, because the whole
            // point is that these packages are not yet visible through the feed.
            if (recoveryPatterns.Count > 0 && PackageGlob.IsAnyMatch(package.FileName, recoveryPatterns))
            {
                decisions.Add(Decide(package, PackageDisposition.PreviouslyAttempted));
                continue;
            }

            if (!availability.TryGetValue(package.IdentityKey, out var isPublished))
            {
                missingAvailability.Add(new ReleaseError(
                    ErrorCodes.PackageFileMissing,
                    $"No NuGet.org availability was determined for {package.Id} {package.Version}."));
                continue;
            }

            decisions.Add(Decide(
                package,
                isPublished ? PackageDisposition.AlreadyPublished : PackageDisposition.Pending));
        }

        return missingAvailability.Count > 0
            ? Result<PruneReport>.Failure(missingAvailability)
            : Result<PruneReport>.Success(new PruneReport { SetName = set.Name, Decisions = decisions });
    }

    private static PruneDecision Decide(PlannedPackage package, PackageDisposition disposition) =>
        new(package.FileName, package.Id, package.NormalizedVersion, disposition);
}
