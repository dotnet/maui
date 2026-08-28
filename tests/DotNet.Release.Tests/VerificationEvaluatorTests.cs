using Xunit;

namespace DotNet.Release.Tests;

public class VerificationEvaluatorTests
{
    private static readonly PlannedPackage Skia = TestData.Planned("SkiaSharp", "3.119.0");
    private static readonly PlannedPackage HarfBuzz = TestData.Planned("HarfBuzzSharp", "8.3.1");
    private static readonly ReleasePackageSet Set = TestData.Set(HarfBuzz, Skia);

    [Fact]
    public void Complete_when_every_planned_package_is_indexed()
    {
        Assert.True(VerificationEvaluator.IsComplete(
            Set.Packages,
            TestData.Availability((Skia, true), (HarfBuzz, true))));
    }

    [Fact]
    public void Reports_the_packages_that_are_not_indexed_yet()
    {
        var missing = VerificationEvaluator.GetMissing(
            Set.Packages,
            TestData.Availability((Skia, true), (HarfBuzz, false)));

        Assert.Equal("HarfBuzzSharp", Assert.Single(missing).Id);
    }

    [Fact]
    public void An_unknown_identity_counts_as_missing()
    {
        var missing = VerificationEvaluator.GetMissing(
            Set.Packages,
            TestData.Availability((Skia, true)));

        Assert.Equal("HarfBuzzSharp", Assert.Single(missing).Id);
    }

    [Fact]
    public void Failure_message_names_every_missing_identity_in_a_stable_order()
    {
        var missing = VerificationEvaluator.GetMissing(
            Set.Packages,
            new Dictionary<string, bool>());

        Assert.Equal(
            "The following packages are not available from NuGet.org: HarfBuzzSharp 8.3.1, SkiaSharp 3.119.0",
            VerificationEvaluator.DescribeMissing(missing));
    }
}
