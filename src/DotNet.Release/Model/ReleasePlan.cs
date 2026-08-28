using System.Text.Json.Serialization;

namespace DotNet.Release;

/// <summary>A package identity read out of a gathered drop, before any policy is applied.</summary>
/// <remarks>
/// Produced by <see cref="NupkgIdentityReader"/>. <see cref="NormalizedVersion"/> is the
/// form accepted by NuGet availability queries.
/// </remarks>
internal sealed record DropPackage(
    string FileName,
    string Id,
    string Version,
    string NormalizedVersion,
    string Sha256);

/// <summary>A package that is going to be released.</summary>
internal sealed record PlannedPackage
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("version")]
    public required string Version { get; init; }

    [JsonPropertyName("normalizedVersion")]
    public required string NormalizedVersion { get; init; }

    [JsonPropertyName("fileName")]
    public required string FileName { get; init; }

    /// <summary>
    /// SHA-256 of the staged nupkg, used to verify the publish input across the job boundary.
    /// </summary>
    [JsonPropertyName("sha256")]
    public required string Sha256 { get; init; }

    /// <summary>Case-insensitive lookup key for NuGet.org availability checks.</summary>
    [JsonIgnore]
    public string IdentityKey => $"{Id.ToLowerInvariant()}/{NormalizedVersion.ToLowerInvariant()}";
}

/// <summary>One gated, independently published group of packages.</summary>
internal sealed record ReleasePackageSet
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Publication order. Workload packs are 0 and manifests are 1, because manifests
    /// reference packs. The pipeline also enforces this as a stage dependency; recording it
    /// here makes the intent data rather than YAML topology, and testable.
    /// </summary>
    [JsonPropertyName("order")]
    public required int Order { get; init; }

    [JsonPropertyName("artifactName")]
    public required string ArtifactName { get; init; }

    [JsonPropertyName("packages")]
    public required IReadOnlyList<PlannedPackage> Packages { get; init; }
}

/// <summary>The workload-set channel a workload build is promoted to.</summary>
internal sealed record WorkloadSetTarget(
    [property: JsonPropertyName("band")] int Band,
    [property: JsonPropertyName("channel")] string Channel,
    [property: JsonPropertyName("feed")] string Feed);

/// <summary>
/// The single contract artifact, serialized as <c>release-plan.json</c>.
/// </summary>
/// <remarks>
/// The plan is immutable after staging. <c>release filter</c> records runtime dispositions
/// in a sidecar so the plan hash remains valid through approval, publishing, and verification.
/// </remarks>
internal sealed record ReleasePlan
{
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; init; } = 1;

    [JsonPropertyName("toolVersion")]
    public required string ToolVersion { get; init; }

    [JsonPropertyName("createdUtc")]
    public required DateTimeOffset CreatedUtc { get; init; }

    [JsonPropertyName("source")]
    public required ResolvedRelease Source { get; init; }

    [JsonPropertyName("workloadSet")]
    public WorkloadSetTarget? WorkloadSet { get; init; }

    [JsonPropertyName("sets")]
    public required IReadOnlyList<ReleasePackageSet> Sets { get; init; }

    /// <summary>Every package across every set, in publication order.</summary>
    public IEnumerable<PlannedPackage> AllPackages =>
        Sets.OrderBy(s => s.Order).SelectMany(s => s.Packages);
}
