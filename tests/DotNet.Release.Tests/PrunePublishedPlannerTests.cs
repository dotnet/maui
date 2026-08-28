using Xunit;

namespace DotNet.Release.Tests;

public class PrunePublishedPlannerTests
{
    private static readonly PlannedPackage Skia = TestData.Planned("SkiaSharp", "3.119.0");
    private static readonly PlannedPackage HarfBuzz = TestData.Planned("HarfBuzzSharp", "8.3.1");

    private static readonly ReleasePackageSet Set = TestData.Set(HarfBuzz, Skia);

    [Fact]
    public void Packages_not_on_nuget_org_stay_pending()
    {
        var report = PrunePublishedPlanner.Plan(Set, [], TestData.Availability((Skia, false), (HarfBuzz, false)));

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
        var report = PrunePublishedPlanner.Plan(Set, [], TestData.Availability((Skia, true), (HarfBuzz, false)));

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
        var report = PrunePublishedPlanner.Plan(Set, [], TestData.Availability((Skia, true), (HarfBuzz, true)));

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
        var report = PrunePublishedPlanner.Plan(
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
        var report = PrunePublishedPlanner.Plan(Set, ["SkiaSharp.*"], TestData.Availability((Skia, false), (HarfBuzz, false)));

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
        var report = PrunePublishedPlanner.Plan(Set, ["Typo.*"], TestData.Availability((Skia, false), (HarfBuzz, false)));

        Assert.True(report.IsFailure);
        Assert.True(report.HasError(ErrorCodes.FilterUnmatched));
        Assert.Contains("Typo.*", report.Errors[0].Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Missing_availability_is_never_treated_as_unpublished()
    {
        var report = PrunePublishedPlanner.Plan(Set, [], TestData.Availability((Skia, false)));

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
        var report = PrunePublishedPlanner.Plan(Set, [], TestData.Availability((Skia, true), (HarfBuzz, false)));

        Assert.Equal(Set.Packages.Count, report.Value.Decisions.Count);
    }
}
