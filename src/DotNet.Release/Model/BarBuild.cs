namespace DotNet.Release;

/// <summary>A BAR channel bound by exact name and numeric ID.</summary>
internal sealed record ChannelReference(string Name, int Id);

/// <summary>The BAR build data required by release policy.</summary>
internal sealed record BarBuild(int Id, string Commit, string? GitHubRepository, string? AzureDevOpsRepository, IReadOnlyList<ChannelReference> Channels);
