using Xunit;

namespace DotNet.Release.Tests;

/// <summary>
/// Covers the boundary where the BAR client's unannotated <c>string</c> properties become
/// this tool's explicitly-nullable model.
/// </summary>
public class MaestroClientMappingTests
{
    [Fact]
    public void Maps_the_fields_the_release_depends_on()
    {
        var mapped = MaestroClient.Map(BuildFactory.Create(id: 328857, channels: [(1648, ".NET Libraries")]));

        Assert.Equal(328857, mapped.Id);
        Assert.Equal(BuildFactory.Commit, mapped.Commit);
        Assert.Equal("https://github.com/mono/skiasharp", mapped.GitHubRepository);
        Assert.Equal(new ChannelReference(".NET Libraries", 1648), Assert.Single(mapped.Channels));
    }

    /// <summary>
    /// The real production case. BAR stores no GitHub URL when Arcade could not verify the
    /// AzDO-to-GitHub mapping, and the client's type system does not warn about it because
    /// the assembly is not nullable-annotated.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Absent_github_repository_becomes_null_not_empty(string? value)
    {
        var mapped = MaestroClient.Map(BuildFactory.Create(gitHubRepository: value));

        Assert.Null(mapped.GitHubRepository);
    }

    /// <summary>
    /// The exact shape of BAR build 328857, released from `dotnet/skiasharp` in production.
    /// </summary>
    /// <remarks>
    /// Root cause of the null: Arcade's <c>PublishBuildToMaestro</c> derives the GitHub
    /// identity from the AzDO mirror name (<c>dotnet-SkiaSharp</c> → <c>dotnet/skiasharp</c>),
    /// verifies it against the GitHub API, receives a 404, and nulls the field as a
    /// <c>LogMessage</c> rather than an error. So the field the client's type system presents
    /// as non-null is null on a real, released build. This test pins that shape.
    /// </remarks>
    [Fact]
    public void Production_build_328857_has_a_null_github_repository()
    {
        var mapped = MaestroClient.Map(BuildFactory.Create(id: 328857, gitHubRepository: null,
            azureDevOpsRepository: "https://dev.azure.com/dnceng/internal/_git/dotnet-SkiaSharp",
            channels: [(1648, ".NET Libraries")]));

        Assert.Equal(328857, mapped.Id);
        Assert.Null(mapped.GitHubRepository);

        // Identity is still established, from the mirror name, and still verifiable.
        Assert.Equal("dotnet/skiasharp", RepositoryId.FromAzureDevOpsMirror(mapped.AzureDevOpsRepository).FullName);
        Assert.Equal(new ChannelReference(".NET Libraries", 1648), Assert.Single(mapped.Channels));
    }

    [Fact]
    public void Null_github_repository_still_carries_the_azdo_mirror_for_identity_recovery()
    {
        var mapped = MaestroClient.Map(BuildFactory.Create(gitHubRepository: null,
            azureDevOpsRepository: "https://dev.azure.com/dnceng/internal/_git/mono-skiasharp"));

        Assert.Null(mapped.GitHubRepository);
        Assert.Equal("https://dev.azure.com/dnceng/internal/_git/mono-skiasharp", mapped.AzureDevOpsRepository);

        // End to end: the mapped build must resolve through the mirror convention.
        Assert.Equal("mono/skiasharp", RepositoryId.FromAzureDevOpsMirror(mapped.AzureDevOpsRepository).FullName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Absent_azdo_repository_becomes_null(string? value)
    {
        Assert.Null(MaestroClient.Map(BuildFactory.Create(azureDevOpsRepository: value)).AzureDevOpsRepository);
    }

    [Fact]
    public void Values_are_trimmed()
    {
        var mapped = MaestroClient.Map(BuildFactory.Create(commit: $"  {BuildFactory.Commit}  ",
            gitHubRepository: "  https://github.com/mono/skiasharp  ",
            channels: [(1648, "  .NET Libraries  ")]));

        Assert.Equal(BuildFactory.Commit, mapped.Commit);
        Assert.Equal("https://github.com/mono/skiasharp", mapped.GitHubRepository);
        Assert.Equal(".NET Libraries", mapped.Channels[0].Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Absent_commit_becomes_empty_so_verification_fails_rather_than_throws(string? value)
    {
        // Build resolution reports the invalid commit; a NullReferenceException here would be
        // a worse diagnostic than the failure the operator needs.
        Assert.Equal(string.Empty, MaestroClient.Map(BuildFactory.Create(commit: value)).Commit);
    }

    [Fact]
    public void Build_with_no_channels_maps_to_an_empty_list()
    {
        Assert.Empty(MaestroClient.Map(BuildFactory.Create()).Channels);
    }

    /// <summary>
    /// A nameless channel cannot satisfy a required-channel check. Mapping it to "" would
    /// make the failure message claim BAR returned a channel called "", which it did not.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Channels_without_a_usable_name_are_dropped(string? name)
    {
        var mapped = MaestroClient.Map(BuildFactory.Create(channels: [(1648, name), (5172, ".NET 10")]));

        Assert.Equal(new ChannelReference(".NET 10", 5172), Assert.Single(mapped.Channels));
    }

    [Fact]
    public void Multiple_channels_are_all_preserved_with_id_and_name()
    {
        var mapped = MaestroClient.Map(BuildFactory.Create(channels: [(1648, ".NET Libraries"), (5172, ".NET 10")]));

        Assert.Equal(
            [(".NET Libraries", 1648), (".NET 10", 5172)],
            mapped.Channels.Select(c => (c.Name, c.Id)));
    }

    [Fact]
    public void Null_build_is_a_programming_error()
    {
        Assert.Throws<ArgumentNullException>(() => MaestroClient.Map(null!));
    }
}
