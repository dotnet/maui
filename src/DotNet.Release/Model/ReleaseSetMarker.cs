using System.Text.Json.Serialization;

namespace DotNet.Release;

/// <summary>
/// A self-describing marker written into each staged package set's directory.
/// </summary>
/// <remarks>
/// <c>filter</c> and <c>verify</c> receive both a package-set name and an artifact directory.
/// The marker binds that directory to the requested set, BAR build, and commit. This produces
/// an explicit <see cref="ErrorCodes.PackageSetMismatch"/> for cross-set or stale-artifact
/// wiring instead of a generic missing-file failure. Package identities remain sourced only
/// from the hashed release plan, so marker changes can only fail a release.
/// </remarks>
internal sealed record ReleaseSetMarker
{
    /// <summary>The marker file name, written beside the staged packages.</summary>
    public const string FileName = "release-set.json";

    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; init; } = 1;

    [JsonPropertyName("setName")]
    public required string SetName { get; init; }

    [JsonPropertyName("artifactName")]
    public required string ArtifactName { get; init; }

    /// <summary>Ties the directory to a specific release, not merely to a set name.</summary>
    [JsonPropertyName("barBuildId")]
    public required int BarBuildId { get; init; }

    [JsonPropertyName("commit")]
    public required string Commit { get; init; }

    public static ReleaseSetMarker For(ReleasePackageSet set, ResolvedRelease source)
    {
        ArgumentNullException.ThrowIfNull(set);
        ArgumentNullException.ThrowIfNull(source);

        return new ReleaseSetMarker
        {
            SetName = set.Name,
            ArtifactName = set.ArtifactName,
            BarBuildId = source.BarBuildId,
            Commit = source.Commit,
        };
    }

    /// <summary>
    /// Asserts that a staged directory declares itself to be the set that was requested,
    /// for the release that was requested.
    /// </summary>
    public static Result<bool> Validate(ReleaseSetMarker? marker, ReleasePackageSet expected, ResolvedRelease source)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(source);

        if (marker is null)
        {
            return Result<bool>.Failure(
                ErrorCodes.PackageSetMismatch,
                $"The staged directory for '{expected.ArtifactName}' has no {FileName}, so it " +
                "cannot be confirmed to be the set that was requested.");
        }

        if (marker.SchemaVersion != 1)
        {
            return Result<bool>.Failure(
                ErrorCodes.PackageSetMismatch,
                $"Unsupported {FileName} schemaVersion '{marker.SchemaVersion}'; expected 1.");
        }

        if (!string.Equals(marker.ArtifactName, expected.ArtifactName, StringComparison.OrdinalIgnoreCase))
        {
            return Result<bool>.Failure(
                ErrorCodes.PackageSetMismatch,
                $"The staged directory declares itself to be '{marker.ArtifactName}' " +
                $"({marker.SetName}), but '{expected.ArtifactName}' ({expected.Name}) was " +
                "requested. The publish stage is wired to the wrong package set.");
        }

        // A directory from a different release would otherwise pass the name check and then
        // fail as a pile of missing files.
        if (marker.BarBuildId != source.BarBuildId ||
            !string.Equals(marker.Commit, source.Commit, StringComparison.OrdinalIgnoreCase))
        {
            return Result<bool>.Failure(
                ErrorCodes.PackageSetMismatch,
                $"The staged directory for '{expected.ArtifactName}' was produced for BAR build " +
                $"{marker.BarBuildId} at commit '{marker.Commit}', but the release plan is for " +
                $"BAR build {source.BarBuildId} at commit '{source.Commit}'.");
        }

        return Result<bool>.Success(true);
    }
}
