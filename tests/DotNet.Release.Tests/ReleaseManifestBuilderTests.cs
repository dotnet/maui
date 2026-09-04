using Xunit;

namespace DotNet.Release.Tests;

public class ReleaseManifestBuilderTests
{
    private static ReleaseManifest Manifest(IReadOnlyList<ReleasePackage> drop, bool workload = false, StageOptions? options = null,
        string repo = "mono/skiasharp") => ReleaseManifestBuilder.Build(
            TestData.Source(workload, repo), TestData.Policy(), drop, options ?? new StageOptions(), TestData.Now, TestData.ToolVersion);

    private static ReleasePackage WorkloadManifest(string band = "10", string version = "10.0.0") =>
        TestData.Drop($"Microsoft.NET.Sdk.Maui.Manifest-{band}.0.100", version);

    private static ReleasePackage Package(string fileName, string id, string version, string normalizedVersion, string sha256) => new()
    {
        Id = id,
        Version = version,
        NormalizedVersion = normalizedVersion,
        FileName = fileName,
        Sha256 = sha256,
    };

    // ---- non-workload ----

    [Fact]
    public void Non_workload_release_produces_one_ordered_set()
    {
        var manifest = Manifest([TestData.Drop("SkiaSharp", "3.119.0"), TestData.Drop("HarfBuzzSharp", "8.3.1")]);

        var set = Assert.Single(manifest.Sets);
        Assert.Equal(0, set.Order);
        Assert.Equal(ReleaseManifestBuilder.PackagesArtifactName, set.ArtifactName);
        Assert.Null(manifest.WorkloadSet);

        // Deterministic ordering keeps the manifest hash stable and the audit readable.
        Assert.Equal(["HarfBuzzSharp", "SkiaSharp"], set.Packages.Select(p => p.Id));
    }

    [Fact]
    public void Non_workload_release_rejects_workload_manifests_outright()
    {
        Assert.Throws<DotNetReleaseException>(
            () => Manifest([TestData.Drop("SkiaSharp", "3.119.0"), WorkloadManifest()]));
    }

    [Fact]
    public void Non_workload_release_can_exclude_a_manifest_from_the_selected_set()
    {
        var manifest = Manifest(
            [TestData.Drop("SkiaSharp", "3.119.0"), WorkloadManifest()], options: new StageOptions { Exclude = ["*Manifest*"] });

        Assert.Equal("SkiaSharp", Assert.Single(manifest.AllPackages).Id);
    }

    [Fact]
    public void Filters_selecting_nothing_fail_closed()
    {
        Assert.Throws<DotNetReleaseException>(() => Manifest(
            [TestData.Drop("SkiaSharp", "3.119.0")], options: new StageOptions { Include = ["NothingMatches*"] }));
    }

    [Fact]
    public void Empty_drop_fails_closed()
    {
        Assert.Throws<DotNetReleaseException>(() => Manifest([]));
    }

    // ---- workload ----

    [Fact]
    public void Workload_release_splits_packs_and_manifests_in_publication_order()
    {
        var manifest = Manifest(
            [TestData.Drop("Microsoft.Maui.Controls", "10.0.0"), WorkloadManifest()], workload: true, repo: "dotnet/maui");

        Assert.Equal(2, manifest.Sets.Count);

        var packs = manifest.Sets.Single(s => s.Order == 0);
        var manifests = manifest.Sets.Single(s => s.Order == 1);

        Assert.Equal(ReleaseManifestBuilder.PacksArtifactName, packs.ArtifactName);
        Assert.Equal(ReleaseManifestBuilder.ManifestsArtifactName, manifests.ArtifactName);
        Assert.Equal("Microsoft.Maui.Controls", Assert.Single(packs.Packages).Id);

        // Manifests reference packs, so packs must come first.
        Assert.True(packs.Order < manifests.Order);
    }

    [Fact]
    public void Workload_release_resolves_the_workload_set_from_the_manifest_band()
    {
        var manifest = Manifest(
            [TestData.Drop("Microsoft.Maui.Controls", "10.0.0"), WorkloadManifest("10")], workload: true, repo: "dotnet/maui");

        Assert.Equal(new WorkloadSet(10, ".NET 10 Workload Release", "dotnet10-workloads"), manifest.WorkloadSet);
    }

