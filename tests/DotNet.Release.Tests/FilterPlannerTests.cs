using Xunit;

namespace DotNet.Release.Tests;

public class FilterPlannerTests
{
    private static readonly PlannedPackage Skia = TestData.Planned("SkiaSharp", "3.119.0");
    private static readonly PlannedPackage HarfBuzz = TestData.Planned("HarfBuzzSharp", "8.3.1");

    private static readonly ReleasePackageSet Set = TestData.Set(HarfBuzz, Skia);

    [Fact]
    public void Packages_not_on_nuget_org_stay_pending()
    {
        var report = FilterPlanner.Plan(Set, [], TestData.Availability((Skia, false), (HarfBuzz, false)));

        Assert.True(report.IsSuccess, string.Join("; ", report.Errors));
        Assert.Equal(2, report.Value.PendingCount);
        Assert.True(report.Value.HasPackagesToPublish);
        Assert.Empty(report.Value.FilesToRemove);
    }

    /// <summary>
    /// The 1ES NuGet task treats HTTP 409 as fatal even with allowPackageConflicts, so
    /// already-published packages must be removed from the push set rather than retried.
    /// </summary>
    [Fact]
    public void Already_published_packages_are_removed_from_the_push_set()
    {
        var report = FilterPlanner.Plan(Set, [], TestData.Availability((Skia, true), (HarfBuzz, false)));

        Assert.True(report.IsSuccess, string.Join("; ", report.Errors));
        Assert.Equal(1, report.Value.PendingCount);
        Assert.Equal([Skia.FileName], report.Value.FilesToRemove);

        Assert.Equal(
            PackageDisposition.AlreadyPublished,
            report.Value.Decisions.Single(d => d.FileName == Skia.FileName).Disposition);
    }

    [Fact]
    public void Everything_published_means_the_publish_task_can_be_skipped()
    {
        var report = FilterPlanner.Plan(Set, [], TestData.Availability((Skia, true), (HarfBuzz, true)));

        Assert.True(report.IsSuccess);
        Assert.Equal(0, report.Value.PendingCount);
        Assert.False(report.Value.HasPackagesToPublish);
    }

    /// <summary>
    /// NuGet.org can reserve a version and return 409 before it is visible through the
    /// flat-container API, so recovery filters are honoured before the feed is consulted.
    /// </summary>
    [Fact]
    public void Recovery_filters_win_over_the_feed_query()
    {
        var report = FilterPlanner.Plan(
            Set,
            [Skia.FileName],
            TestData.Availability((Skia, false), (HarfBuzz, false)));

        Assert.True(report.IsSuccess, string.Join("; ", report.Errors));
        Assert.Equal(
            PackageDisposition.PreviouslyAttempted,
            report.Value.Decisions.Single(d => d.FileName == Skia.FileName).Disposition);
        Assert.Equal(1, report.Value.PendingCount);
    }

    [Fact]
    public void Recovery_filters_support_wildcards()
    {
        var report = FilterPlanner.Plan(Set, ["SkiaSharp.*"], TestData.Availability((Skia, false), (HarfBuzz, false)));

        Assert.True(report.IsSuccess, string.Join("; ", report.Errors));
        Assert.Equal([Skia.FileName], report.Value.FilesToRemove);
    }

