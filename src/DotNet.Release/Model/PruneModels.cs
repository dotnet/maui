using System.Text.Json.Serialization;

namespace DotNet.Release;

internal enum PackageDisposition
{
    Pending,
    AlreadyPublished,
    PreviouslyAttempted,
}

internal sealed record PruneDecision(
    [property: JsonPropertyName("fileName")] string FileName,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("normalizedVersion")] string NormalizedVersion,
    [property: JsonPropertyName("disposition")] PackageDisposition Disposition);

/// <summary>Package dispositions written by <c>release prune-published</c>.</summary>
internal sealed record PruneReport
{
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; init; } = 1;

    [JsonPropertyName("setName")]
    public required string SetName { get; init; }

    [JsonPropertyName("decisions")]
    public required IReadOnlyList<PruneDecision> Decisions { get; init; }

    [JsonIgnore]
    public IReadOnlyList<string> FilesToRemove =>
    [
        .. Decisions
            .Where(decision => decision.Disposition != PackageDisposition.Pending)
            .Select(decision => decision.FileName)
    ];

    [JsonPropertyName("pendingCount")]
    public int PendingCount =>
        Decisions.Count(decision => decision.Disposition == PackageDisposition.Pending);

    [JsonIgnore]
    public bool HasPackagesToPublish => PendingCount > 0;
}
