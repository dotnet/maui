using DotNet.Release.Core;
using Xunit;

namespace DotNet.Release.Maestro.Tests;

/// <summary>
/// Covers the boundary where the BAR client's unannotated <c>string</c> properties become
/// this tool's explicitly-nullable model.
/// </summary>
public class BarBuildMapperTests
{
    [Fact]
    public void Maps_the_fields_the_release_depends_on()
    {
        var mapped = BarBuildMapper.Map(BuildFactory.Create(
            id: 328857,
            channels: [(1648, ".NET Libraries")]));

        Assert.Equal(328857, mapped.Id);
        Assert.Equal(BuildFactory.Commit, mapped.Commit);
        Assert.Equal("https://github.com/dotnet/skiasharp", mapped.GitHubRepository);
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
        var mapped = BarBuildMapper.Map(BuildFactory.Create(gitHubRepository: value));

        Assert.Null(mapped.GitHubRepository);
    }

    [Fact]
    public void Null_github_repository_still_carries_the_azdo_mirror_for_identity_recovery()
    {
        var mapped = BarBuildMapper.Map(BuildFactory.Create(
            gitHubRepository: null,
            azureDevOpsRepository: "https://dev.azure.com/dnceng/internal/_git/dotnet-skiasharp"));

        Assert.Null(mapped.GitHubRepository);
        Assert.Equal("https://dev.azure.com/dnceng/internal/_git/dotnet-skiasharp", mapped.AzureDevOpsRepository);

        // End to end: the mapped build must resolve through the mirror convention.
        Assert.Equal("dotnet/skiasharp", RepositoryId.FromAzureDevOpsMirror(mapped.AzureDevOpsRepository).Value.FullName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Absent_azdo_repository_becomes_null(string? value)
    {
        Assert.Null(BarBuildMapper.Map(BuildFactory.Create(azureDevOpsRepository: value)).AzureDevOpsRepository);
    }

    [Fact]
    public void Values_are_trimmed()
    {
        var mapped = BarBuildMapper.Map(BuildFactory.Create(
            commit: $"  {BuildFactory.Commit}  ",
            gitHubRepository: "  https://github.com/dotnet/skiasharp  ",
            channels: [(1648, "  .NET Libraries  ")]));

        Assert.Equal(BuildFactory.Commit, mapped.Commit);
        Assert.Equal("https://github.com/dotnet/skiasharp", mapped.GitHubRepository);
        Assert.Equal(".NET Libraries", mapped.Channels[0].Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Absent_commit_becomes_empty_so_verification_fails_rather_than_throws(string? value)
    {
        // Core reports BAR_COMMIT_MISMATCH for this; a NullReferenceException here would be
        // a worse diagnostic than the failure the operator actually needs to see.
        Assert.Equal(string.Empty, BarBuildMapper.Map(BuildFactory.Create(commit: value)).Commit);
    }

    [Fact]
    public void Build_with_no_channels_maps_to_an_empty_list()
    {
        Assert.Empty(BarBuildMapper.Map(BuildFactory.Create()).Channels);
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
        var mapped = BarBuildMapper.Map(BuildFactory.Create(channels: [(1648, name), (5172, ".NET 10")]));

        Assert.Equal(new ChannelReference(".NET 10", 5172), Assert.Single(mapped.Channels));
    }

    [Fact]
    public void Multiple_channels_are_all_preserved_with_id_and_name()
    {
        var mapped = BarBuildMapper.Map(BuildFactory.Create(
            channels: [(1648, ".NET Libraries"), (5172, ".NET 10")]));

        Assert.Equal(
            [(".NET Libraries", 1648), (".NET 10", 5172)],
            mapped.Channels.Select(c => (c.Name, c.Id)));
    }

    [Fact]
    public void Null_build_is_a_programming_error()
    {
        Assert.Throws<ArgumentNullException>(() => BarBuildMapper.Map(null!));
    }
}
