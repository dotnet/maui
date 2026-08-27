using Xunit;

namespace DotNet.Release.Core.Tests;

public class StagePlannerTests
{
    private static Result<ReleasePlan> Plan(
        IReadOnlyList<DropPackage> drop,
        bool workload = false,
        StageOptions? options = null,
        string repo = "dotnet/skiasharp") =>
        StagePlanner.Create(
            TestData.Resolved(workload, repo),
            TestData.Policy(),
            drop,
            options ?? new StageOptions(),
            TestData.Tool,
            TestData.Now,
            TestData.ToolVersion);

    private static DropPackage Manifest(string band = "10", string version = "10.0.0") =>
        TestData.Drop($"Microsoft.NET.Sdk.Maui.Manifest-{band}.0.100", version);

    // ---- non-workload ----

    [Fact]
    public void Non_workload_release_produces_one_ordered_set()
    {
        var plan = Plan([TestData.Drop("SkiaSharp", "3.119.0"), TestData.Drop("HarfBuzzSharp", "8.3.1")]);

        Assert.True(plan.IsSuccess, string.Join("; ", plan.Errors));
        var set = Assert.Single(plan.Value.Sets);
        Assert.Equal(0, set.Order);
        Assert.Equal(StagePlanner.PackagesArtifactName, set.ArtifactName);
        Assert.Null(plan.Value.WorkloadSet);

        // Deterministic ordering keeps the plan hash stable and the audit readable.
        Assert.Equal(["HarfBuzzSharp", "SkiaSharp"], set.Packages.Select(p => p.Id));
    }

    [Fact]
    public void Non_workload_release_rejects_workload_manifests_outright()
    {
        var plan = Plan([TestData.Drop("SkiaSharp", "3.119.0"), Manifest()]);

        Assert.True(plan.IsFailure);
        Assert.True(plan.HasError(ErrorCodes.ManifestInNonWorkload));
    }

    [Fact]
    public void Filters_selecting_nothing_fail_closed()
    {
        var plan = Plan(
            [TestData.Drop("SkiaSharp", "3.119.0")],
            options: new StageOptions { Include = ["NothingMatches*"] });

        Assert.True(plan.IsFailure);
        Assert.True(plan.HasError(ErrorCodes.PackageSetEmpty));
    }

    [Fact]
    public void Empty_drop_fails_closed()
    {
        Assert.True(Plan([]).HasError(ErrorCodes.PackageSetEmpty));
    }

    // ---- workload ----

    [Fact]
    public void Workload_release_splits_packs_and_manifests_in_publication_order()
    {
        var plan = Plan(
            [TestData.Drop("Microsoft.Maui.Controls", "10.0.0"), Manifest()],
            workload: true,
            repo: "dotnet/maui");

        Assert.True(plan.IsSuccess, string.Join("; ", plan.Errors));
        Assert.Equal(2, plan.Value.Sets.Count);

        var packs = plan.Value.Sets.Single(s => s.Order == 0);
        var manifests = plan.Value.Sets.Single(s => s.Order == 1);

        Assert.Equal(StagePlanner.PacksArtifactName, packs.ArtifactName);
        Assert.Equal(StagePlanner.ManifestsArtifactName, manifests.ArtifactName);
        Assert.Equal("Microsoft.Maui.Controls", Assert.Single(packs.Packages).Id);

        // Manifests reference packs, so packs must come first.
        Assert.True(packs.Order < manifests.Order);
    }

    [Fact]
    public void Workload_release_resolves_the_workload_set_from_the_manifest_band()
    {
        var plan = Plan(
            [TestData.Drop("Microsoft.Maui.Controls", "10.0.0"), Manifest("10")],
            workload: true,
            repo: "dotnet/maui");

        Assert.True(plan.IsSuccess, string.Join("; ", plan.Errors));
        Assert.Equal(new WorkloadSetTarget(10, ".NET 10 Workload Release", "dotnet10-workloads"), plan.Value.WorkloadSet);
    }

