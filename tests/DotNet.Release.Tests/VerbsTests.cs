using Xunit;

namespace DotNet.Release.Tests;

public class VerbsTests : IDisposable
{
    private static readonly ChannelReference Libraries = new(".NET Libraries", 1648);

    private readonly Workspace _workspace = new();
    private readonly RecordingWriter _output = new();

    /// <summary>The pin the preparing stage computes. Production always supplies it.</summary>
    private string ManifestHash => ReleaseManifestSerializer.ComputeHash(_workspace.ReadManifest());

    public void Dispose() => _workspace.Dispose();

    private Task Stage(StageOptions? options = null, string repo = "dotnet/skiasharp", string? commit = null, int barId = 4242,
        IBuildRegistry? registry = null) =>
        StageCommand.ExecuteAsync(
            _output,
            registry ?? new FakeRegistry(Workspace.Build(channels: Libraries)),
            Workspace.PolicyJson,
            repo,
            commit ?? Workspace.Commit,
            barId,
            _workspace.Drop,
            _workspace.Out,
            options ?? new StageOptions(),
            Workspace.Now,
            "1.0.0-test",
            CancellationToken.None);

    // ---- stage ----

    [Fact]
    public async Task Stage_validates_the_drop_and_writes_the_release_manifest()
    {
        _workspace.WritePackage("SkiaSharp", "3.119.0.0");
        _workspace.WritePackage("HarfBuzzSharp", "8.3.1.5");
        await Stage();

        var manifest = ReleaseManifestSerializer.DeserializeManifest(_workspace.ReadManifest());
        var skia = manifest.AllPackages.Single(package => package.Id == "SkiaSharp");
        Assert.Equal(2, manifest.Sets[0].Packages.Count);
        // The staged directory is what 1ES will glob.
        var staged = Directory.GetFiles(_workspace.StagedSet(ReleaseManifestBuilder.PackagesArtifactName), "*.nupkg");
        Assert.Equal(2, staged.Length);

        var output = _output.AllOutput;
        Assert.Contains("Release manifest:", output, StringComparison.Ordinal);
        Assert.Contains("Resolved build:", output, StringComparison.Ordinal);
        Assert.Contains("Repository origin : GitHubRepository", output, StringComparison.Ordinal);
        Assert.Contains("Schema version : 1", output, StringComparison.Ordinal);
        Assert.Contains("Tool version   : 1.0.0-test", output, StringComparison.Ordinal);
        Assert.Contains($"Created UTC    : {Workspace.Now:O}", output, StringComparison.Ordinal);
        Assert.Contains("Repository        : dotnet/skiasharp", output, StringComparison.Ordinal);
        Assert.Contains("Repository URL    : https://github.com/dotnet/skiasharp", output, StringComparison.Ordinal);
        Assert.Contains($"Commit            : {Workspace.Commit}", output, StringComparison.Ordinal);
        Assert.Contains("BAR build ID      : 4242", output, StringComparison.Ordinal);
        Assert.Contains("Workload          : False", output, StringComparison.Ordinal);
        Assert.Contains("Channel name      : .NET Libraries", output, StringComparison.Ordinal);
        Assert.Contains("Channel ID        : 1648", output, StringComparison.Ordinal);
        Assert.Contains("Workload target : (none)", output, StringComparison.Ordinal);
        Assert.Contains("Name          : NuGet packages", output, StringComparison.Ordinal);
        Assert.Contains("Order         : 0", output, StringComparison.Ordinal);
        Assert.Contains($"Artifact name : {ReleaseManifestBuilder.PackagesArtifactName}", output, StringComparison.Ordinal);
        Assert.Contains("ID                 : SkiaSharp", output, StringComparison.Ordinal);
        Assert.Contains($"Raw version        : {skia.Version}", output, StringComparison.Ordinal);
        Assert.Contains($"Normalized version : {skia.NormalizedVersion}", output, StringComparison.Ordinal);
        Assert.Contains("File name          : SkiaSharp.3.119.0.0.nupkg", output, StringComparison.Ordinal);
        Assert.Contains($"SHA-256            : {skia.Sha256}", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Stage_prints_the_manifest_hash_for_the_pipeline_to_pin()
    {
        _workspace.WritePackage("SkiaSharp", "3.119.0");
        await Stage();

        var expected = ReleaseManifestSerializer.ComputeHash(_workspace.ReadManifest());
        Assert.Contains($"Release manifest SHA-256: {expected}", _output.AllOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Stage_fails_closed_on_an_empty_drop()
    {
        await Assert.ThrowsAsync<DotNetReleaseException>(() => Stage());
    }

    [Fact]
    public async Task Stage_does_not_fall_back_to_packages_outside_the_shipping_directory()
    {
        var package = _workspace.WritePackage("SkiaSharp", "3.119.0");
        var outsideShipping = Path.Combine(_workspace.Drop, Path.GetFileName(package));
        File.Move(package, outsideShipping);
        Directory.Delete(Path.Combine(_workspace.Drop, "shipping"), recursive: true);
        await Assert.ThrowsAsync<DotNetReleaseException>(() => Stage());
    }

    [Fact]
    public async Task Stage_applies_exclude_filters()
    {
        _workspace.WritePackage("SkiaSharp", "3.119.0");
        _workspace.WritePackage("HarfBuzzSharp", "8.3.1.5");
        await Stage(new StageOptions { Exclude = ["HarfBuzz*"] });

        var manifest = ReleaseManifestSerializer.DeserializeManifest(_workspace.ReadManifest());
        Assert.Equal("SkiaSharp", Assert.Single(manifest.Sets[0].Packages).Id);
    }

    [Fact]
    public async Task Stage_rejects_a_workload_manifest_in_a_non_workload_release()
    {
        _workspace.WritePackage("SkiaSharp", "3.119.0");
        _workspace.WritePackage("Microsoft.NET.Sdk.Maui.Manifest-10.0.100", "10.0.0");
        await Assert.ThrowsAsync<DotNetReleaseException>(() => Stage());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Stage_rejects_a_non_positive_bar_id(int barId)
    {
        _workspace.WritePackage("SkiaSharp", "3.119.0");

        await Assert.ThrowsAsync<DotNetReleaseException>(() => Stage(barId: barId));
    }

    [Fact]
    public async Task Stage_requeries_and_validates_the_candidate_BAR_build()
    {
        _workspace.WritePackage("SkiaSharp", "3.119.0");
        var registry = new FakeRegistry(Workspace.Build(channels: Libraries));

        await Stage(registry: registry);

        Assert.Equal(4242, registry.RequestedBarId);
    }

    [Fact]
    public async Task Stage_fails_closed_when_the_required_BAR_channel_is_missing()
    {
        _workspace.WritePackage("SkiaSharp", "3.119.0");

        await Assert.ThrowsAsync<DotNetReleaseException>(() => Stage(registry: new FakeRegistry(Workspace.Build())));
    }

    [Fact]
    public async Task Stage_fails_closed_when_the_candidate_BAR_build_belongs_to_another_repository()
    {
        _workspace.WritePackage("SkiaSharp", "3.119.0");
        var build = Workspace.Build("https://github.com/dotnet/maui", Libraries);

        await Assert.ThrowsAsync<DotNetReleaseException>(() => Stage(registry: new FakeRegistry(build)));
    }

    // ---- prune-published ----

    [Theory]
    [InlineData("../outside.nupkg")]
    [InlineData("sub/package.nupkg")]
    [InlineData("/rooted.nupkg")]
    [InlineData("")]
    public void A_deletion_target_must_be_one_file_name(string value)
    {
        Assert.False(ReleaseArtifact.IsSinglePathComponent(value));
    }

    [Theory]
    [InlineData("SkiaSharp.3.119.0.nupkg")]
    [InlineData("release-manifest.json")]
    public void A_single_path_component_is_accepted(string value)
    {
        Assert.True(ReleaseArtifact.IsSinglePathComponent(value));
    }

    private async Task<ReleaseManifest> StagedManifestAsync(params (string Id, string Version)[] packages)
    {
        foreach (var (id, version) in packages)
        {
            _workspace.WritePackage(id, version);
        }

        await Stage();

        return ReleaseManifestSerializer.DeserializeManifest(_workspace.ReadManifest());
    }

    private Task Filter(INuGetPackageLookup probe, string[]? recovery = null, string? expectedHash = null, string? set = null) =>
        PrunePublishedCommand.ExecuteAsync(
            _output, probe, _workspace.ReadManifest(), _workspace.Out, recovery ?? [], expectedHash ?? ManifestHash, set, CancellationToken.None);

    [Fact]
    public async Task Filter_removes_already_published_packages_from_the_push_set()
    {
        await StagedManifestAsync(("SkiaSharp", "3.119.0"), ("HarfBuzzSharp", "8.3.1.5"));

        await Filter(new FakeProbe("skiasharp/3.119.0"));

        var directory = _workspace.StagedSet(ReleaseManifestBuilder.PackagesArtifactName);
        Assert.False(File.Exists(Path.Combine(directory, "SkiaSharp.3.119.0.nupkg")));
        Assert.True(File.Exists(Path.Combine(directory, "HarfBuzzSharp.8.3.1.5.nupkg")));

        Assert.Single(Directory.GetFiles(directory, "*.nupkg"));
    }

    [Fact]
    public async Task Prune_logs_release_identity_and_every_decision_field_and_disposition()
    {
        await StagedManifestAsync(("SkiaSharp", "3.119.0"), ("HarfBuzzSharp", "8.3.1.5"), ("Pending.Package", "1.2.3"));
        _output.Output.Clear();

        await Filter(new FakeProbe("skiasharp/3.119.0"), recovery: ["HarfBuzzSharp.*"]);

        var output = _output.AllOutput;
        Assert.Contains("Selected release:", output, StringComparison.Ordinal);
        Assert.Contains("Repository        : dotnet/skiasharp", output, StringComparison.Ordinal);
        Assert.Contains($"Commit            : {Workspace.Commit}", output, StringComparison.Ordinal);
        Assert.Contains("BAR build ID      : 4242", output, StringComparison.Ordinal);
        Assert.Contains($"Manifest SHA-256 : {ManifestHash}", output, StringComparison.Ordinal);
        Assert.Contains("Set name       : NuGet packages", output, StringComparison.Ordinal);
        Assert.Contains($"Artifact name  : {ReleaseManifestBuilder.PackagesArtifactName}", output, StringComparison.Ordinal);
        Assert.Contains("ID                 : SkiaSharp", output, StringComparison.Ordinal);
        Assert.Contains("Normalized version : 3.119.0", output, StringComparison.Ordinal);
        Assert.Contains("File name          : SkiaSharp.3.119.0.nupkg", output, StringComparison.Ordinal);
        Assert.Contains("Disposition        : AlreadyPublished", output, StringComparison.Ordinal);
        Assert.Contains("ID                 : HarfBuzzSharp", output, StringComparison.Ordinal);
        Assert.Contains("Normalized version : 8.3.1.5", output, StringComparison.Ordinal);
        Assert.Contains("File name          : HarfBuzzSharp.8.3.1.5.nupkg", output, StringComparison.Ordinal);
        Assert.Contains("Disposition        : PreviouslyAttempted", output, StringComparison.Ordinal);
        Assert.Contains("ID                 : Pending.Package", output, StringComparison.Ordinal);
        Assert.Contains("Raw version        : 1.2.3", output, StringComparison.Ordinal);
        Assert.Contains("Normalized version : 1.2.3", output, StringComparison.Ordinal);
        Assert.Contains("File name          : Pending.Package.1.2.3.nupkg", output, StringComparison.Ordinal);
        var pendingPackage = ReleaseManifestSerializer.DeserializeManifest(_workspace.ReadManifest()).AllPackages
            .Single(package => package.Id == "Pending.Package");
        Assert.Contains($"SHA-256            : {pendingPackage.Sha256}", output, StringComparison.Ordinal);
        Assert.Contains("Disposition        : Pending", output, StringComparison.Ordinal);
        Assert.Contains("Pending count : 1", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Filter_keeps_the_manifest_immutable_and_writes_no_sidecar()
    {
        await StagedManifestAsync(("SkiaSharp", "3.119.0"));
        var beforeFilter = _workspace.ReadManifest();

        await Filter(new FakeProbe("skiasharp/3.119.0"));

        Assert.Empty(Directory.GetFiles(_workspace.StagedSet(ReleaseManifestBuilder.PackagesArtifactName), "*.json"));
        Assert.Equal(beforeFilter, _workspace.ReadManifest());
    }

    [Fact]
    public async Task Filter_can_run_twice_without_restaging()
    {
        await StagedManifestAsync(("SkiaSharp", "3.119.0"), ("HarfBuzzSharp", "8.3.1.5"));
        var lookup = new FakeProbe("skiasharp/3.119.0");

        await Filter(lookup);
        await Filter(lookup);

        var directory = _workspace.StagedSet(ReleaseManifestBuilder.PackagesArtifactName);
        Assert.False(File.Exists(Path.Combine(directory, "SkiaSharp.3.119.0.nupkg")));
        Assert.True(File.Exists(Path.Combine(directory, "HarfBuzzSharp.8.3.1.5.nupkg")));
    }

    [Fact]
    public async Task Filter_reports_nothing_to_publish_when_everything_is_live()
    {
        await StagedManifestAsync(("SkiaSharp", "3.119.0"));

        await Filter(new FakeProbe("skiasharp/3.119.0"));

        Assert.Empty(Directory.GetFiles(_workspace.StagedSet(ReleaseManifestBuilder.PackagesArtifactName), "*.nupkg"));
        Assert.DoesNotContain(_output.Output, line => line.StartsWith("##vso[", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Prune_honours_recovery_filters_before_consulting_the_feed()
    {
        await StagedManifestAsync(("SkiaSharp", "3.119.0"), ("HarfBuzzSharp", "8.3.1.5"));

        await Filter(new FakeProbe(), recovery: ["SkiaSharp.*"]);

        var directory = _workspace.StagedSet(ReleaseManifestBuilder.PackagesArtifactName);
        Assert.False(File.Exists(Path.Combine(directory, "SkiaSharp.3.119.0.nupkg")));
        Assert.Contains("Disposition        : PreviouslyAttempted", _output.AllOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Prune_fails_closed_when_a_recovery_filter_matches_nothing()
    {
        await StagedManifestAsync(("SkiaSharp", "3.119.0"));

        await Assert.ThrowsAsync<DotNetReleaseException>(
            () => Filter(new FakeProbe(), recovery: ["Typo.*"]));
    }

    [Fact]
    public async Task Filter_fails_closed_when_the_manifest_hash_does_not_match()
    {
        await StagedManifestAsync(("SkiaSharp", "3.119.0"));

        await Assert.ThrowsAsync<DotNetReleaseException>(
            () => Filter(new FakeProbe(), expectedHash: new string('0', 64)));
    }

    [Fact]
    public async Task Filter_accepts_the_matching_manifest_hash()
    {
        await StagedManifestAsync(("SkiaSharp", "3.119.0"));
        var hash = ReleaseManifestSerializer.ComputeHash(_workspace.ReadManifest());

        await Filter(new FakeProbe(), expectedHash: hash);
    }

    /// <summary>
    /// 1ES pushes whatever its glob matches, so a file the manifest does not list would be
    /// published without ever having been validated.
    /// </summary>
    [Fact]
    public async Task Filter_fails_closed_on_an_unlisted_file_in_the_staging_directory()
    {
        await StagedManifestAsync(("SkiaSharp", "3.119.0"));
        File.WriteAllText(
            Path.Combine(_workspace.StagedSet(ReleaseManifestBuilder.PackagesArtifactName), "Sneaky.1.0.0.nupkg"),
            "not reviewed");

        await Assert.ThrowsAsync<DotNetReleaseException>(() => Filter(new FakeProbe()));
    }

    [Fact]
    public async Task Filter_fails_closed_when_a_staged_package_was_tampered_with()
    {
        await StagedManifestAsync(("SkiaSharp", "3.119.0"));
        File.WriteAllText(
            Path.Combine(_workspace.StagedSet(ReleaseManifestBuilder.PackagesArtifactName), "SkiaSharp.3.119.0.nupkg"),
            "swapped");

        await Assert.ThrowsAsync<DotNetReleaseException>(() => Filter(new FakeProbe()));
    }

    // ---- verify ----

    private Task Verify(INuGetPackageLookup probe, int maxMinutes = 30, string? set = null, int pollSeconds = 20)
    {
        var now = Workspace.Now;

        return VerifyCommand.ExecuteAsync(_output, probe, _workspace.ReadManifest(), TimeSpan.FromMinutes(maxMinutes), TimeSpan.FromSeconds(pollSeconds),
            () => now,
            (delay, _) =>
            {
                now = now.Add(delay);
                return Task.CompletedTask;
            }, set, ManifestHash, CancellationToken.None);
    }

    [Fact]
    public async Task Verify_succeeds_once_every_package_is_indexed()
    {
        await StagedManifestAsync(("SkiaSharp", "3.119.0"));

        await Verify(new FakeProbe("skiasharp/3.119.0"));
    }

    [Fact]
    public async Task Verify_polls_until_indexing_completes()
    {
        await StagedManifestAsync(("SkiaSharp", "3.119.0"));

        var probe = new FakeProbe { AvailableAfterCall = { ["skiasharp/3.119.0"] = 3 } };

        await Verify(probe);
        Assert.Equal(3, probe.Calls);
    }

    [Fact]
    public async Task Verify_fails_with_the_missing_identities_at_the_deadline()
    {
        await StagedManifestAsync(("SkiaSharp", "3.119.0"), ("HarfBuzzSharp", "8.3.1.5"));

        var exception = await Assert.ThrowsAsync<DotNetReleaseException>(
            () => Verify(new FakeProbe("skiasharp/3.119.0"), maxMinutes: 1));
        Assert.Contains("HarfBuzzSharp 8.3.1.5", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("SkiaSharp 3.119.0,", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verification covers packages `prune-published` withheld: they were withheld because they were
    /// already live, so their absence would mean that claim was wrong.
    /// </summary>
    [Fact]
    public async Task Verify_covers_packages_that_pruning_removed()
    {
        await StagedManifestAsync(("SkiaSharp", "3.119.0"), ("HarfBuzzSharp", "8.3.1.5"));
        await Filter(new FakeProbe("skiasharp/3.119.0"));

        var exception = await Assert.ThrowsAsync<DotNetReleaseException>(
            () => Verify(new FakeProbe("harfbuzzsharp/8.3.1.5"), maxMinutes: 1));
        Assert.Contains("SkiaSharp 3.119.0", exception.Message, StringComparison.Ordinal);
    }
}
