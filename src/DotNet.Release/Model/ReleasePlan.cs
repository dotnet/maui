using System.Text.Json.Serialization;

namespace DotNet.Release;

/// <summary>The workload-set channel a workload build is promoted to.</summary>
internal sealed record WorkloadSetTarget(
    [property: JsonPropertyName("band")] int Band,
    [property: JsonPropertyName("channel")] string Channel,
    [property: JsonPropertyName("feed")] string Feed);

/// <summary>
/// The single contract artifact, serialized as <c>release-plan.json</c>.
/// </summary>
/// <remarks>
/// The plan is immutable after staging. <c>release prune-published</c> records dispositions
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
