using Xunit;

namespace DotNet.Release.Tests;

public class MaestroClientTests
{
    private static readonly RepositoryId Skia = RepositoryId.Parse("mono/skiasharp");

    // ---- resolution by BAR ID ----

    [Fact]
    public async Task Get_by_bar_id_returns_the_single_build()
    {
        var fake = new FakeBuilds(getById: id => BuildFactory.Create(id: id, channels: [(1648, ".NET Libraries")]));

        var builds = await new MaestroClient(fake).GetBuildAsync(328857, CancellationToken.None);

        Assert.Equal(328857, Assert.Single(builds).Id);
    }

    /// <summary>
    /// A missing BAR build is a lookup result, not a transport failure. The adapter returns
    /// an empty result so policy reports the missing build with release context.
    /// </summary>
    [Fact]
    public async Task Http_404_becomes_an_empty_result_rather_than_an_exception()
    {
        var fake = new FakeBuilds(throwOnGet: BuildFactory.NotFound());

        var builds = await new MaestroClient(fake).GetBuildAsync(1, CancellationToken.None);

        Assert.Empty(builds);
    }

    /// <summary>
    /// A server error is not "no such build". Flattening it to an empty result would report
    /// a transient outage as a missing build and send the operator hunting the wrong problem.
    /// </summary>
    [Fact]
    public async Task Other_http_failures_still_surface()
    {
        var fake = new FakeBuilds(throwOnGet: BuildFactory.ServerError());

        await Assert.ThrowsAsync<Microsoft.DotNet.ProductConstructionService.Client.RestApiException>(
            () => new MaestroClient(fake).GetBuildAsync(1, CancellationToken.None));
    }

    [Fact]
    public async Task A_null_build_response_is_an_empty_result()
    {
        var fake = new FakeBuilds(getById: _ => null);

        Assert.Empty(await new MaestroClient(fake).GetBuildAsync(1, CancellationToken.None));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Non_positive_bar_ids_are_rejected_without_calling_the_service(int barBuildId)
    {
        var fake = new FakeBuilds(getById: _ => throw new InvalidOperationException("should not be called"));

        Assert.Empty(await new MaestroClient(fake).GetBuildAsync(barBuildId, CancellationToken.None));
    }

    [Fact]
    public async Task Asset_locations_are_not_requested_because_the_drop_is_gathered_by_darc()
    {
        var fake = new FakeBuilds(getById: id => BuildFactory.Create(id: id));

        await new MaestroClient(fake).GetBuildAsync(1, CancellationToken.None);

        Assert.False(fake.LastIncludeAssetLocation);
    }

    // ---- resolution by repository + commit ----

    [Fact]
    public async Task Repository_and_commit_lookup_loads_every_matching_build_and_channel()
    {
        var fake = new FakeBuilds(list: (_, _) =>
        [
            BuildFactory.Create(id: 1, channels: [(1648, ".NET Libraries")]),
            BuildFactory.Create(id: 2, channels: [(1648, ".NET Libraries")]),
        ]);

        var builds = await new MaestroClient(fake).GetBuildsAsync(BuildFactory.Commit, CancellationToken.None);

        Assert.Equal([1, 2], builds.Select(build => build.Id));
        Assert.Null(fake.LastRepository);
        Assert.Equal(BuildFactory.Commit, fake.LastCommit);
        Assert.True(fake.LastLoadCollections);
        Assert.All(builds, build => Assert.Equal(new ChannelReference(".NET Libraries", 1648), Assert.Single(build.Channels)));
    }

    // ---- end-to-end through build resolution ----

    /// <summary>
    /// The production shape for SkiaSharp build 328857: BAR has no GitHub URL, and identity
    /// is still established and verified from the Azure DevOps mirror.
    /// </summary>
    [Fact]
    public async Task Null_github_repository_resolves_and_verifies_end_to_end()
    {
        var skia = RepositoryId.Parse("mono/skiasharp");
        var policy = ReleasePolicy.Parse("""
        {
          "schemaVersion": 1,
          "repositories": {
            "mono/skiasharp": {
              "workload": false,
              "barRepositoryAliases": [ "dotnet/skiasharp" ],
              "channel": { "name": ".NET Libraries", "id": 1648 }
            }
          }
        }
        """);

        var fake = new FakeBuilds(getById: id => BuildFactory.Create(id: id, gitHubRepository: null,
            azureDevOpsRepository: "https://dev.azure.com/dnceng/internal/_git/mono-skiasharp",
            channels: [(1648, ".NET Libraries")]));

        var builds = await new MaestroClient(fake).GetBuildAsync(328857, CancellationToken.None);

        var resolved = BuildResolver.Resolve(
            new ReleaseRequest(skia, Commit: null, BarBuildId: 328857), policy.GetRepository(skia), builds);

        Assert.Equal("mono/skiasharp", resolved.Repository);
        Assert.Equal(RepositoryOrigin.AzureDevOpsMirrorConvention, resolved.RepositoryOrigin);
        Assert.Equal(328857, resolved.BarBuildId);
    }

    [Fact]
    public void The_registry_requires_a_builds_endpoint()
    {
        Assert.Throws<ArgumentNullException>(() => new MaestroClient(null!));
    }
}
