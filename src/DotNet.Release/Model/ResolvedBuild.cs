namespace DotNet.Release;

/// <summary>The repository and BAR build selected by the operator.</summary>
internal sealed record ReleaseRequest(
    RepositoryId Repository,
    int BarBuildId);

/// <summary>The verified BAR build and repository policy resolved in memory.</summary>
internal sealed record ResolvedBuild
{
    public required string Repository { get; init; }

    public required string RepositoryUrl { get; init; }

    public required string Commit { get; init; }

    public required int BarBuildId { get; init; }

    public required RepositoryOrigin RepositoryOrigin { get; init; }

    public required bool Workload { get; init; }

    public ChannelReference? Channel { get; init; }
}
