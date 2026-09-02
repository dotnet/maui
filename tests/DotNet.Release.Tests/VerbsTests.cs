using Xunit;

namespace DotNet.Release.Tests;

public class VerbsTests : IDisposable
{
    private static readonly ChannelReference Libraries = new(".NET Libraries", 1648);

    private readonly Workspace _workspace = new();
    private readonly RecordingWriter _output = new();

    /// <summary>The pin the preparing stage computes. Production always supplies it.</summary>
    private string PlanHash => ReleasePlanSerializer.ComputeHash(_workspace.ReadPlan());

    public void Dispose() => _workspace.Dispose();

    private Task Plan(FakeRegistry registry, string repo = "dotnet/skiasharp", int? barId = null) => PlanCommand.ExecuteAsync(
            _output, registry, Workspace.PolicyJson, repo, Workspace.Commit, barId, _workspace.Out, Workspace.Now, "1.0.0-test", CancellationToken.None);

    private Task Stage(StageOptions? options = null) => StageCommand.ExecuteAsync(_output, Workspace.PolicyJson,
            File.ReadAllText(Path.Combine(_workspace.Out, PlanCommand.FileName)), _workspace.Drop, _workspace.Out, options ?? new StageOptions(),
            Workspace.Now, "1.0.0-test", CancellationToken.None);

    // ---- plan ----

    [Fact]
    public async Task Plan_writes_the_resolved_release_and_emits_pipeline_outputs()
    {
        await Plan(new FakeRegistry(Workspace.Build(channels: Libraries)));
        Assert.True(File.Exists(Path.Combine(_workspace.Out, PlanCommand.FileName)));

        var output = _output.AllOutput;
        Assert.Contains("Schema version    : 1", output, StringComparison.Ordinal);
        Assert.Contains("Tool version      : 1.0.0-test", output, StringComparison.Ordinal);
        Assert.Contains($"Created UTC       : {Workspace.Now:O}", output, StringComparison.Ordinal);
        Assert.Contains("Repository        : dotnet/skiasharp", output, StringComparison.Ordinal);
        Assert.Contains(
            "Repository URL    : https://github.com/dotnet/skiasharp",
            output, StringComparison.Ordinal);
        Assert.Contains($"Commit            : {Workspace.Commit}", output, StringComparison.Ordinal);
        Assert.Contains("BAR build ID      : 4242", output, StringComparison.Ordinal);
        Assert.Contains("Repository origin : GitHubRepository", output, StringComparison.Ordinal);
        Assert.Contains("Workload          : False", output, StringComparison.Ordinal);
        Assert.Contains("Channel name      : .NET Libraries", output, StringComparison.Ordinal);
        Assert.Contains("Channel ID        : 1648", output, StringComparison.Ordinal);
        Assert.Contains("##vso[task.setvariable variable=BarId;isOutput=true]4242", _output.Output);
        Assert.Contains("##vso[task.setvariable variable=IsWorkload;isOutput=true]false", _output.Output);
    }

