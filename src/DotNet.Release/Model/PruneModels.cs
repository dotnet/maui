using System.Text.Json.Serialization;

namespace DotNet.Release;

internal enum PackageDisposition
{
    Pending,
    AlreadyPublished,
    PreviouslyAttempted,
}

internal sealed record PruneDecision(
    string FileName,
    string Id,
    string NormalizedVersion,
    PackageDisposition Disposition);

/// <summary>In-memory package dispositions decided by <c>release prune-published</c>.</summary>
internal sealed record PruneReport
{
    public required string SetName { get; init; }

    public required IReadOnlyList<PruneDecision> Decisions { get; init; }

    public IReadOnlyList<string> FilesToRemove =>
    [.. Decisions
            .Where(decision => decision.Disposition != PackageDisposition.Pending)
            .Select(decision => decision.FileName)
    ];

    public int PendingCount =>
        Decisions.Count(decision => decision.Disposition == PackageDisposition.Pending);

    public bool HasPackagesToPublish => PendingCount > 0;
}

/// <summary>Transient machine output from <c>release prune-published</c>.</summary>
internal sealed record PrunePublishedResult(
    [property: JsonPropertyName("pendingPackageCount")] int PendingPackageCount);
