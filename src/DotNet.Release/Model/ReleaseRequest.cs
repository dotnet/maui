namespace DotNet.Release;

/// <summary>The repository and either commit or BAR build selected by the operator.</summary>
internal sealed record ReleaseRequest(
    RepositoryId Repository,
    string? Commit,
    int? BarBuildId);
