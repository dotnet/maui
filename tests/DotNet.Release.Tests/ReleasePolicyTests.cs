using Xunit;

namespace DotNet.Release.Tests;

public class ReleasePolicyTests
{
    [Fact]
    public void Unlisted_repository_fails_closed()
    {
        var exception = Assert.Throws<DotNetReleaseException>(
            () => TestData.Policy().GetRepository(TestData.Repo("dotnet/runtime")));

        // The message must name the allowed set, or an operator cannot act on the failure.
        Assert.Contains("mono/skiasharp", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Workload_repository_has_no_required_channel()
    {
        var policy = TestData.Policy().GetRepository(TestData.Repo("dotnet/maui"));

        Assert.True(policy.Workload);
        Assert.Null(policy.Channel);
    }

    [Theory]
    [InlineData("mono/skiasharp", ".NET Libraries", 1648)]
    [InlineData("dotnet/android-libraries", ".NET 10", 5172)]
    public void Non_workload_repository_carries_its_required_channel(string repo, string channel, int id)
    {
        var policy = TestData.Policy().GetRepository(TestData.Repo(repo));

        Assert.False(policy.Workload);
        Assert.Equal(new ChannelReference(channel, id), policy.Channel);
    }

    [Fact]
    public void Repository_lookup_is_case_insensitive()
    {
        Assert.Equal("mono/skiasharp", TestData.Policy().GetRepository(TestData.Repo("Mono/SkiaSharp")).Repository.FullName);
    }

    [Fact]
    public void Historical_BAR_repository_alias_is_not_a_separate_releasable_repository()
    {
        var policy = TestData.Policy().GetRepository(TestData.Repo("mono/skiasharp"));

        Assert.Equal([TestData.Repo("dotnet/skiasharp")], policy.BarRepositoryAliases);
        Assert.True(policy.MatchesBarRepository(TestData.Repo("dotnet/skiasharp")));
        Assert.Throws<DotNetReleaseException>(() => TestData.Policy().GetRepository(TestData.Repo("dotnet/skiasharp")));
    }

    [Fact]
    public void Unconfigured_workload_band_fails_closed()
    {
        Assert.Throws<DotNetReleaseException>(() => TestData.Policy().GetWorkloadSet(99));
    }

    [Fact]
    public void Workload_band_resolves_to_channel_and_feed()
    {
        var set = TestData.Policy().GetWorkloadSet(10);

        Assert.Equal(".NET 10 Workload Release", set.Channel);
        Assert.Equal("dotnet10-workloads", set.Feed);
    }

    [Theory]
    [InlineData("not json at all")]
    [InlineData("""{ "schemaVersion": 2, "repositories": { "dotnet/maui": { "workload": true } } }""")]
    [InlineData("""{ "schemaVersion": 1, "repositories": {} }""")]
    [InlineData("""{ "schemaVersion": 1 }""")]
    public void Malformed_policy_is_rejected(string json)
    {
        Assert.Throws<DotNetReleaseException>(() => ReleasePolicy.Parse(json));
    }

    [Fact]
    public void Channel_without_a_positive_id_is_rejected()
    {
        Assert.Throws<DotNetReleaseException>(() => ReleasePolicy.Parse("""
        {
          "schemaVersion": 1,
          "repositories": { "mono/skiasharp": { "workload": false, "channel": { "name": ".NET Libraries" } } }
        }
        """));
    }

    [Fact]
    public void Workload_set_missing_a_feed_is_rejected()
    {
        Assert.Throws<DotNetReleaseException>(() => ReleasePolicy.Parse("""
        {
          "schemaVersion": 1,
          "repositories": { "dotnet/maui": { "workload": true } },
          "workloadSets": { "10": { "channel": ".NET 10 Workload Release" } }
        }
        """));
    }

    [Fact]
    public void Policy_validation_reports_all_actionable_errors()
    {
        var exception = Assert.Throws<DotNetReleaseException>(() => ReleasePolicy.Parse("""
        {
          "schemaVersion": 1,
          "repositories": {
            "mono/skiasharp": { "workload": false, "channel": { "name": "", "id": 0 } },
            "not-a-repository": { "workload": false }
          },
          "workloadSets": {
            "bad-band": { "channel": "", "feed": "" },
            "10": { "channel": ".NET 10 Workload Release", "feed": "" }
          }
        }
        """));

        Assert.Contains("mono/skiasharp", exception.Message, StringComparison.Ordinal);
        Assert.Contains("not-a-repository", exception.Message, StringComparison.Ordinal);
        Assert.Contains("bad-band", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Workload set '10'", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// The checked-in policy is the real one the pipeline will load, so a bad edit must fail
    /// the build rather than a release.
    /// </summary>
    [Fact]
    public void Checked_in_policy_is_valid_and_matches_supported_repositories()
    {
        var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "repositories.json"));

        var policy = ReleasePolicy.Parse(json);
        foreach (var repo in new[] { "dotnet/maui", "dotnet/android", "dotnet/macios" })
        {
            Assert.True(policy.GetRepository(TestData.Repo(repo)).Workload);
        }

        Assert.Equal(new ChannelReference(".NET 10", 5172), policy.GetRepository(TestData.Repo("dotnet/android-libraries")).Channel);

        var skiaSharp = policy.GetRepository(TestData.Repo("mono/skiasharp"));
        Assert.Equal(new ChannelReference(".NET Libraries", 1648), skiaSharp.Channel);
        Assert.True(skiaSharp.MatchesBarRepository(TestData.Repo("dotnet/skiasharp")));

        foreach (var band in new[] { 8, 9, 10, 11 })
        {
            Assert.Equal(band, policy.GetWorkloadSet(band).Band);
        }
    }

    [Theory]
    [InlineData("mono/skiasharp")]
    [InlineData("not-a-repository")]
    public void Invalid_BAR_repository_alias_is_rejected(string alias)
    {
        var exception = Assert.Throws<DotNetReleaseException>(() => ReleasePolicy.Parse($$"""
        {
          "schemaVersion": 1,
          "repositories": {
            "mono/skiasharp": {
              "workload": false,
              "barRepositoryAliases": [ "{{alias}}" ],
              "channel": { "name": ".NET Libraries", "id": 1648 }
            }
          }
        }
        """));

        Assert.Contains("alias", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BAR_repository_alias_cannot_belong_to_multiple_policies()
    {
        var exception = Assert.Throws<DotNetReleaseException>(() => ReleasePolicy.Parse("""
        {
          "schemaVersion": 1,
          "repositories": {
            "mono/skiasharp": {
              "workload": false,
              "barRepositoryAliases": [ "dotnet/skiasharp" ]
            },
            "dotnet/other": {
              "workload": false,
              "barRepositoryAliases": [ "dotnet/skiasharp" ]
            }
          }
        }
        """));

        Assert.Contains("assigned to both", exception.Message, StringComparison.Ordinal);
    }
}
