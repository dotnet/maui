using Xunit;

namespace DotNet.Release.Tests;

public class MaestroBuildRegistryTests
{
    private static readonly RepositoryId Skia = RepositoryId.Parse("dotnet/skiasharp").Value;

    // ---- resolution by BAR ID ----

    [Fact]
    public async Task Get_by_bar_id_returns_the_single_build()
    {
        var fake = new FakeBuilds(getById: id => BuildFactory.Create(id: id, channels: [(1648, ".NET Libraries")]));

        var builds = await new MaestroBuildRegistry(fake).GetBuildAsync(328857, CancellationToken.None);

        Assert.Equal(328857, Assert.Single(builds).Id);
    }

    /// <summary>
    /// A missing BAR build is a lookup result, not a transport failure. The adapter returns
    /// an empty result so policy reports BAR_BUILD_NOT_FOUND with release context.
    /// </summary>
    [Fact]
    public async Task Http_404_becomes_an_empty_result_rather_than_an_exception()
    {
        var fake = new FakeBuilds(throwOnGet: BuildFactory.NotFound());

        var builds = await new MaestroBuildRegistry(fake).GetBuildAsync(1, CancellationToken.None);

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
            () => new MaestroBuildRegistry(fake).GetBuildAsync(1, CancellationToken.None));
    }

    [Fact]
    public async Task A_null_build_response_is_an_empty_result()
    {
        var fake = new FakeBuilds(getById: _ => null);

        Assert.Empty(await new MaestroBuildRegistry(fake).GetBuildAsync(1, CancellationToken.None));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Non_positive_bar_ids_are_rejected_without_calling_the_service(int barBuildId)
    {
        var fake = new FakeBuilds(getById: _ => throw new InvalidOperationException("should not be called"));

        Assert.Empty(await new MaestroBuildRegistry(fake).GetBuildAsync(barBuildId, CancellationToken.None));
    }

    [Fact]
    public async Task Asset_locations_are_not_requested_because_the_drop_is_gathered_by_darc()
    {
        var fake = new FakeBuilds(getById: id => BuildFactory.Create(id: id));

        await new MaestroBuildRegistry(fake).GetBuildAsync(1, CancellationToken.None);

        Assert.False(fake.LastIncludeAssetLocation);
    }

    // ---- resolution by repository + commit ----

    [Fact]
    public async Task Get_by_repository_and_commit_queries_bar_with_the_github_url()
    {
        var fake = new FakeBuilds(list: (_, _) => [BuildFactory.Create()]);

        await new MaestroBuildRegistry(fake).GetBuildsAsync(Skia, BuildFactory.Commit, CancellationToken.None);

        Assert.Equal("https://github.com/dotnet/skiasharp", fake.LastRepository);
        Assert.Equal(BuildFactory.Commit, fake.LastCommit);
    }

    [Fact]
    public async Task Commit_is_trimmed_before_the_query()
    {
        var fake = new FakeBuilds(list: (_, _) => []);

        await new MaestroBuildRegistry(fake).GetBuildsAsync(Skia, $"  {BuildFactory.Commit}\n", CancellationToken.None);

        Assert.Equal(BuildFactory.Commit, fake.LastCommit);
    }

    /// <summary>
    /// The service only eager-loads <c>BuildChannels</c> when <c>loadCollections</c> is true:
    /// <code>if (loadCollections ?? false) { query = query.Include(b => b.BuildChannels)... }</code>
    /// With it false, <c>Build.Channels</c> is empty and every required-channel check fails
    /// with <c>BAR_CHANNEL_MISSING</c> while BAR shows the build correctly assigned — a
    /// failure whose message points the operator at the wrong thing entirely.
    /// </summary>
    [Fact]
    public async Task Channels_are_requested_because_the_service_omits_them_otherwise()
    {
        var fake = new FakeBuilds(list: (_, _) => [BuildFactory.Create()]);

        await new MaestroBuildRegistry(fake).GetBuildsAsync(Skia, BuildFactory.Commit, CancellationToken.None);

        Assert.True(fake.LastLoadCollections);
    }

    /// <summary>
    /// Repository-and-commit lookup must load channel collections so policy can verify the
    /// required channel.
    /// </summary>
    [Fact]
    public async Task Required_channel_verification_succeeds_on_the_repository_and_commit_path()
    {
        var policy = ReleasePolicy.Parse("""
        {
          "schemaVersion": 1,
          "repositories": {
            "dotnet/skiasharp": { "workload": false, "channel": { "name": ".NET Libraries", "id": 1648 } }
          }
        }
        """).Value;

        var fake = new FakeBuilds(list: (_, _) => [BuildFactory.Create(channels: [(1648, ".NET Libraries")])]);

        var builds = await new MaestroBuildRegistry(fake).GetBuildsAsync(Skia, BuildFactory.Commit, CancellationToken.None);

        var resolved = BuildResolver.Resolve(
            new ReleaseRequest(Skia, BuildFactory.Commit, null),
            policy.GetRepository(Skia).Value,
            builds,
            DateTimeOffset.UnixEpoch,
            "1.0.0-test");

        Assert.True(resolved.IsSuccess, string.Join("; ", resolved.Errors));
        Assert.Equal(new ChannelReference(".NET Libraries", 1648), resolved.Value.Channel);
    }

    [Fact]
    public async Task Every_page_entry_is_returned_so_Core_can_reject_an_ambiguous_match()
    {
        var fake = new FakeBuilds(list: (_, _) => [BuildFactory.Create(id: 1), BuildFactory.Create(id: 2)]);

        var builds = await new MaestroBuildRegistry(fake).GetBuildsAsync(Skia, BuildFactory.Commit, CancellationToken.None);

        Assert.Equal([1, 2], builds.Select(b => b.Id));
    }

    [Fact]
    public async Task No_matching_build_is_an_empty_result()
    {
        var fake = new FakeBuilds(list: (_, _) => []);

        Assert.Empty(await new MaestroBuildRegistry(fake).GetBuildsAsync(Skia, BuildFactory.Commit, CancellationToken.None));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Blank_commit_is_rejected_without_calling_the_service(string commit)
    {
        var fake = new FakeBuilds(list: (_, _) => throw new InvalidOperationException("should not be called"));

        Assert.Empty(await new MaestroBuildRegistry(fake).GetBuildsAsync(Skia, commit, CancellationToken.None));
    }

    // ---- end-to-end through build resolution ----

    /// <summary>
    /// The full production path for SkiaSharp build 328857: BAR has no GitHub URL, the build
    /// is reachable only by ID, and identity is still established and verified.
    /// </summary>
    [Fact]
    public async Task Null_github_repository_resolves_and_verifies_end_to_end()
    {
        var policy = ReleasePolicy.Parse("""
        {
          "schemaVersion": 1,
          "repositories": {
            "dotnet/skiasharp": { "workload": false, "channel": { "name": ".NET Libraries", "id": 1648 } }
          }
        }
        """).Value;

        var fake = new FakeBuilds(getById: id => BuildFactory.Create(
            id: id,
            gitHubRepository: null,
            azureDevOpsRepository: "https://dev.azure.com/dnceng/internal/_git/dotnet-skiasharp",
            channels: [(1648, ".NET Libraries")]));

        var builds = await new MaestroBuildRegistry(fake).GetBuildAsync(328857, CancellationToken.None);

        var resolved = BuildResolver.Resolve(
            new ReleaseRequest(Skia, BuildFactory.Commit, BarBuildId: 328857),
            policy.GetRepository(Skia).Value,
            builds,
            DateTimeOffset.UnixEpoch,
            "1.0.0-test");

        Assert.True(resolved.IsSuccess, string.Join("; ", resolved.Errors));
        Assert.Equal("dotnet/skiasharp", resolved.Value.Repository);
        Assert.Equal(RepositoryOrigin.AzureDevOpsMirrorConvention, resolved.Value.RepositoryOrigin);
        Assert.Equal(328857, resolved.Value.BarBuildId);
    }

    [Fact]
    public void The_registry_requires_a_builds_endpoint()
    {
        Assert.Throws<ArgumentNullException>(() => new MaestroBuildRegistry(null!));
    }
}
