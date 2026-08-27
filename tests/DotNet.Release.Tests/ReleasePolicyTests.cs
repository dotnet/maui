using Xunit;

namespace DotNet.Release.Tests;

public class ReleasePolicyTests
{
    [Fact]
    public void Unlisted_repository_fails_closed()
    {
        var result = TestData.Policy().GetRepository(TestData.Repo("dotnet/runtime"));

        Assert.True(result.IsFailure);
        Assert.True(result.HasError(ErrorCodes.RepositoryNotAllowed));

        // The message must name the allowed set, or an operator cannot act on the failure.
        Assert.Contains("dotnet/skiasharp", result.Errors[0].Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Workload_repository_has_no_required_channel()
    {
        var policy = TestData.Policy().GetRepository(TestData.Repo("dotnet/maui")).Value;

        Assert.True(policy.Workload);
        Assert.Null(policy.Channel);
    }

    [Theory]
    [InlineData("dotnet/skiasharp", ".NET Libraries", 1648)]
    [InlineData("dotnet/android-libraries", ".NET 10", 5172)]
    public void Non_workload_repository_carries_its_required_channel(string repo, string channel, int id)
    {
        var policy = TestData.Policy().GetRepository(TestData.Repo(repo)).Value;

        Assert.False(policy.Workload);
        Assert.Equal(new ChannelReference(channel, id), policy.Channel);
    }

    [Fact]
    public void Repository_lookup_is_case_insensitive()
    {
        Assert.True(TestData.Policy().GetRepository(TestData.Repo("DotNet/SkiaSharp")).IsSuccess);
    }

    /// <summary>
    /// The pipeline decides workload-ness at compile time because it decides which stages
    /// exist. This check is what stops that second source of truth from drifting away from
    /// the policy — a workload repo misclassified as non-workload would publish manifests
    /// and packs through one stage, losing the ordering guarantee entirely.
    /// </summary>
    [Theory]
    [InlineData("dotnet/maui", false)]
    [InlineData("dotnet/skiasharp", true)]
    public void A_pipeline_classification_disagreeing_with_the_policy_fails_closed(string repo, bool expected)
    {
        var policy = TestData.Policy().GetRepository(TestData.Repo(repo)).Value;

        var result = ReleasePolicy.VerifyWorkloadClassification(policy, expected);

        Assert.True(result.IsFailure);
        Assert.True(result.HasError(ErrorCodes.WorkloadMismatch));
        Assert.Contains("must agree", result.Errors[0].Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("dotnet/maui", true)]
    [InlineData("dotnet/skiasharp", false)]
    public void A_matching_classification_is_accepted(string repo, bool expected)
    {
        var policy = TestData.Policy().GetRepository(TestData.Repo(repo)).Value;

        Assert.True(ReleasePolicy.VerifyWorkloadClassification(policy, expected).IsSuccess);
    }

    [Fact]
    public void An_absent_classification_is_not_checked()
    {
        // Local runs need not assert one; only the pipeline does.
        var policy = TestData.Policy().GetRepository(TestData.Repo("dotnet/maui")).Value;

        Assert.True(ReleasePolicy.VerifyWorkloadClassification(policy, null).IsSuccess);
    }

    [Fact]
    public void Unconfigured_workload_band_fails_closed()
    {
        var result = TestData.Policy().GetWorkloadSet(99);

        Assert.True(result.IsFailure);
        Assert.True(result.HasError(ErrorCodes.WorkloadSetNotConfigured));
    }

    [Fact]
    public void Workload_band_resolves_to_channel_and_feed()
    {
        var set = TestData.Policy().GetWorkloadSet(10).Value;

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
        Assert.True(ReleasePolicy.Parse(json).IsFailure);
    }

    [Fact]
    public void Channel_without_a_positive_id_is_rejected()
    {
        var result = ReleasePolicy.Parse("""
        {
          "schemaVersion": 1,
          "repositories": { "dotnet/skiasharp": { "workload": false, "channel": { "name": ".NET Libraries" } } }
        }
        """);

        Assert.True(result.IsFailure);
        Assert.True(result.HasError(ErrorCodes.PolicyInvalid));
    }

    [Fact]
    public void Workload_set_missing_a_feed_is_rejected()
    {
        var result = ReleasePolicy.Parse("""
        {
          "schemaVersion": 1,
          "repositories": { "dotnet/maui": { "workload": true } },
          "workloadSets": { "10": { "channel": ".NET 10 Workload Release" } }
        }
        """);

        Assert.True(result.IsFailure);
        Assert.True(result.HasError(ErrorCodes.PolicyInvalid));
    }

    /// <summary>
    /// The checked-in policy is the real one the pipeline will load, so a bad edit must fail
    /// the build rather than a release.
    /// </summary>
    [Fact]
    public void Checked_in_policy_is_valid_and_matches_the_repositories_released_today()
    {
        var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "repositories.json"));

        var policy = ReleasePolicy.Parse(json);
        Assert.True(policy.IsSuccess, string.Join("; ", policy.Errors));

        foreach (var repo in new[] { "dotnet/maui", "dotnet/android", "dotnet/macios" })
        {
            Assert.True(policy.Value.GetRepository(TestData.Repo(repo)).Value.Workload);
        }

        Assert.Equal(
            new ChannelReference(".NET 10", 5172),
            policy.Value.GetRepository(TestData.Repo("dotnet/android-libraries")).Value.Channel);

        Assert.Equal(
            new ChannelReference(".NET Libraries", 1648),
            policy.Value.GetRepository(TestData.Repo("dotnet/skiasharp")).Value.Channel);

        foreach (var band in new[] { 8, 9, 10, 11 })
        {
            Assert.True(policy.Value.GetWorkloadSet(band).IsSuccess);
        }
    }
}
