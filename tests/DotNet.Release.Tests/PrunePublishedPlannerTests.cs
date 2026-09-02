using Xunit;

namespace DotNet.Release.Tests;

public class PrunePublishedPlannerTests
{
    private static readonly ReleasePackage Skia = TestData.Drop("SkiaSharp", "3.119.0");
    private static readonly ReleasePackage HarfBuzz = TestData.Drop("HarfBuzzSharp", "8.3.1");

    private static readonly ReleasePackageSet Set = TestData.Set(HarfBuzz, Skia);

    [Fact]
    public void Packages_not_on_nuget_org_stay_pending()
    {
        var report = PrunePublishedPlanner.Plan(Set, Set.Packages, [], TestData.Availability((Skia, false), (HarfBuzz, false)));

        Assert.Equal(2, report.PendingCount);
        Assert.True(report.HasPackagesToPublish);
        Assert.Empty(report.FilesToRemove);
    }

    /// <summary>
    /// The 1ES NuGet task treats HTTP 409 as fatal even with allowPackageConflicts, so
    /// already-published packages must be removed from the push set rather than retried.
    /// </summary>
    [Fact]
    public void Already_published_packages_are_removed_from_the_push_set()
    {
        var report = PrunePublishedPlanner.Plan(Set, Set.Packages, [], TestData.Availability((Skia, true), (HarfBuzz, false)));

        Assert.Equal(1, report.PendingCount);
        Assert.Equal([Skia.FileName], report.FilesToRemove);

        Assert.Equal(PackageDisposition.AlreadyPublished,
            report.Decisions.Single(d => d.Package.FileName == Skia.FileName).Disposition);
    }

    [Fact]
    public void Everything_published_means_the_publish_task_can_be_skipped()
    {
        var report = PrunePublishedPlanner.Plan(Set, Set.Packages, [], TestData.Availability((Skia, true), (HarfBuzz, true)));

        Assert.Equal(0, report.PendingCount);
        Assert.False(report.HasPackagesToPublish);
    }

    /// <summary>
    /// NuGet.org can reserve a version and return 409 before it is visible through the
    /// flat-container API, so recovery filters are honoured before the feed is consulted.
    /// </summary>
    [Fact]
    public void Recovery_filters_win_over_the_feed_query()
    {
        var report = PrunePublishedPlanner.Plan(Set, Set.Packages,
            [Skia.FileName], TestData.Availability((Skia, false), (HarfBuzz, false)));

        Assert.Equal(PackageDisposition.PreviouslyAttempted,
            report.Decisions.Single(d => d.Package.FileName == Skia.FileName).Disposition);
        Assert.Equal(1, report.PendingCount);
    }

    [Fact]
    public void Recovery_filters_support_wildcards()
    {
        var report = PrunePublishedPlanner.Plan(Set, Set.Packages,
            ["SkiaSharp.*"], TestData.Availability((Skia, false), (HarfBuzz, false)));

        Assert.Equal([Skia.FileName], report.FilesToRemove);
    }

    /// <summary>
    /// A mistyped recovery filter would publish the package the operator meant to withhold,
    /// which is the exact failure the filter exists to prevent.
    /// </summary>
    [Fact]
    public void Recovery_filter_matching_nothing_fails_closed()
    {
        var exception = Assert.Throws<DotNetReleaseException>(() => PrunePublishedPlanner.Plan(Set, Set.Packages,
            ["Typo.*"], TestData.Availability((Skia, false), (HarfBuzz, false))));

        Assert.Contains("Typo.*", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Missing_availability_is_never_treated_as_unpublished()
    {
        Assert.Throws<DotNetReleaseException>(() => PrunePublishedPlanner.Plan(Set, Set.Packages, [], TestData.Availability((Skia, false))));
    }

    [Fact]
    public void Identity_key_is_case_insensitive_on_both_id_and_version()
    {
        var upper = TestData.Drop("SKIASHARP", "3.119.0");

        Assert.Equal(Skia.IdentityKey, upper.IdentityKey);
    }

    [Fact]
    public void Report_records_a_decision_for_every_release_package()
    {
        var report = PrunePublishedPlanner.Plan(Set, Set.Packages, [], TestData.Availability((Skia, true), (HarfBuzz, false)));

        Assert.Equal(Set.Packages.Count, report.Decisions.Count);
    }

    [Fact]
    public void Recovery_filter_for_another_release_set_is_valid_but_does_not_prune_this_set()
    {
        var manifest = TestData.Drop("Microsoft.NET.Sdk.Maui.Manifest-10.0.100", "10.0.0");

        var report = PrunePublishedPlanner.Plan(Set,
            [.. Set.Packages, manifest],
            [manifest.FileName], TestData.Availability((Skia, false), (HarfBuzz, false)));

        Assert.Equal(2, report.PendingCount);
        Assert.Empty(report.FilesToRemove);
    }
}