    [Fact]
    public void Workload_release_with_an_unconfigured_band_fails_closed()
    {
        Assert.Throws<DotNetReleaseException>(() => Manifest(
            [TestData.Drop("Microsoft.Maui.Controls", "12.0.0"), WorkloadManifest("12", "12.0.0")], workload: true, repo: "dotnet/maui"));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Workload_release_requires_both_packs_and_manifests(bool hasPack, bool hasManifest)
    {
        var drop = new List<ReleasePackage>();
        if (hasPack)
        {
            drop.Add(TestData.Drop("Microsoft.Maui.Controls", "10.0.0"));
        }

        if (hasManifest)
        {
            drop.Add(WorkloadManifest());
        }

        Assert.Throws<DotNetReleaseException>(
            () => Manifest(drop, workload: true, repo: "dotnet/maui"));
    }

    /// <summary>
    /// Include filters select workload packs. Manifests remain selected unless explicitly
    /// excluded because a workload release requires both sets.
    /// </summary>
    [Fact]
    public void Include_filters_apply_to_packs_but_not_to_manifests()
    {
        var manifest = Manifest(
            [
                TestData.Drop("Microsoft.Maui.Controls", "10.0.0"),
                TestData.Drop("Microsoft.Maui.Essentials", "10.0.0"),
                WorkloadManifest(),
            ], workload: true, options: new StageOptions { Include = ["Microsoft.Maui.Controls.*"] }, repo: "dotnet/maui");

        Assert.Equal("Microsoft.Maui.Controls", Assert.Single(manifest.Sets[0].Packages).Id);
        Assert.Single(manifest.Sets[1].Packages);
    }

    [Fact]
    public void Exclude_filters_apply_to_manifests_too()
    {
        Assert.Throws<DotNetReleaseException>(() => Manifest(
            [TestData.Drop("Microsoft.Maui.Controls", "10.0.0"), WorkloadManifest()], workload: true,
            options: new StageOptions { Exclude = ["*Manifest*"] },
            repo: "dotnet/maui"));
    }

    // ---- validation ----

    [Fact]
    public void Duplicate_file_names_fail_closed()
    {
        var duplicate = TestData.Drop("SkiaSharp", "3.119.0");
        Assert.Throws<DotNetReleaseException>(() => Manifest([duplicate, duplicate]));
    }

    [Fact]
    public void Duplicate_identities_under_different_file_names_fail_closed()
    {
        Assert.Throws<DotNetReleaseException>(() => Manifest([
            TestData.Drop("SkiaSharp", "3.119.0"),
            TestData.Drop("SkiaSharp", "3.119.0", fileName: "SkiaSharp.3.119.0.0.nupkg"),
        ]));
    }

    [Fact]
    public void Duplicate_normalized_identities_fail_closed()
    {
        Assert.Throws<DotNetReleaseException>(() => Manifest([
            TestData.Drop("SkiaSharp", "3.119", fileName: "SkiaSharp.3.119.nupkg"),
            TestData.Drop("SkiaSharp", "3.119.0", fileName: "SkiaSharp.3.119.0.nupkg"),
        ]));
    }

    [Fact]
    public void Duplicate_normalized_identities_across_workload_sets_fail_closed()
    {
        Assert.Throws<DotNetReleaseException>(() => Manifest(
            [
                TestData.Drop("Shared", "1.0", fileName: "Shared.1.0.nupkg"),
                TestData.Drop("Shared", "1.0.0", fileName: "Shared.Manifest-10.1.0.0.nupkg"),
            ], workload: true, repo: "dotnet/maui"));
    }

    [Fact]
    public void Package_without_an_id_fails_closed()
    {
        Assert.Throws<DotNetReleaseException>(() => Manifest([Package("broken.nupkg", "", "1.0.0", "1.0.0", TestData.Hash("x"))]));
    }

    [Fact]
    public void File_name_disagreeing_with_the_nuspec_id_fails_closed()
    {
        Assert.Throws<DotNetReleaseException>(() => Manifest([
            Package("SomethingElse.3.119.0.nupkg", "SkiaSharp", "3.119.0", "3.119.0", TestData.Hash("x")),
        ]));
    }

    [Fact]
    public void File_name_containing_a_directory_fails_closed()
    {
        Assert.Throws<DotNetReleaseException>(() => Manifest([
            Package("sub/SkiaSharp.3.119.0.nupkg", "SkiaSharp", "3.119.0", "3.119.0", TestData.Hash("x")),
        ]));
    }

    [Fact]
    public void Package_staged_without_a_content_hash_fails_closed()
    {
        Assert.Throws<DotNetReleaseException>(() => Manifest(
            [Package("SkiaSharp.3.119.0.nupkg", "SkiaSharp", "3.119.0", "3.119.0", "")]));
    }

    [Fact]
    public void Package_validation_reports_all_actionable_errors()
    {
        var exception = Assert.Throws<DotNetReleaseException>(() => Manifest([
            Package("First.1.0.0.nupkg", "", "1.0.0", "1.0.0", TestData.Hash("first")),
            Package("wrong-name.nupkg", "Second", "1.0.0", "1.0.0", ""),
        ]));

        Assert.Contains("First.1.0.0.nupkg", exception.Message, StringComparison.Ordinal);
        Assert.Contains("without a content hash", exception.Message, StringComparison.Ordinal);
        Assert.Contains("does not start with its ID", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Wrongly_normalized_version_fails_closed()
    {
        // A reader that returned the raw file-name version instead of the normalized form.
        // This is the check that converts the substring approach's silent wrong answer into
        // a loud failure at stage time.
        Assert.Throws<DotNetReleaseException>(() => Manifest([
            Package("SkiaSharp.3.119.0.0.nupkg", "SkiaSharp", "3.119.0.0", "3.119.0.0", TestData.Hash("x")),
        ]));
    }

    [Fact]
    public void Manifest_carries_the_verified_source_and_tool_version()
    {
        var manifest = Manifest([TestData.Drop("SkiaSharp", "3.119.0")]);

        Assert.Equal(TestData.Commit, manifest.Source.Commit);
        Assert.Equal(4242, manifest.Source.BarBuildId);
        Assert.Equal(TestData.ToolVersion, manifest.ToolVersion);
    }
}
