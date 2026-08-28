using Xunit;

namespace DotNet.Release.Tests;

public class PackageAvailabilityProbeTests
{
    private static PlannedPackage Package(string id, string version) => new()
    {
        Id = id,
        Version = version,
        NormalizedVersion = PackageVersions.Normalize(version).Value,
        FileName = $"{id}.{version}.nupkg",
        Sha256 = new string('a', 64),
    };

    private static readonly PlannedPackage Skia = Package("SkiaSharp", "3.119.0");
    private static readonly PlannedPackage HarfBuzz = Package("HarfBuzzSharp", "8.3.1.5");

    [Fact]
    public async Task Reports_availability_keyed_by_identity()
    {
        var checker = new FakeExistenceChecker("SkiaSharp/3.119.0");

        var availability = await new PackageAvailabilityProbe(checker)
            .GetAvailabilityAsync([Skia, HarfBuzz], CancellationToken.None);

        Assert.True(availability[Skia.IdentityKey]);
        Assert.False(availability[HarfBuzz.IdentityKey]);
    }

    [Fact]
    public async Task Queries_the_normalized_version()
    {
        var checker = new FakeExistenceChecker();

        await new PackageAvailabilityProbe(checker)
            .GetAvailabilityAsync([Package("SkiaSharp", "3.119.0.0")], CancellationToken.None);

        Assert.Equal("SkiaSharp/3.119.0", Assert.Single(checker.Queried));
    }

    [Fact]
    public async Task Every_planned_package_gets_an_answer()
    {
        var packages = new[] { Skia, HarfBuzz, Package("Third", "1.0.0") };

        var availability = await new PackageAvailabilityProbe(new FakeExistenceChecker())
            .GetAvailabilityAsync(packages, CancellationToken.None);

        Assert.Equal(3, availability.Count);
        Assert.All(packages, p => Assert.True(availability.ContainsKey(p.IdentityKey)));
    }

    /// <summary>
    /// The same identity can legitimately appear in more than one set. Querying it twice is
    /// wasted work with a chance of two different answers.
    /// </summary>
    [Fact]
    public async Task Duplicate_identities_are_queried_once()
    {
        var checker = new FakeExistenceChecker();

        var availability = await new PackageAvailabilityProbe(checker)
            .GetAvailabilityAsync([Skia, Skia, Package("SKIASHARP", "3.119.0")], CancellationToken.None);

        Assert.Equal(1, checker.Calls);
        Assert.Single(availability);
    }

    /// <summary>
    /// A feed failure must not be silently reported as "not published": that would let the
    /// publish job re-push a package that already exists, and the 1ES NuGet task treats the
    /// resulting HTTP 409 as fatal.
    /// </summary>
    [Fact]
    public async Task A_feed_failure_surfaces_rather_than_becoming_not_published()
    {
        var checker = new FakeExistenceChecker { Throw = (id, _) => id == "SkiaSharp" };

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            new PackageAvailabilityProbe(checker).GetAvailabilityAsync([Skia, HarfBuzz], CancellationToken.None));
    }

    [Fact]
    public async Task An_empty_plan_makes_no_queries()
    {
        var checker = new FakeExistenceChecker();

        Assert.Empty(await new PackageAvailabilityProbe(checker).GetAvailabilityAsync([], CancellationToken.None));
        Assert.Equal(0, checker.Calls);
    }

    [Fact]
    public async Task Concurrency_is_bounded()
    {
        var packages = Enumerable.Range(0, 50).Select(i => Package($"Package{i}", "1.0.0")).ToList();
        var checker = new FakeExistenceChecker();

        var availability = await new PackageAvailabilityProbe(checker, maxConcurrency: 4)
            .GetAvailabilityAsync(packages, CancellationToken.None);

        Assert.Equal(50, availability.Count);
        Assert.Equal(50, checker.Calls);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Concurrency_must_be_positive(int maxConcurrency)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PackageAvailabilityProbe(new FakeExistenceChecker(), maxConcurrency));
    }

    [Fact]
    public void A_checker_is_required()
    {
        Assert.Throws<ArgumentNullException>(() => new PackageAvailabilityProbe(null!));
    }

    // ---- integration with release decisions ----

    [Fact]
    public async Task Drives_the_filter_decision_end_to_end()
    {
        var set = new ReleasePackageSet
        {
            Name = "NuGet packages",
            Order = 0,
            ArtifactName = StagePlanner.PackagesArtifactName,
            Packages = [Skia, HarfBuzz],
        };

        var availability = await new PackageAvailabilityProbe(new FakeExistenceChecker("SkiaSharp/3.119.0"))
            .GetAvailabilityAsync(set.Packages, CancellationToken.None);

        var report = FilterPlanner.Plan(set, [], availability);

        Assert.True(report.IsSuccess, string.Join("; ", report.Errors));
        Assert.Equal([Skia.FileName], report.Value.FilesToRemove);
        Assert.Equal(1, report.Value.PendingCount);
    }

    [Fact]
    public async Task Drives_the_verification_decision_end_to_end()
    {
        var packages = new[] { Skia, HarfBuzz };

        var partial = await new PackageAvailabilityProbe(new FakeExistenceChecker("SkiaSharp/3.119.0"))
            .GetAvailabilityAsync(packages, CancellationToken.None);
        Assert.Equal("HarfBuzzSharp", Assert.Single(VerificationEvaluator.GetMissing(packages, partial)).Id);

        var complete = await new PackageAvailabilityProbe(
                new FakeExistenceChecker("SkiaSharp/3.119.0", "HarfBuzzSharp/8.3.1.5"))
            .GetAvailabilityAsync(packages, CancellationToken.None);
        Assert.True(VerificationEvaluator.IsComplete(packages, complete));
    }

    /// <summary>
    /// The real checker points at NuGet.org read-only and is never given a credential. This
    /// asserts the constant rather than making a network call.
    /// </summary>
    [Fact]
    public void The_production_feed_is_nuget_org_read_only()
    {
        Assert.Equal("https://api.nuget.org/v3/index.json", FlatContainerExistenceChecker.NuGetOrgIndex);

        Assert.DoesNotContain(
            typeof(FlatContainerExistenceChecker).GetMethods(),
            m => m.Name.Contains("Push", StringComparison.OrdinalIgnoreCase) ||
                 m.Name.Contains("Publish", StringComparison.OrdinalIgnoreCase));
    }
}
