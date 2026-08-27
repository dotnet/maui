using System.Text.Json.Serialization;

namespace DotNet.Release.Core;

/// <summary>What <c>release filter</c> decided to do with one package.</summary>
public enum PackageDisposition
{
    /// <summary>Not on NuGet.org. Its file stays staged and will be pushed.</summary>
    Pending,

    /// <summary>Proven live on NuGet.org. Its file is removed from the push set.</summary>
    AlreadyPublished,

    /// <summary>Matched a recovery filter for a package a previous run already submitted.</summary>
    PreviouslyAttempted,
}

/// <summary>Whether a given identity is currently resolvable on NuGet.org.</summary>
public sealed record PackageAvailability(string IdentityKey, bool IsPublished);

public sealed record FilterDecision(
    [property: JsonPropertyName("fileName")] string FileName,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("normalizedVersion")] string NormalizedVersion,
    [property: JsonPropertyName("disposition")] PackageDisposition Disposition);

/// <summary>
/// The <c>release-filter.json</c> sidecar.
/// </summary>
/// <remarks>
/// Dispositions live here rather than in the plan because <c>filter</c> deletes files, and
/// editing the plan to match would invalidate the hash that pins it across the job
/// boundary. The plan stays immutable; this records what happened to it.
/// </remarks>
public sealed record FilterReport
{
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; init; } = 1;

    [JsonPropertyName("setName")]
    public required string SetName { get; init; }

    [JsonPropertyName("decisions")]
    public required IReadOnlyList<FilterDecision> Decisions { get; init; }

    /// <summary>Files the caller must delete so the 1ES push glob sees only pending packages.</summary>
    [JsonIgnore]
    public IReadOnlyList<string> FilesToRemove =>
        [.. Decisions.Where(d => d.Disposition != PackageDisposition.Pending).Select(d => d.FileName)];

    [JsonPropertyName("pendingCount")]
    public int PendingCount => Decisions.Count(d => d.Disposition == PackageDisposition.Pending);

    /// <summary>False when everything is already live, so the publish task can be skipped entirely.</summary>
    [JsonIgnore]
    public bool HasPackagesToPublish => PendingCount > 0;
}

/// <summary>
/// Decides which staged packages still need publishing. Pure: the caller performs the
/// NuGet.org queries and the file deletions.
/// </summary>
public static class FilterPlanner
{
    /// <summary>
    /// Plans the filtering of one package set.
    /// </summary>
    /// <param name="set">The set being published.</param>
    /// <param name="skipPatterns">
    /// Recovery filters naming packages a previous run already submitted. NuGet.org can
    /// reserve a version and return HTTP 409 before it becomes visible through the
    /// flat-container API, so those packages must be withheld from a second push without
    /// waiting for visibility.
    /// </param>
    /// <param name="availability">Availability keyed by <see cref="PlannedPackage.IdentityKey"/>.</param>
    public static Result<FilterReport> Plan(
        ReleasePackageSet set,
        IReadOnlyList<string> skipPatterns,
        IReadOnlyDictionary<string, bool> availability)
    {
        ArgumentNullException.ThrowIfNull(set);
        ArgumentNullException.ThrowIfNull(skipPatterns);
        ArgumentNullException.ThrowIfNull(availability);

        // A recovery filter that matches nothing means the operator mistyped a package name,
        // and silently publishing the package they meant to withhold is the exact failure
        // the filter exists to prevent. So it is an error, not a no-op.
        var unmatched = skipPatterns
            .Where(pattern => !set.Packages.Any(p => PackageGlob.IsMatch(p.FileName, pattern)))
            .ToList();

        if (unmatched.Count > 0)
        {
            return Result<FilterReport>.Failure(
                ErrorCodes.FilterUnmatched,
                $"Previously attempted package filters matched no expected packages in " +
                $"'{set.Name}': {string.Join(", ", unmatched)}.");
        }

        var decisions = new List<FilterDecision>(set.Packages.Count);
        var missingAvailability = new List<ReleaseError>();

        foreach (var package in set.Packages)
        {
            // Recovery filters are honoured before the feed is consulted, because the whole
            // point is that these packages are not yet visible through the feed.
            if (skipPatterns.Count > 0 && PackageGlob.IsAnyMatch(package.FileName, skipPatterns))
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
            ? Result<FilterReport>.Failure(missingAvailability)
            : Result<FilterReport>.Success(new FilterReport { SetName = set.Name, Decisions = decisions });
    }

    private static FilterDecision Decide(PlannedPackage package, PackageDisposition disposition) =>
        new(package.FileName, package.Id, package.NormalizedVersion, disposition);
}
