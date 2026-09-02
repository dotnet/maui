namespace DotNet.Release;

/// <summary>
/// Calculates whether each package is pending, already published, or previously attempted.
/// </summary>
/// <remarks>The caller performs NuGet.org queries and deletes withheld local files.</remarks>
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
    /// <param name="availability">Availability keyed by <see cref="ReleasePackage.IdentityKey"/>.</param>
    public static PruneReport Plan(ReleasePackageSet set, IEnumerable<ReleasePackage> releasePackages, IReadOnlyList<string> recoveryPatterns,
        IReadOnlyDictionary<string, bool> availability)
    {
        ArgumentNullException.ThrowIfNull(set);
        ArgumentNullException.ThrowIfNull(releasePackages);
        ArgumentNullException.ThrowIfNull(recoveryPatterns);
        ArgumentNullException.ThrowIfNull(availability);

        var allPackages = releasePackages.ToList();

        // A recovery filter that matches nothing means the operator mistyped a package name,
        // and silently publishing the package they meant to withhold is the exact failure
        // the filter exists to prevent. Match against the complete release so a pack-only
        // filter remains valid when the manifest set is processed, and vice versa.
        var unmatched = recoveryPatterns
            .Where(pattern => !allPackages.Any(p => PackageGlob.IsMatch(p.FileName, pattern))).ToList();

        if (unmatched.Count > 0)
        {
            throw new DotNetReleaseException(
                $"Recovery filters matched no expected packages in the release: {string.Join(", ", unmatched)}.");
        }

        var decisions = new List<PruneDecision>(set.Packages.Count);
        var missingAvailability = new List<string>();

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
                missingAvailability.Add($"No NuGet.org availability was determined for {package.Id} {package.Version}.");
                continue;
            }

            decisions.Add(Decide(package, isPublished ? PackageDisposition.AlreadyPublished : PackageDisposition.Pending));
        }

        if (missingAvailability.Count > 0)
        {
            throw new DotNetReleaseException(missingAvailability);
        }

        return new PruneReport { Decisions = decisions };
    }

    private static PruneDecision Decide(ReleasePackage package, PackageDisposition disposition) =>
        new(package, disposition);
}
