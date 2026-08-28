using System.Text.Json.Serialization;

namespace DotNet.Release;

internal enum PackageDisposition
{
    Pending,
    AlreadyPublished,
    PreviouslyAttempted,
}

internal sealed record FilterDecision(
    [property: JsonPropertyName("fileName")] string FileName,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("normalizedVersion")] string NormalizedVersion,
    [property: JsonPropertyName("disposition")] PackageDisposition Disposition);

/// <summary>Package dispositions written by <c>release filter</c>.</summary>
internal sealed record FilterReport
{
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; init; } = 1;

    [JsonPropertyName("setName")]
    public required string SetName { get; init; }

    [JsonPropertyName("decisions")]
    public required IReadOnlyList<FilterDecision> Decisions { get; init; }

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
