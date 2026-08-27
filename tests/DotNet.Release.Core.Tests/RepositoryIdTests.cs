using Xunit;

namespace DotNet.Release.Core.Tests;

public class RepositoryIdTests
{
    [Theory]
    [InlineData("dotnet/maui", "dotnet", "maui")]
    [InlineData("DotNet/MAUI", "dotnet", "maui")]
    [InlineData("  dotnet/skiasharp  ", "dotnet", "skiasharp")]
    [InlineData("dotnet/android-libraries", "dotnet", "android-libraries")]
    public void Parse_normalizes_owner_and_name(string value, string owner, string name)
    {
        var id = RepositoryId.Parse(value);

        Assert.True(id.IsSuccess);
        Assert.Equal(owner, id.Value.Owner);
        Assert.Equal(name, id.Value.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("maui")]
    [InlineData("dotnet/maui/extra")]
    [InlineData("dotnet/")]
    [InlineData("/maui")]
    public void Parse_rejects_malformed_input(string? value)
    {
        var id = RepositoryId.Parse(value);

        Assert.True(id.IsFailure);
        Assert.True(id.HasError(ErrorCodes.RepositoryUnparseable));
    }

    [Theory]
    [InlineData("https://github.com/dotnet/skiasharp")]
    [InlineData("https://github.com/dotnet/skiasharp/")]
    [InlineData("https://github.com/dotnet/SkiaSharp")]
    [InlineData("https://github.com/dotnet/skiasharp.git")]
    [InlineData("https://www.github.com/dotnet/skiasharp")]
    [InlineData("https://github.com/dotnet/skiasharp?foo=bar")]
    [InlineData("https://github.com/dotnet/skiasharp#readme")]
    public void FromGitHubUrl_handles_the_variations_BAR_actually_stores(string url)
    {
        var id = RepositoryId.FromGitHubUrl(url);

        Assert.True(id.IsSuccess, string.Join("; ", id.Errors));
        Assert.Equal("dotnet/skiasharp", id.Value.FullName);
    }

    [Theory]
    [InlineData("https://dev.azure.com/dnceng/internal/_git/dotnet-skiasharp")]
    [InlineData("not-a-url")]
    [InlineData(null)]
    public void FromGitHubUrl_rejects_non_github_sources(string? url)
    {
        Assert.True(RepositoryId.FromGitHubUrl(url).IsFailure);
    }

    [Theory]
    [InlineData("https://dev.azure.com/dnceng/internal/_git/dotnet-skiasharp", "dotnet/skiasharp")]
    [InlineData("https://dev.azure.com/dnceng/internal/_git/dotnet-android-libraries", "dotnet/android-libraries")]
    [InlineData("https://dev.azure.com/dnceng/internal/_git/DotNet-Maui", "dotnet/maui")]
    [InlineData("dotnet-maui", "dotnet/maui")]
    public void FromAzureDevOpsMirror_reproduces_the_Arcade_convention(string mirror, string expected)
    {
        var id = RepositoryId.FromAzureDevOpsMirror(mirror);

        Assert.True(id.IsSuccess, string.Join("; ", id.Errors));
        Assert.Equal(expected, id.Value.FullName);

        // Only the first '-' separates owner from name; the rest belong to the name.
        Assert.Equal("dotnet", id.Value.Owner);
    }

    [Theory]
    [InlineData("nodashhere")]
    [InlineData("-leading")]
    [InlineData("trailing-")]
    [InlineData("")]
    [InlineData(null)]
    public void FromAzureDevOpsMirror_refuses_to_guess(string? mirror)
    {
        var id = RepositoryId.FromAzureDevOpsMirror(mirror);

        Assert.True(id.IsFailure);
        Assert.True(id.HasError(ErrorCodes.BarMirrorNameInvalid));
    }

    [Fact]
    public void GitHubUrl_round_trips_through_FromGitHubUrl()
    {
        var original = RepositoryId.Parse("dotnet/android-libraries").Value;

        Assert.Equal(original, RepositoryId.FromGitHubUrl(original.GitHubUrl).Value);
    }
}
