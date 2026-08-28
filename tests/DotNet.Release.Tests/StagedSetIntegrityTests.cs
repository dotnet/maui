using Xunit;

namespace DotNet.Release.Tests;

public class StagedSetIntegrityTests
{
    private static readonly PlannedPackage Skia = TestData.Planned("SkiaSharp", "3.119.0");
    private static readonly PlannedPackage HarfBuzz = TestData.Planned("HarfBuzzSharp", "8.3.1");
    private static readonly ReleasePackageSet Set = TestData.Set(HarfBuzz, Skia);

    private static Dictionary<string, string> Files(params PlannedPackage[] packages) =>
        packages.ToDictionary(p => p.FileName, p => p.Sha256, StringComparer.OrdinalIgnoreCase);

    [Fact]
    public void Freshly_staged_directory_matching_the_plan_is_valid()
    {
        Assert.True(StagedSetIntegrity.ValidateStaged(Set, Files(Skia, HarfBuzz)).IsSuccess);
    }

    [Fact]
    public void Companion_file_names_are_not_allow_listed_in_the_rule_itself()
    {
        var files = Files(Skia, HarfBuzz);
        files["release-plan.json"] = TestData.Hash("plan");
        files["release-set.json"] = TestData.Hash("marker");

        var result = StagedSetIntegrity.ValidateStaged(Set, files);

        Assert.True(result.IsFailure);
        Assert.True(result.HasError(ErrorCodes.PackageFileUnexpected));
    }

    [Fact]
    public void A_stray_package_is_rejected()
    {
        var files = Files(Skia, HarfBuzz);
        files["Sneaky.1.0.0.nupkg"] = TestData.Hash("sneaky");

        Assert.True(
            StagedSetIntegrity.ValidateStaged(Set, files)
                .HasError(ErrorCodes.PackageFileUnexpected));
    }

    [Fact]
    public void Missing_pending_file_fails_closed()
    {
        var result = StagedSetIntegrity.ValidateStaged(Set, Files(Skia));

        Assert.True(result.IsFailure);
        Assert.True(result.HasError(ErrorCodes.PackageFileMissing));
    }

    [Fact]
    public void Tampered_file_fails_closed()
    {
        var files = Files(Skia, HarfBuzz);
        files[Skia.FileName] = TestData.Hash("tampered");

        var result = StagedSetIntegrity.ValidateStaged(Set, files);

        Assert.True(result.IsFailure);
        Assert.True(result.HasError(ErrorCodes.PackageHashMismatch));
    }

    [Fact]
    public void Removed_files_are_absent_after_filtering()
    {
        var report = FilterPlanner.Plan(
            Set,
            [],
            TestData.Availability((Skia, true), (HarfBuzz, false))).Value;

        Assert.True(StagedSetIntegrity.ValidateFiltered(Set, Files(HarfBuzz), report).IsSuccess);
    }

    [Fact]
    public void A_withheld_package_still_on_disk_fails_closed()
    {
        var report = FilterPlanner.Plan(
            Set,
            [],
            TestData.Availability((Skia, true), (HarfBuzz, false))).Value;

        var result = StagedSetIntegrity.ValidateFiltered(Set, Files(Skia, HarfBuzz), report);

        Assert.True(result.IsFailure);
        Assert.True(result.HasError(ErrorCodes.PackageFileUnexpected));
    }

    [Fact]
    public void The_invariant_holds_for_every_disposition()
    {
        var report = FilterPlanner.Plan(
            Set,
            [HarfBuzz.FileName],
            TestData.Availability((Skia, true), (HarfBuzz, false))).Value;

        Assert.True(
            StagedSetIntegrity.ValidateFiltered(
                Set,
                new Dictionary<string, string>(),
                report).IsSuccess);
        Assert.Equal(0, report.PendingCount);
    }
}
