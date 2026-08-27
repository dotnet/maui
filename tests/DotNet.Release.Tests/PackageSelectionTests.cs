using Xunit;

namespace DotNet.Release.Tests;

public class PackageSelectionTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("skip")]
    [InlineData("SKIP")]
    public void ParseList_treats_the_skip_sentinel_as_no_filters(string? value)
    {
        Assert.Empty(PackageGlob.ParseList(value));
    }

    [Fact]
    public void ParseList_splits_trims_and_drops_empties()
    {
        Assert.Equal(
            ["a.nupkg", "b*.nupkg"],
            PackageGlob.ParseList(" a.nupkg ; ; b*.nupkg ;"));
    }

    [Theory]
    [InlineData("SkiaSharp.3.119.0.nupkg", "SkiaSharp.*", true)]
    [InlineData("SkiaSharp.3.119.0.nupkg", "skiasharp.*", true)]
    [InlineData("SkiaSharp.3.119.0.nupkg", "*.nupkg", true)]
    [InlineData("SkiaSharp.3.119.0.nupkg", "HarfBuzz*", false)]
    [InlineData("a.nupkg", "?.nupkg", true)]
    [InlineData("ab.nupkg", "?.nupkg", false)]
    public void IsMatch_supports_star_and_question_case_insensitively(string name, string pattern, bool expected)
    {
        Assert.Equal(expected, PackageGlob.IsMatch(name, pattern));
    }

    [Fact]
    public void Empty_include_list_includes_everything()
    {
        Assert.True(PackageGlob.IsSelected("Anything.nupkg", include: [], exclude: []));
    }

    [Fact]
    public void Exclude_beats_include()
    {
        Assert.False(PackageGlob.IsSelected("SkiaSharp.3.0.0.nupkg", ["SkiaSharp.*"], ["*3.0.0*"]));
    }

    [Theory]
    [InlineData("Microsoft.NET.Sdk.Maui.Manifest-10.0.100.10.0.0.nupkg", true)]
    [InlineData("microsoft.net.sdk.maui.manifest-10.0.100.10.0.0.nupkg", true)]
    [InlineData("Microsoft.Maui.Controls.10.0.0.nupkg", false)]
    [InlineData("Something.Manifest.txt", false)]
    public void Workload_manifests_are_identified_by_the_existing_file_name_convention(string name, bool expected)
    {
        Assert.Equal(expected, PackageClassifier.IsWorkloadManifest(name));
    }

    [Fact]
    public void Workload_band_comes_from_the_manifest_file_name()
    {
        var band = PackageClassifier.GetWorkloadBand([
            "Microsoft.NET.Sdk.Maui.Manifest-10.0.100.10.0.0.nupkg",
            "Microsoft.NET.Sdk.Maui.Manifest-10.0.200.10.0.0.nupkg",
        ]);

        Assert.True(band.IsSuccess, string.Join("; ", band.Errors));
        Assert.Equal(10, band.Value);
    }

    [Fact]
    public void Mixed_workload_bands_fail_closed()
    {
        var band = PackageClassifier.GetWorkloadBand([
            "Microsoft.NET.Sdk.Maui.Manifest-10.0.100.10.0.0.nupkg",
            "Microsoft.NET.Sdk.Maui.Manifest-11.0.100.11.0.0.nupkg",
        ]);

        Assert.True(band.IsFailure);
        Assert.True(band.HasError(ErrorCodes.WorkloadBandAmbiguous));
    }

    [Theory]
    [InlineData("Microsoft.NET.Sdk.Maui.10.0.0.nupkg")]
    [InlineData("Manifest.nupkg")]
    public void Unparseable_manifest_name_fails_closed(string name)
    {
        var band = PackageClassifier.GetWorkloadBand([name]);

        Assert.True(band.IsFailure);
        Assert.True(band.HasError(ErrorCodes.WorkloadBandUnresolved));
    }

    [Fact]
    public void No_manifests_fails_closed()
    {
        Assert.True(PackageClassifier.GetWorkloadBand([]).HasError(ErrorCodes.WorkloadBandUnresolved));
    }
}

public class PackageVersionsTests
{
    [Theory]
    [InlineData("3.119.0", "3.119.0")]
    [InlineData("3.119.0.0", "3.119.0")]
    [InlineData("1.0", "1.0.0")]
    [InlineData("10.0.0-preview.1.25123.4", "10.0.0-preview.1.25123.4")]
    [InlineData("1.0.0+build.5", "1.0.0")]
    public void Normalize_produces_the_form_NuGet_org_indexes_under(string version, string expected)
    {
        var normalized = PackageVersions.Normalize(version);

        Assert.True(normalized.IsSuccess, string.Join("; ", normalized.Errors));
        Assert.Equal(expected, normalized.Value);
    }

    /// <summary>
    /// Normalization drops trailing zero components, which is the case the substring
    /// approach cannot reproduce from a file name that is not already normalized.
    /// </summary>
    /// <remarks>
    /// Not a failure that fires today: feeds serve normalized file names, so the substring
    /// happens to agree. Verified against NuGet.org — four-part versions such as
    /// HarfBuzzSharp 8.3.1.5 are indexed as-is, and none end in a fourth component of .0.
    /// </remarks>
    [Fact]
    public void Trailing_zero_components_are_dropped()
    {
        Assert.Equal("3.119.0", PackageVersions.Normalize("3.119.0.0").Value);

        // A four-part version with a non-zero fourth component is preserved, which is why
        // real packages like HarfBuzzSharp 8.3.1.5 are indexed under that exact string.
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
