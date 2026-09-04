using Xunit;

namespace DotNet.Release.Tests;

public class RepositoryIdTests
{
    [Theory]
    [InlineData("dotnet/maui", "dotnet", "maui")]
    [InlineData("DotNet/MAUI", "dotnet", "maui")]
    [InlineData("  mono/skiasharp  ", "mono", "skiasharp")]
    [InlineData("dotnet/android-libraries", "dotnet", "android-libraries")]
    [InlineData("dotnet/repository.name_with-symbols", "dotnet", "repository.name_with-symbols")]
    public void Parse_normalizes_owner_and_name(string value, string owner, string name)
    {
        var id = RepositoryId.Parse(value);

        Assert.Equal(owner, id.Owner);
        Assert.Equal(name, id.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("maui")]
    [InlineData("dotnet/maui/extra")]
    [InlineData("dotnet/")]
    [InlineData("/maui")]
    [InlineData("dot_net/maui")]
    [InlineData("dotnet/my repo")]
    [InlineData("dotnet/repository@github")]
    [InlineData("dotnet /maui")]
    public void Parse_rejects_malformed_input(string? value)
    {
        Assert.Throws<DotNetReleaseException>(() => RepositoryId.Parse(value));
    }

    [Theory]
    [InlineData("https://github.com/mono/skiasharp")]
    [InlineData("https://github.com/mono/skiasharp/")]
    [InlineData("https://github.com/mono/SkiaSharp")]
    [InlineData("https://github.com/mono/skiasharp.git")]
    [InlineData("https://www.github.com/mono/skiasharp")]
    [InlineData("https://github.com/mono/skiasharp?foo=bar")]
    [InlineData("https://github.com/mono/skiasharp#readme")]
    public void FromGitHubUrl_handles_the_variations_BAR_actually_stores(string url)
    {
        var id = RepositoryId.FromGitHubUrl(url);

        Assert.Equal("mono/skiasharp", id.FullName);
    }

    [Theory]
    [InlineData("https://dev.azure.com/dnceng/internal/_git/dotnet-skiasharp")]
    [InlineData("not-a-url")]
    [InlineData(null)]
    public void FromGitHubUrl_rejects_non_github_sources(string? url)
    {
        Assert.Throws<DotNetReleaseException>(() => RepositoryId.FromGitHubUrl(url));
    }

    [Theory]
    [InlineData("https://dev.azure.com/dnceng/internal/_git/mono-SkiaSharp", "mono/skiasharp")]
    [InlineData("https://dev.azure.com/dnceng/internal/_git/dotnet-skiasharp", "dotnet/skiasharp")]
    [InlineData("https://dev.azure.com/dnceng/internal/_git/dotnet-android-libraries", "dotnet/android-libraries")]
    [InlineData("https://dev.azure.com/dnceng/internal/_git/DotNet-Maui", "dotnet/maui")]
    [InlineData("dotnet-maui", "dotnet/maui")]
    public void FromAzureDevOpsMirror_reproduces_the_Arcade_convention(string mirror, string expected)
    {
        var id = RepositoryId.FromAzureDevOpsMirror(mirror);

        Assert.Equal(expected, id.FullName);

        // Only the first '-' separates owner from name; the rest belong to the name.
        Assert.Equal(expected.Split('/')[0], id.Owner);
    }

    [Theory]
    [InlineData("nodashhere")]
    [InlineData("-leading")]
    [InlineData("trailing-")]
    [InlineData("")]
    [InlineData(null)]
    public void FromAzureDevOpsMirror_refuses_to_guess(string? mirror)
    {
        Assert.Throws<DotNetReleaseException>(() => RepositoryId.FromAzureDevOpsMirror(mirror));
    }

    [Fact]
    public void GitHubUrl_round_trips_through_FromGitHubUrl()
    {
        var original = RepositoryId.Parse("dotnet/android-libraries");

        Assert.Equal(original, RepositoryId.FromGitHubUrl(original.GitHubUrl));
    }
}
