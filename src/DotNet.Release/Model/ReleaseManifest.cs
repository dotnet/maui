using System.Text.Json.Serialization;

namespace DotNet.Release;

/// <summary>The workload-set channel a workload build is promoted to.</summary>
internal sealed record WorkloadSetTarget(
    [property: JsonPropertyName("band")] int Band,
    [property: JsonPropertyName("channel")] string Channel,
    [property: JsonPropertyName("feed")] string Feed);

/// <summary>
/// The verified source of a release.
/// </summary>
internal sealed record ReleaseSource
{
    [JsonPropertyName("repository")]
    public required string Repository { get; init; }

    [JsonPropertyName("repositoryUrl")]
    public required string RepositoryUrl { get; init; }

    [JsonPropertyName("commit")]
    public required string Commit { get; init; }

    [JsonPropertyName("barBuildId")]
    public required int BarBuildId { get; init; }

    [JsonPropertyName("workload")]
    public required bool Workload { get; init; }

    [JsonPropertyName("channel")]
    public ChannelReference? Channel { get; init; }
}

/// <summary>The immutable contract artifact serialized as <c>release-manifest.json</c>.</summary>
internal sealed record ReleaseManifest
{
    [JsonPropertyName("toolVersion")]
    public required string ToolVersion { get; init; }

    [JsonPropertyName("createdUtc")]
    public required DateTimeOffset CreatedUtc { get; init; }

    [JsonPropertyName("source")]
    public required ReleaseSource Source { get; init; }

    [JsonPropertyName("workloadSet")]
    public WorkloadSetTarget? WorkloadSet { get; init; }

    [JsonPropertyName("sets")]
    public required IReadOnlyList<ReleasePackageSet> Sets { get; init; }

    /// <summary>Every package across every set, in publication order.</summary>
    public IEnumerable<PlannedPackage> AllPackages =>
        Sets.OrderBy(s => s.Order).SelectMany(s => s.Packages);
}
