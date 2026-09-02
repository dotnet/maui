using Xunit;

namespace DotNet.Release.Tests;

public class PackageSelectionTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseList_treats_blank_as_no_filters(string? value)
    {
        Assert.Empty(PackageGlob.ParseList(value));
    }

    [Fact]
    public void ParseList_splits_trims_and_drops_empties()
    {
        Assert.Equal(
            ["a.nupkg", "b*.nupkg"], PackageGlob.ParseList(" a.nupkg ; ; b*.nupkg ;"));
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

        Assert.Equal(10, band);
    }

    [Fact]
    public void Mixed_workload_bands_fail_closed()
    {
        Assert.Throws<DotNetReleaseException>(() => PackageClassifier.GetWorkloadBand([
            "Microsoft.NET.Sdk.Maui.Manifest-10.0.100.10.0.0.nupkg",
            "Microsoft.NET.Sdk.Maui.Manifest-11.0.100.11.0.0.nupkg",
        ]));
    }

    [Theory]
    [InlineData("Microsoft.NET.Sdk.Maui.10.0.0.nupkg")]
    [InlineData("Manifest.nupkg")]
    public void Unparseable_manifest_name_fails_closed(string name)
    {
        Assert.Throws<DotNetReleaseException>(() => PackageClassifier.GetWorkloadBand([name]));
    }

    [Fact]
    public void No_manifests_fails_closed()
    {
        Assert.Throws<DotNetReleaseException>(() => PackageClassifier.GetWorkloadBand([]));
    }
}
