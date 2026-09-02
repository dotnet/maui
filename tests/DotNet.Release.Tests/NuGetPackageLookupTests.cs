using NuGet.Versioning;
using Xunit;

namespace DotNet.Release.Tests;

public class NuGetPackageLookupTests
{
    private static PlannedPackage Package(string id, string version) => new()
    {
        Id = id,
        Version = version,
        NormalizedVersion = PackageVersions.Normalize(version),
        FileName = $"{id}.{version}.nupkg",
        Sha256 = new string('a', 64),
    };

    private static readonly PlannedPackage Skia = Package("SkiaSharp", "3.119.0");
    private static readonly PlannedPackage HarfBuzz = Package("HarfBuzzSharp", "8.3.1.5");

    [Fact]
    public async Task Reports_availability_keyed_by_identity()
    {
        using var lookup = Lookup((id, _) => id == "SkiaSharp");

        var availability = await lookup.GetAvailabilityAsync([Skia, HarfBuzz], CancellationToken.None);

        Assert.True(availability[Skia.IdentityKey]);
        Assert.False(availability[HarfBuzz.IdentityKey]);
    }

    [Fact]
    public async Task Parses_and_queries_the_normalized_version()
    {
        var queried = new List<string>();
        using var lookup = new NuGetPackageLookup((id, version, _) =>
        {
            queried.Add($"{id}/{version.ToNormalizedString()}");
            return Task.FromResult(false);
        });

        await lookup.GetAvailabilityAsync([Package("SkiaSharp", "3.119.0.0")], CancellationToken.None);

        Assert.Equal("SkiaSharp/3.119.0", Assert.Single(queried));
    }

    [Fact]
    public async Task Every_planned_package_gets_an_answer()
    {
        var packages = new[] { Skia, HarfBuzz, Package("Third", "1.0.0") };
        using var lookup = Lookup((_, _) => false);

        var availability = await lookup.GetAvailabilityAsync(packages, CancellationToken.None);

        Assert.Equal(3, availability.Count);
        Assert.All(packages, package => Assert.Contains(package.IdentityKey, availability.Keys));
    }

    [Fact]
    public async Task Duplicate_identities_are_queried_once()
    {
        var calls = 0;
        using var lookup = new NuGetPackageLookup((_, _, _) =>
        {
            Interlocked.Increment(ref calls);
            return Task.FromResult(false);
        });

        var availability = await lookup.GetAvailabilityAsync(
            [Skia, Skia, Package("SKIASHARP", "3.119.0")], CancellationToken.None);

        Assert.Equal(1, calls);
        Assert.Single(availability);
    }

    [Fact]
    public async Task A_feed_failure_surfaces_rather_than_becoming_not_published()
    {
        using var lookup = new NuGetPackageLookup((id, _, _) => id == "SkiaSharp" ? Task.FromException<bool>(new HttpRequestException("feed unavailable"))
                : Task.FromResult(false));

        await Assert.ThrowsAsync<HttpRequestException>(
            () => lookup.GetAvailabilityAsync([Skia, HarfBuzz], CancellationToken.None));
    }

    [Fact]
    public async Task An_empty_plan_makes_no_queries()
    {
        var calls = 0;
        using var lookup = new NuGetPackageLookup((_, _, _) =>
        {
            Interlocked.Increment(ref calls);
            return Task.FromResult(false);
        });

        Assert.Empty(await lookup.GetAvailabilityAsync([], CancellationToken.None));
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task Concurrency_is_bounded()
    {
        var packages = Enumerable.Range(0, 50)
            .Select(index => Package($"Package{index}", "1.0.0")).ToList();
        var active = 0;
        var peak = 0;
        using var lookup = new NuGetPackageLookup(async (_, _, cancellationToken) =>
        {
            var current = Interlocked.Increment(ref active);
            int observed;
            while (current > (observed = Volatile.Read(ref peak)))
            {
                Interlocked.CompareExchange(ref peak, current, observed);
            }

            try
            {
                await Task.Delay(10, cancellationToken);
                return false;
            }
            finally
            {
                Interlocked.Decrement(ref active);
            }
        }, maxConcurrency: 4);

        var availability = await lookup.GetAvailabilityAsync(packages, CancellationToken.None);

        Assert.Equal(50, availability.Count);
        Assert.Equal(4, peak);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Concurrency_must_be_positive(int maxConcurrency)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new NuGetPackageLookup((_, _, _) => Task.FromResult(false), maxConcurrency));
    }

    [Fact]
    public void A_lookup_delegate_is_required()
    {
        Assert.Throws<ArgumentNullException>(
            () => new NuGetPackageLookup((Func<string, NuGetVersion, CancellationToken, Task<bool>>)null!));
    }

    [Fact]
    public void The_production_feed_is_nuget_org()
    {
        Assert.Equal("https://api.nuget.org/v3/index.json", NuGetPackageLookup.NuGetOrgIndex);
    }

    private static NuGetPackageLookup Lookup(Func<string, NuGetVersion, bool> lookup) =>
        new((id, version, _) => Task.FromResult(lookup(id, version)));
}
