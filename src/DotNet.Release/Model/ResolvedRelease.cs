using System.Text.Json.Serialization;

namespace DotNet.Release;

/// <summary>The repository, commit, and optional BAR build selected by the operator.</summary>
internal sealed record ReleaseRequest(RepositoryId Repository, string Commit, int? BarBuildId);

/// <summary>The verified BAR build and repository policy written by <c>release plan</c>.</summary>
internal sealed record ResolvedRelease
{
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; init; } = 1;

    [JsonPropertyName("toolVersion")]
    public required string ToolVersion { get; init; }

    [JsonPropertyName("createdUtc")]
    public required DateTimeOffset CreatedUtc { get; init; }

    [JsonPropertyName("repository")]
    public required string Repository { get; init; }

    [JsonPropertyName("repositoryUrl")]
    public required string RepositoryUrl { get; init; }

    [JsonPropertyName("commit")]
    public required string Commit { get; init; }

    [JsonPropertyName("barBuildId")]
    public required int BarBuildId { get; init; }

    [JsonPropertyName("repositoryOrigin")]
    public required RepositoryOrigin RepositoryOrigin { get; init; }

    [JsonPropertyName("workload")]
    public required bool Workload { get; init; }

    [JsonPropertyName("channel")]
    public ChannelReference? Channel { get; init; }
}