    [Fact]
    public async Task Plan_emits_bar_id_and_workload_classification()
    {
        await Plan(new FakeRegistry(Workspace.Build(channels: Libraries)));

        Assert.Equal(2, _output.Output.Count(l => l.StartsWith("##vso[", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task Plan_fails_closed_for_a_repository_outside_the_policy()
    {
        var exception = await Assert.ThrowsAsync<DotNetReleaseException>(
            () => Plan(new FakeRegistry(Workspace.Build(channels: Libraries)), repo: "dotnet/runtime"));
        Assert.Contains("not enabled for release", exception.Message, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(_workspace.Out, PlanCommand.FileName)));
    }

    [Fact]
    public async Task Plan_fails_closed_when_the_channel_is_missing()
    {
        await Assert.ThrowsAsync<DotNetReleaseException>(
            () => Plan(new FakeRegistry(Workspace.Build())));
    }

    [Fact]
    public async Task Plan_uses_the_bar_id_lookup_when_one_is_supplied()
    {
        var registry = new FakeRegistry(Workspace.Build(channels: Libraries));

        await Plan(registry, barId: 4242);

        Assert.Equal(4242, registry.RequestedBarId);
        Assert.Null(registry.RequestedCommit);
    }

    [Fact]
    public async Task Plan_records_that_identity_came_from_the_mirror_convention()
    {
        var registry = new FakeRegistry(new BarBuild(4242, Workspace.Commit, null,
            "https://dev.azure.com/dnceng/internal/_git/dotnet-skiasharp", [Libraries]));

        await Plan(registry, barId: 4242);
        Assert.Contains("AzureDevOpsMirrorConvention", _output.AllOutput, StringComparison.Ordinal);
    }

    // ---- stage ----

    [Fact]
    public async Task Stage_validates_the_drop_and_writes_the_release_plan()
    {
        _workspace.WritePackage("SkiaSharp", "3.119.0.0");
        _workspace.WritePackage("HarfBuzzSharp", "8.3.1.5");
        await Plan(new FakeRegistry(Workspace.Build(channels: Libraries)));
        _output.Output.Clear();

        await Stage();

        var plan = ReleasePlanSerializer.DeserializePlan(_workspace.ReadPlan());
        var skia = plan.AllPackages.Single(package => package.Id == "SkiaSharp");
        Assert.Equal(2, plan.Sets[0].Packages.Count);
        // The staged directory is what 1ES will glob.
        var staged = Directory.GetFiles(_workspace.StagedSet(StagePlanner.PackagesArtifactName), "*.nupkg");
        Assert.Equal(2, staged.Length);

        var output = _output.AllOutput;
        Assert.Contains("Release plan:", output, StringComparison.Ordinal);
        Assert.Contains("Schema version : 1", output, StringComparison.Ordinal);
        Assert.Contains("Tool version   : 1.0.0-test", output, StringComparison.Ordinal);
        Assert.Contains($"Created UTC    : {Workspace.Now:O}", output, StringComparison.Ordinal);
        Assert.Contains("Repository        : dotnet/skiasharp", output, StringComparison.Ordinal);
        Assert.Contains("Workload target : (none)", output, StringComparison.Ordinal);
        Assert.Contains("Name          : NuGet packages", output, StringComparison.Ordinal);
        Assert.Contains("Order         : 0", output, StringComparison.Ordinal);
        Assert.Contains($"Artifact name : {StagePlanner.PackagesArtifactName}", output, StringComparison.Ordinal);
        Assert.Contains("ID                 : SkiaSharp", output, StringComparison.Ordinal);
        Assert.Contains($"Raw version        : {skia.Version}", output, StringComparison.Ordinal);
        Assert.Contains($"Normalized version : {skia.NormalizedVersion}", output, StringComparison.Ordinal);
        Assert.Contains("File name          : SkiaSharp.3.119.0.0.nupkg", output, StringComparison.Ordinal);
        Assert.Contains($"SHA-256            : {skia.Sha256}", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Stage_prints_the_plan_hash_for_the_pipeline_to_pin()
    {
        _workspace.WritePackage("SkiaSharp", "3.119.0");
        await Plan(new FakeRegistry(Workspace.Build(channels: Libraries)));
        await Stage();

        var expected = ReleasePlanSerializer.ComputeHash(_workspace.ReadPlan());
        Assert.Contains($"Release plan SHA-256: {expected}", _output.AllOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Stage_fails_closed_on_an_empty_drop()
    {
        await Plan(new FakeRegistry(Workspace.Build(channels: Libraries)));

        await Assert.ThrowsAsync<DotNetReleaseException>(() => Stage());
    }

    [Fact]
    public async Task Stage_does_not_fall_back_to_packages_outside_the_shipping_directory()
    {
        var package = _workspace.WritePackage("SkiaSharp", "3.119.0");
        var outsideShipping = Path.Combine(_workspace.Drop, Path.GetFileName(package));
        File.Move(package, outsideShipping);
        Directory.Delete(Path.Combine(_workspace.Drop, "shipping"), recursive: true);
        await Plan(new FakeRegistry(Workspace.Build(channels: Libraries)));

        await Assert.ThrowsAsync<DotNetReleaseException>(() => Stage());
    }

    [Fact]
    public async Task Stage_applies_exclude_filters()
    {
        _workspace.WritePackage("SkiaSharp", "3.119.0");
        _workspace.WritePackage("HarfBuzzSharp", "8.3.1.5");
        await Plan(new FakeRegistry(Workspace.Build(channels: Libraries)));

        await Stage(new StageOptions { Exclude = ["HarfBuzz*"] });

        var plan = ReleasePlanSerializer.DeserializePlan(_workspace.ReadPlan());
        Assert.Equal("SkiaSharp", Assert.Single(plan.Sets[0].Packages).Id);
    }

    [Fact]
    public async Task Stage_rejects_a_workload_manifest_in_a_non_workload_release()
    {
        _workspace.WritePackage("SkiaSharp", "3.119.0");
        _workspace.WritePackage("Microsoft.NET.Sdk.Maui.Manifest-10.0.100", "10.0.0");
        await Plan(new FakeRegistry(Workspace.Build(channels: Libraries)));

        await Assert.ThrowsAsync<DotNetReleaseException>(() => Stage());
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
    [InlineData("release-prune.json")]
    public void A_single_path_component_is_accepted(string value)
    {
        Assert.True(ReleaseArtifact.IsSinglePathComponent(value));
    }

    private async Task<ReleasePlan> StagedPlanAsync(params (string Id, string Version)[] packages)
    {
        foreach (var (id, version) in packages)
        {
            _workspace.WritePackage(id, version);
        }

        await Plan(new FakeRegistry(Workspace.Build(channels: Libraries)));
        await Stage();

        return ReleasePlanSerializer.DeserializePlan(_workspace.ReadPlan());
    }

    private Task Filter(INuGetPackageLookup probe, string[]? recovery = null, string? expectedHash = null, string? set = null) =>
        PrunePublishedCommand.ExecuteAsync(
            _output, probe, _workspace.ReadPlan(), _workspace.Out, recovery ?? [], expectedHash ?? PlanHash, set, CancellationToken.None);

    [Fact]
    public async Task Filter_removes_already_published_packages_from_the_push_set()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"), ("HarfBuzzSharp", "8.3.1.5"));

        await Filter(new FakeProbe("skiasharp/3.119.0"));

        var directory = _workspace.StagedSet(StagePlanner.PackagesArtifactName);
        Assert.False(File.Exists(Path.Combine(directory, "SkiaSharp.3.119.0.nupkg")));
        Assert.True(File.Exists(Path.Combine(directory, "HarfBuzzSharp.8.3.1.5.nupkg")));

        Assert.Contains("##vso[task.setvariable variable=NuGetPackagesToPublish]true", _output.Output);
    }

    [Fact]
    public async Task Prune_logs_release_identity_and_every_decision_field_and_disposition()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"), ("HarfBuzzSharp", "8.3.1.5"), ("Pending.Package", "1.2.3"));
        _output.Output.Clear();

        await Filter(new FakeProbe("skiasharp/3.119.0"), recovery: ["HarfBuzzSharp.*"]);

        var output = _output.AllOutput;
        Assert.Contains("Selected release:", output, StringComparison.Ordinal);
        Assert.Contains("Repository        : dotnet/skiasharp", output, StringComparison.Ordinal);
        Assert.Contains($"Commit            : {Workspace.Commit}", output, StringComparison.Ordinal);
        Assert.Contains("BAR build ID      : 4242", output, StringComparison.Ordinal);
        Assert.Contains($"Plan SHA-256 : {PlanHash}", output, StringComparison.Ordinal);
        Assert.Contains("Schema version : 1", output, StringComparison.Ordinal);
        Assert.Contains("Set name       : NuGet packages", output, StringComparison.Ordinal);
        Assert.Contains($"Artifact name  : {StagePlanner.PackagesArtifactName}", output, StringComparison.Ordinal);
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
        var pendingPackage = ReleasePlanSerializer.DeserializePlan(_workspace.ReadPlan()).AllPackages
            .Single(package => package.Id == "Pending.Package");
        Assert.Contains($"SHA-256            : {pendingPackage.Sha256}", output, StringComparison.Ordinal);
        Assert.Contains("Disposition        : Pending", output, StringComparison.Ordinal);
        Assert.Contains("Pending count : 1", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Filter_writes_the_disposition_sidecar_without_touching_the_plan()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"));
        var beforeFilter = _workspace.ReadPlan();

        await Filter(new FakeProbe("skiasharp/3.119.0"));

        var sidecar = Path.Combine(_workspace.StagedSet(StagePlanner.PackagesArtifactName), PrunePublishedCommand.ReportFileName);
        Assert.True(File.Exists(sidecar));
        Assert.Contains("AlreadyPublished", File.ReadAllText(sidecar), StringComparison.Ordinal);

        // The plan is immutable, so the hash pinned by the preparing stage stays valid even
        // though pruning deleted a file the plan still lists.
        Assert.Equal(beforeFilter, _workspace.ReadPlan());
    }

    [Fact]
    public async Task Filter_can_run_twice_without_restaging()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"), ("HarfBuzzSharp", "8.3.1.5"));
        var lookup = new FakeProbe("skiasharp/3.119.0");

        await Filter(lookup);
        await Filter(lookup);

        var directory = _workspace.StagedSet(StagePlanner.PackagesArtifactName);
        Assert.False(File.Exists(Path.Combine(directory, "SkiaSharp.3.119.0.nupkg")));
        Assert.True(File.Exists(Path.Combine(directory, "HarfBuzzSharp.8.3.1.5.nupkg")));
    }

    [Fact]
    public async Task Filter_reports_nothing_to_publish_when_everything_is_live()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"));

        await Filter(new FakeProbe("skiasharp/3.119.0"));

        Assert.Contains("##vso[task.setvariable variable=NuGetPackagesToPublish]false", _output.Output);
    }

    [Fact]
    public async Task Prune_honours_recovery_filters_before_consulting_the_feed()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"), ("HarfBuzzSharp", "8.3.1.5"));

        await Filter(new FakeProbe(), recovery: ["SkiaSharp.*"]);

        var directory = _workspace.StagedSet(StagePlanner.PackagesArtifactName);
        Assert.False(File.Exists(Path.Combine(directory, "SkiaSharp.3.119.0.nupkg")));

        var sidecar = File.ReadAllText(Path.Combine(directory, PrunePublishedCommand.ReportFileName));
        Assert.Contains("PreviouslyAttempted", sidecar, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Prune_fails_closed_when_a_recovery_filter_matches_nothing()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"));

        await Assert.ThrowsAsync<DotNetReleaseException>(
            () => Filter(new FakeProbe(), recovery: ["Typo.*"]));
    }

    [Fact]
    public async Task Filter_fails_closed_when_the_plan_hash_does_not_match()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"));

        await Assert.ThrowsAsync<DotNetReleaseException>(
            () => Filter(new FakeProbe(), expectedHash: new string('0', 64)));
    }

    [Fact]
    public async Task Filter_accepts_the_matching_plan_hash()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"));
        var hash = ReleasePlanSerializer.ComputeHash(_workspace.ReadPlan());

        await Filter(new FakeProbe(), expectedHash: hash);
    }

    /// <summary>
    /// 1ES pushes whatever its glob matches, so a file the plan does not list would be
    /// published without ever having been validated.
    /// </summary>
    [Fact]
    public async Task Filter_fails_closed_on_an_unlisted_file_in_the_staging_directory()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"));
        File.WriteAllText(Path.Combine(_workspace.StagedSet(StagePlanner.PackagesArtifactName), "Sneaky.1.0.0.nupkg"), "not reviewed");

        await Assert.ThrowsAsync<DotNetReleaseException>(() => Filter(new FakeProbe()));
    }

    [Fact]
    public async Task Filter_fails_closed_when_a_staged_package_was_tampered_with()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"));
        File.WriteAllText(Path.Combine(_workspace.StagedSet(StagePlanner.PackagesArtifactName), "SkiaSharp.3.119.0.nupkg"), "swapped");

        await Assert.ThrowsAsync<DotNetReleaseException>(() => Filter(new FakeProbe()));
    }

    // ---- verify ----

    private Task Verify(INuGetPackageLookup probe, int maxMinutes = 30, string? set = null, int pollSeconds = 20)
    {
        var now = Workspace.Now;

        return VerifyCommand.ExecuteAsync(_output, probe, _workspace.ReadPlan(), TimeSpan.FromMinutes(maxMinutes), TimeSpan.FromSeconds(pollSeconds),
            () => now,
            (delay, _) =>
            {
                now = now.Add(delay);
                return Task.CompletedTask;
            }, set, PlanHash, CancellationToken.None);
    }

    [Fact]
    public async Task Verify_succeeds_once_every_package_is_indexed()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"));

        await Verify(new FakeProbe("skiasharp/3.119.0"));
    }

    [Fact]
    public async Task Verify_polls_until_indexing_completes()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"));

        var probe = new FakeProbe { AvailableAfterCall = { ["skiasharp/3.119.0"] = 3 } };

        await Verify(probe);
        Assert.Equal(3, probe.Calls);
    }

    [Fact]
    public async Task Verify_fails_with_the_missing_identities_at_the_deadline()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"), ("HarfBuzzSharp", "8.3.1.5"));

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
        await StagedPlanAsync(("SkiaSharp", "3.119.0"), ("HarfBuzzSharp", "8.3.1.5"));
        await Filter(new FakeProbe("skiasharp/3.119.0"));

        var exception = await Assert.ThrowsAsync<DotNetReleaseException>(
            () => Verify(new FakeProbe("harfbuzzsharp/8.3.1.5"), maxMinutes: 1));
        Assert.Contains("SkiaSharp 3.119.0", exception.Message, StringComparison.Ordinal);
    }
}
