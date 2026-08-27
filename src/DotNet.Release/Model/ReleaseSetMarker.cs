using System.Text.Json.Serialization;

namespace DotNet.Release;

/// <summary>
/// A self-describing marker written into each staged package set's directory.
/// </summary>
/// <remarks>
/// <para>
/// This buys back a guarantee the single-plan design gave away, and the history is worth
/// recording because it explains the shape.
/// </para>
/// <para>
/// The pipeline this replaces had no central plan. Each set's directory carried its own
/// <c>expected-packages.json</c>, and the helper script resolved that manifest from inside
/// the directory it was handed — so pointing it at the packs artifact made reasoning about
/// manifests <b>unrepresentable</b>, not merely untested. Cross-set contamination could not
/// be expressed.
/// </para>
/// <para>
/// A single hashed plan is better for supply-chain integrity — one root of trust instead of
/// N — but it means <c>filter</c> and <c>verify</c> must be <i>told</i> which set they are
/// operating on. That replaced a structural guarantee with a passed argument, and passed
/// arguments depend on the caller being right.
/// </para>
/// <para>
/// <c>--set</c> plus <see cref="ErrorCodes.PackageSetNotFound"/> catches a <i>misspelled</i>
/// set. It does not catch a <i>valid but wrong</i> one: asking the packs directory for
/// <c>ReleaseManifests</c> resolves cleanly and only fails later, as
/// <see cref="ErrorCodes.PackageFileMissing"/> — the same error a genuinely broken artifact
/// upload produces. That is the diagnostic failure mode this project rejects elsewhere, so
/// it is rejected here too.
/// </para>
/// <para>
/// The marker makes the directory declare what it is, so a wrong-but-valid set is detected
/// immediately with an unambiguous cause. Tampering with it can only cause a failure, never
/// a silently wrong publish, because the package identities still come from the hashed plan.
/// </para>
/// </remarks>
public sealed record ReleaseSetMarker
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
