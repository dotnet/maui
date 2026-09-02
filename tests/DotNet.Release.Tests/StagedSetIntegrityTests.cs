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
        StagedSetIntegrity.ValidateStaged(Set, Files(Skia, HarfBuzz));
    }

    [Fact]
    public void Companion_file_names_are_not_allow_listed_in_the_rule_itself()
    {
        var files = Files(Skia, HarfBuzz);
        files["release-manifest.json"] = TestData.Hash("manifest");
        Assert.Throws<DotNetReleaseException>(() => StagedSetIntegrity.ValidateStaged(Set, files));
    }

    [Fact]
    public void A_stray_package_is_rejected()
    {
        var files = Files(Skia, HarfBuzz);
        files["Sneaky.1.0.0.nupkg"] = TestData.Hash("sneaky");

        Assert.Throws<DotNetReleaseException>(() => StagedSetIntegrity.ValidateStaged(Set, files));
    }

    [Fact]
    public void Missing_pending_file_fails_closed()
    {
        Assert.Throws<DotNetReleaseException>(
            () => StagedSetIntegrity.ValidateStaged(Set, Files(Skia)));
    }

    [Fact]
    public void Tampered_file_fails_closed()
    {
        var files = Files(Skia, HarfBuzz);
        files[Skia.FileName] = TestData.Hash("tampered");

        Assert.Throws<DotNetReleaseException>(() => StagedSetIntegrity.ValidateStaged(Set, files));
    }

    [Fact]
    public void Removed_files_are_absent_after_pruning()
    {
        var report = PrunePublishedPlanner.Plan(Set, Set.Packages,
            [], TestData.Availability((Skia, true), (HarfBuzz, false)));

        StagedSetIntegrity.ValidateFiltered(Set, Files(HarfBuzz), report);
    }

    [Fact]
    public void A_withheld_package_still_on_disk_fails_closed()
    {
        var report = PrunePublishedPlanner.Plan(Set, Set.Packages,
            [], TestData.Availability((Skia, true), (HarfBuzz, false)));

        Assert.Throws<DotNetReleaseException>(
            () => StagedSetIntegrity.ValidateFiltered(Set, Files(Skia, HarfBuzz), report));
    }

    [Fact]
    public void The_invariant_holds_for_every_disposition()
    {
        var report = PrunePublishedPlanner.Plan(Set, Set.Packages,
            [HarfBuzz.FileName], TestData.Availability((Skia, true), (HarfBuzz, false)));

        StagedSetIntegrity.ValidateFiltered(Set, new Dictionary<string, string>(), report);
        Assert.Equal(0, report.PendingCount);
    }

    [Fact]
    public void Package_hash_enumeration_is_extension_scoped()
    {
        using var workspace = new Workspace();
        var directory = workspace.StagedSet(ReleaseManifestBuilder.PackagesArtifactName);
        Directory.CreateDirectory(Path.Combine(directory, "diagnostics"));
        File.WriteAllText(Path.Combine(directory, Skia.FileName), "package");
        File.WriteAllText(Path.Combine(directory, "release-audit.md"), "companion");
        File.WriteAllText(Path.Combine(directory, "diagnostics", "release.log"), "companion");
        File.WriteAllText(Path.Combine(directory, "nupkg-lookalike.nupkg.txt"), "companion");

        Assert.Equal([Skia.FileName], ReleaseArtifact.ReadPackageHashes(directory).Keys);
    }
}
