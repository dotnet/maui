namespace DotNet.Release;

/// <summary>Describes why a manifest package is retained for publishing or withheld.</summary>
internal enum PackageDisposition
{
    /// <summary>The package is not visible on the feed and remains in the publish set.</summary>
    Pending,

    /// <summary>The exact package identity is already visible on the feed.</summary>
    AlreadyPublished,

    /// <summary>An operator recovery filter identifies the package as previously submitted.</summary>
    PreviouslyAttempted,
}

/// <summary>The publication disposition calculated for one package in a manifest set.</summary>
internal sealed record PruneDecision(
    ReleasePackage Package,
    PackageDisposition Disposition);

/// <summary>In-memory package dispositions decided by <c>release prune-published</c>.</summary>
internal sealed record PruneReport
{
    public required IReadOnlyList<PruneDecision> Decisions { get; init; }

    public IReadOnlyList<string> FilesToRemove =>
    [.. Decisions
            .Where(decision => decision.Disposition != PackageDisposition.Pending)
            .Select(decision => decision.Package.FileName)
    ];

    public int PendingCount =>
        Decisions.Count(decision => decision.Disposition == PackageDisposition.Pending);

    public bool HasPackagesToPublish => PendingCount > 0;
}