    /// <summary>
    /// A mistyped recovery filter would publish the package the operator meant to withhold,
    /// which is the exact failure the filter exists to prevent.
    /// </summary>
    [Fact]
    public void Recovery_filter_matching_nothing_fails_closed()
    {
        var report = FilterPlanner.Plan(Set, ["Typo.*"], TestData.Availability((Skia, false), (HarfBuzz, false)));

        Assert.True(report.IsFailure);
        Assert.True(report.HasError(ErrorCodes.FilterUnmatched));
        Assert.Contains("Typo.*", report.Errors[0].Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Missing_availability_is_never_treated_as_unpublished()
    {
        var report = FilterPlanner.Plan(Set, [], TestData.Availability((Skia, false)));

        Assert.True(report.IsFailure);
    }

    [Fact]
    public void Identity_key_is_case_insensitive_on_both_id_and_version()
    {
        var upper = TestData.Planned("SKIASHARP", "3.119.0");

        Assert.Equal(Skia.IdentityKey, upper.IdentityKey);
    }

    [Fact]
    public void Report_records_a_decision_for_every_planned_package()
    {
        var report = FilterPlanner.Plan(Set, [], TestData.Availability((Skia, true), (HarfBuzz, false)));

        Assert.Equal(Set.Packages.Count, report.Value.Decisions.Count);
    }
}

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

    /// <summary>
    /// The unexpected-file rule is scoped by extension where files are enumerated, not by
    /// allow-listing companion names here.
    /// </summary>
    /// <remarks>
    /// This is the anti-brittleness assertion: Core has no knowledge of
    /// <c>release-plan.json</c> or <c>release-set.json</c> and <i>would</i> reject them if
    /// they were ever observed. The safety comes from the caller only ever observing
    /// <c>.nupkg</c> files. Companion files are excluded by enumeration scope, not by a
    /// filename allow-list.
    /// </remarks>
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
    public void A_stray_package_is_still_rejected_when_companions_are_absent()
    {
        var files = Files(Skia, HarfBuzz);
        files["Sneaky.1.0.0.nupkg"] = TestData.Hash("sneaky");

        Assert.True(StagedSetIntegrity.ValidateStaged(Set, files).HasError(ErrorCodes.PackageFileUnexpected));
    }

    [Fact]
    public void Missing_pending_file_fails_closed()
    {
        var result = StagedSetIntegrity.ValidateStaged(Set, Files(Skia));

        Assert.True(result.IsFailure);
        Assert.True(result.HasError(ErrorCodes.PackageFileMissing));
    }

    /// <summary>
    /// 1ES pushes whatever its glob matches, so an unlisted file would be published without
    /// ever having been validated.
    /// </summary>
    [Fact]
    public void Unexpected_file_in_the_staging_directory_fails_closed()
    {
        var files = Files(Skia, HarfBuzz);
        files["Sneaky.1.0.0.nupkg"] = TestData.Hash("sneaky");

        var result = StagedSetIntegrity.ValidateStaged(Set, files);

        Assert.True(result.IsFailure);
        Assert.True(result.HasError(ErrorCodes.PackageFileUnexpected));
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
    public void After_filtering_removed_files_are_expected_to_be_absent()
    {
        var report = FilterPlanner.Plan(Set, [], TestData.Availability((Skia, true), (HarfBuzz, false))).Value;

        Assert.True(StagedSetIntegrity.ValidateFiltered(Set, Files(HarfBuzz), report).IsSuccess);
    }

    [Fact]
    public void A_withheld_package_still_on_disk_fails_closed()
    {
        var report = FilterPlanner.Plan(Set, [], TestData.Availability((Skia, true), (HarfBuzz, false))).Value;

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

        // Both were withheld, for different reasons, so the directory must be empty.
        Assert.True(StagedSetIntegrity.ValidateFiltered(Set, new Dictionary<string, string>(), report).IsSuccess);
        Assert.Equal(0, report.PendingCount);
    }
}

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

    /// <summary>
    /// Verification covers packages that <c>filter</c> removed too: they were removed on the
    /// grounds that they were already live, so their absence means that claim was wrong.
    /// </summary>
    [Fact]
    public void An_unknown_identity_counts_as_missing_rather_than_present()
    {
        var missing = VerificationEvaluator.GetMissing(Set.Packages, TestData.Availability((Skia, true)));

        Assert.Equal("HarfBuzzSharp", Assert.Single(missing).Id);
    }

    [Fact]
    public void Failure_message_names_every_missing_identity_in_a_stable_order()
    {
        var missing = VerificationEvaluator.GetMissing(Set.Packages, new Dictionary<string, bool>());

        Assert.Equal(
            "The following packages are not available from NuGet.org: HarfBuzzSharp 8.3.1, SkiaSharp 3.119.0",
            VerificationEvaluator.DescribeMissing(missing));
    }
}