    [Fact]
    public void Workload_release_with_an_unconfigured_band_fails_closed()
    {
        var plan = Plan(
            [TestData.Drop("Microsoft.Maui.Controls", "12.0.0"), Manifest("12", "12.0.0")],
            workload: true,
            repo: "dotnet/maui");

        Assert.True(plan.IsFailure);
        Assert.True(plan.HasError(ErrorCodes.WorkloadSetNotConfigured));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Workload_release_requires_both_packs_and_manifests(bool hasPack, bool hasManifest)
    {
        var drop = new List<DropPackage>();
        if (hasPack)
        {
            drop.Add(TestData.Drop("Microsoft.Maui.Controls", "10.0.0"));
        }

        if (hasManifest)
        {
            drop.Add(Manifest());
        }

        var plan = Plan(drop, workload: true, repo: "dotnet/maui");

        Assert.True(plan.IsFailure);
        Assert.True(plan.HasError(ErrorCodes.PackageSetEmpty));
    }

    /// <summary>
    /// Preserved verbatim from the current pipeline: include filters select packs, but
    /// manifests are selected unless explicitly excluded. Applying include filters to
    /// manifests would silently drop them from the release.
    /// </summary>
    [Fact]
    public void Include_filters_apply_to_packs_but_not_to_manifests()
    {
        var plan = Plan(
            [
                TestData.Drop("Microsoft.Maui.Controls", "10.0.0"),
                TestData.Drop("Microsoft.Maui.Essentials", "10.0.0"),
                Manifest(),
            ],
            workload: true,
            options: new StageOptions { Include = ["Microsoft.Maui.Controls.*"] },
            repo: "dotnet/maui");

        Assert.True(plan.IsSuccess, string.Join("; ", plan.Errors));
        Assert.Equal("Microsoft.Maui.Controls", Assert.Single(plan.Value.Sets[0].Packages).Id);
        Assert.Single(plan.Value.Sets[1].Packages);
    }

    [Fact]
    public void Exclude_filters_apply_to_manifests_too()
    {
        var plan = Plan(
            [TestData.Drop("Microsoft.Maui.Controls", "10.0.0"), Manifest()],
            workload: true,
            options: new StageOptions { Exclude = ["*Manifest*"] },
            repo: "dotnet/maui");

        Assert.True(plan.IsFailure);
        Assert.True(plan.HasError(ErrorCodes.PackageSetEmpty));
    }

    // ---- validation ----

    [Fact]
    public void Duplicate_file_names_fail_closed()
    {
        var duplicate = TestData.Drop("SkiaSharp", "3.119.0");
        var plan = Plan([duplicate, duplicate]);

        Assert.True(plan.IsFailure);
        Assert.True(plan.HasError(ErrorCodes.PackageDuplicateFileName));
    }

    [Fact]
    public void Duplicate_identities_under_different_file_names_fail_closed()
    {
        var plan = Plan([
            TestData.Drop("SkiaSharp", "3.119.0"),
            TestData.Drop("SkiaSharp", "3.119.0", fileName: "SkiaSharp.3.119.0.0.nupkg"),
        ]);

        Assert.True(plan.IsFailure);
        Assert.True(plan.HasError(ErrorCodes.PackageDuplicateIdentity));
    }

    [Fact]
    public void Package_without_an_id_fails_closed()
    {
        var plan = Plan([new DropPackage("broken.nupkg", "", "1.0.0", "1.0.0", TestData.Hash("x"))]);

        Assert.True(plan.IsFailure);
        Assert.True(plan.HasError(ErrorCodes.PackageMalformed));
    }

    [Fact]
    public void File_name_disagreeing_with_the_nuspec_id_fails_closed()
    {
        var plan = Plan([new DropPackage(
            "SomethingElse.3.119.0.nupkg", "SkiaSharp", "3.119.0", "3.119.0", TestData.Hash("x"))]);

        Assert.True(plan.IsFailure);
        Assert.True(plan.HasError(ErrorCodes.PackageMalformed));
    }

    [Fact]
    public void File_name_containing_a_directory_fails_closed()
    {
        var plan = Plan([new DropPackage(
            "sub/SkiaSharp.3.119.0.nupkg", "SkiaSharp", "3.119.0", "3.119.0", TestData.Hash("x"))]);

        Assert.True(plan.IsFailure);
        Assert.True(plan.HasError(ErrorCodes.PackageMalformed));
    }

    [Fact]
    public void Package_staged_without_a_content_hash_fails_closed()
    {
        var plan = Plan([new DropPackage("SkiaSharp.3.119.0.nupkg", "SkiaSharp", "3.119.0", "3.119.0", "")]);

        Assert.True(plan.IsFailure);
        Assert.True(plan.HasError(ErrorCodes.PackageMalformed));
    }

    [Fact]
    public void Wrongly_normalized_version_fails_closed()
    {
        // What the old substring hack would have produced for a four-part version.
        var plan = Plan([new DropPackage(
            "SkiaSharp.3.119.0.0.nupkg", "SkiaSharp", "3.119.0.0", "3.119.0.0", TestData.Hash("x"))]);

        Assert.True(plan.IsFailure);
        Assert.True(plan.HasError(ErrorCodes.PackageMalformed));
    }

    [Fact]
    public void Plan_carries_the_verified_source_and_the_tool_hash()
    {
        var plan = Plan([TestData.Drop("SkiaSharp", "3.119.0")]).Value;

        Assert.Equal(TestData.Commit, plan.Source.Commit);
        Assert.Equal(4242, plan.Source.BarBuildId);
        Assert.Equal(TestData.Tool, plan.Tool);
        Assert.Equal(TestData.ToolVersion, plan.ToolVersion);
    }
}
