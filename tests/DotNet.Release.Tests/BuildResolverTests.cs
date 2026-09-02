using Xunit;

namespace DotNet.Release.Tests;

public class BuildResolverTests
{
    private static ResolvedRelease Resolve(
        IReadOnlyList<BarBuild> candidates,
        ReleaseRequest? request = null,
        string repo = "dotnet/skiasharp") =>
        BuildResolver.Resolve(
            request ?? TestData.Request(repo),
            TestData.RepoPolicy(repo),
            candidates,
            TestData.Now,
            TestData.ToolVersion);

    private static readonly ChannelReference Libraries = new(".NET Libraries", 1648);

    [Fact]
    public void Resolves_a_single_matching_build()
    {
        var result = Resolve([TestData.Build(channels: Libraries)]);

        Assert.Equal(4242, result.BarBuildId);
        Assert.Equal("dotnet/skiasharp", result.Repository);
        Assert.Equal(TestData.Commit, result.Commit);
        Assert.Equal(RepositoryOrigin.GitHubRepository, result.RepositoryOrigin);
        Assert.False(result.Workload);
        Assert.Equal(Libraries, result.Channel);
    }

    [Fact]
    public void No_build_fails_closed_and_points_at_the_bar_id_workaround()
    {
        var exception = Assert.Throws<DotNetReleaseException>(() => Resolve([]));
        Assert.Contains("--bar-id", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Multiple_builds_fail_closed_and_name_them()
    {
        var exception = Assert.Throws<DotNetReleaseException>(() => Resolve([
            TestData.Build(id: 1, channels: Libraries),
            TestData.Build(id: 2, channels: Libraries),
        ]));
        Assert.Contains("1, 2", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Wrong_commit_fails_closed()
    {
        Assert.Throws<DotNetReleaseException>(() =>
            Resolve([TestData.Build(commit: "0000000000000000000000000000000000000000", channels: Libraries)]));
    }

    [Fact]
    public void Wrong_repository_fails_closed()
    {
        Assert.Throws<DotNetReleaseException>(() =>
            Resolve([TestData.Build(gitHubRepository: "https://github.com/dotnet/maui", channels: Libraries)]));
    }

    [Fact]
    public void Missing_required_channel_fails_closed()
    {
        Assert.Throws<DotNetReleaseException>(() =>
            Resolve([TestData.Build(channels: new ChannelReference(".NET 10", 5172))]));
    }

    [Fact]
    public void Channel_with_the_right_name_but_the_wrong_id_fails_closed()
    {
        Assert.Throws<DotNetReleaseException>(() =>
            Resolve([TestData.Build(channels: new ChannelReference(".NET Libraries", 9999))]));
    }

    [Fact]
    public void Channel_name_comparison_is_case_sensitive()
    {
        // Channel policy binds both the exact display name and numeric ID.
        Assert.Throws<DotNetReleaseException>(() =>
            Resolve([TestData.Build(channels: new ChannelReference(".net libraries", 1648))]));
    }

    [Fact]
    public void Workload_repository_needs_no_channel()
    {
        var result = Resolve(
            [TestData.Build(gitHubRepository: "https://github.com/dotnet/maui")],
            TestData.Request("dotnet/maui"),
            "dotnet/maui");

        Assert.True(result.Workload);
        Assert.Null(result.Channel);
    }

    /// <summary>
    /// The real, currently-hit case: BAR nulls gitHubRepository when Arcade could not verify
    /// the AzDO-to-GitHub mapping. Resolution by BAR ID must still establish identity.
    /// </summary>
    [Fact]
    public void Null_github_repository_falls_back_to_the_mirror_convention()
    {
        var result = Resolve([TestData.Build(
            gitHubRepository: null,
            azureDevOpsRepository: "https://dev.azure.com/dnceng/internal/_git/dotnet-skiasharp",
            channels: Libraries)]);

        Assert.Equal("dotnet/skiasharp", result.Repository);

        // The audit trail must state which path established identity.
        Assert.Equal(RepositoryOrigin.AzureDevOpsMirrorConvention, result.RepositoryOrigin);
    }

    [Fact]
    public void Mirror_fallback_still_verifies_the_repository()
    {
        Assert.Throws<DotNetReleaseException>(() => Resolve([TestData.Build(
            gitHubRepository: null,
            azureDevOpsRepository: "https://dev.azure.com/dnceng/internal/_git/dotnet-maui",
            channels: Libraries)]));
    }

    [Fact]
    public void Resolution_by_bar_id_still_verifies_the_commit()
    {
        Assert.Throws<DotNetReleaseException>(() => Resolve(
            [TestData.Build(commit: "1111111111111111111111111111111111111111", channels: Libraries)],
            TestData.Request(barId: 4242)));
    }

    [Fact]
    public void Build_with_no_usable_identity_fails_closed()
    {
        Assert.Throws<DotNetReleaseException>(() => Resolve([TestData.Build(
            gitHubRepository: null,
            azureDevOpsRepository: null,
            channels: Libraries)]));
    }

    [Fact]
    public void Policy_for_a_different_repository_is_rejected()
    {
        Assert.Throws<DotNetReleaseException>(() => BuildResolver.Resolve(
            TestData.Request("dotnet/skiasharp"),
            TestData.RepoPolicy("dotnet/maui"),
            [TestData.Build(channels: Libraries)],
            TestData.Now,
            TestData.ToolVersion));
    }

    [Fact]
    public void Empty_commit_is_rejected()
    {
        Assert.Throws<DotNetReleaseException>(() =>
            Resolve([TestData.Build(channels: Libraries)], TestData.Request(commit: "  ")));
    }
}
