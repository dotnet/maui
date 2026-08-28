using Xunit;

namespace DotNet.Release.Tests;

public class PackageVersionsTests
{
    [Theory]
    [InlineData("3.119.0", "3.119.0")]
    [InlineData("3.119.0.0", "3.119.0")]
    [InlineData("1.0", "1.0.0")]
    [InlineData("10.0.0-preview.1.25123.4", "10.0.0-preview.1.25123.4")]
    [InlineData("1.0.0+build.5", "1.0.0")]
    public void Normalize_produces_the_form_NuGet_org_indexes_under(
        string version,
        string expected)
    {
        var normalized = PackageVersions.Normalize(version);

        Assert.True(normalized.IsSuccess, string.Join("; ", normalized.Errors));
        Assert.Equal(expected, normalized.Value);
    }

    [Fact]
    public void Trailing_zero_components_are_dropped()
    {
        Assert.Equal("3.119.0", PackageVersions.Normalize("3.119.0.0").Value);
        Assert.Equal("8.3.1.5", PackageVersions.Normalize("8.3.1.5").Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-version")]
    public void Invalid_versions_fail_closed(string? version)
    {
        var result = PackageVersions.Normalize(version);

        Assert.True(result.IsFailure);
        Assert.True(result.HasError(ErrorCodes.PackageMalformed));
    }

    [Fact]
    public void IsNormalizedForm_detects_a_reader_that_got_it_wrong()
    {
        Assert.True(PackageVersions.IsNormalizedForm("3.119.0.0", "3.119.0"));
        Assert.False(PackageVersions.IsNormalizedForm("3.119.0.0", "3.119.0.0"));
    }
}
