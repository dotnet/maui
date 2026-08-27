using DotNet.Release.Core;
using DotNet.Release.NuGet;
using Xunit;

namespace DotNet.Release.Cli.Tests;

public class VerbsTests : IDisposable
{
    private static readonly ChannelReference Libraries = new(".NET Libraries", 1648);

    private readonly Workspace _workspace = new();
    private readonly RecordingConsole _console = new();

    public void Dispose() => _workspace.Dispose();

    private Task<int> Plan(FakeRegistry registry, string repo = "dotnet/skiasharp", int? barId = null) =>
        Verbs.PlanAsync(
            _console, registry, Workspace.PolicyJson, repo, Workspace.Commit, barId,
            _workspace.Out, Workspace.Now, "1.0.0-test", CancellationToken.None);

    private Task<int> Stage(StageOptions? options = null) =>
        Verbs.StageAsync(
            _console, new NupkgIdentityReader(), Workspace.PolicyJson,
            File.ReadAllText(Path.Combine(_workspace.Out, Verbs.PlanFileName)),
            _workspace.Drop, _workspace.Out, options ?? new StageOptions(),
            _workspace.Tool, Workspace.Now, "1.0.0-test", CancellationToken.None);

    // ---- plan ----

    [Fact]
    public async Task Plan_writes_the_resolved_release_and_emits_the_bar_id()
    {
        var exit = await Plan(new FakeRegistry(Workspace.Build(channels: Libraries)));

        Assert.Equal(ExitCodes.Success, exit);
        Assert.True(File.Exists(Path.Combine(_workspace.Out, Verbs.PlanFileName)));

        // The one pipeline variable the tool emits: `darc gather-drop` needs this ID.
        Assert.Contains("##vso[task.setvariable variable=BarId;isOutput=true]4242", _console.Output);
    }

    [Fact]
    public async Task Plan_emits_exactly_one_pipeline_variable()
    {
        await Plan(new FakeRegistry(Workspace.Build(channels: Libraries)));

        Assert.Single(_console.Output, l => l.StartsWith("##vso[", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Plan_fails_closed_for_a_repository_outside_the_policy()
    {
        var exit = await Plan(new FakeRegistry(Workspace.Build(channels: Libraries)), repo: "dotnet/runtime");

        Assert.Equal(ExitCodes.ReleaseError, exit);
        Assert.Contains(ErrorCodes.RepositoryNotAllowed, _console.AllErrors, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(_workspace.Out, Verbs.PlanFileName)));
    }

    [Fact]
    public async Task Plan_fails_closed_when_the_channel_is_missing()
    {
        var exit = await Plan(new FakeRegistry(Workspace.Build()));

        Assert.Equal(ExitCodes.ReleaseError, exit);
        Assert.Contains(ErrorCodes.BarChannelMissing, _console.AllErrors, StringComparison.Ordinal);
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
        var registry = new FakeRegistry(new BarBuild(
            4242, Workspace.Commit, null,
            "https://dev.azure.com/dnceng/internal/_git/dotnet-skiasharp", [Libraries]));

        Assert.Equal(ExitCodes.Success, await Plan(registry, barId: 4242));
        Assert.Contains("AzureDevOpsMirrorConvention", _console.AllOutput, StringComparison.Ordinal);
    }

    // ---- stage ----

    [Fact]
    public async Task Stage_validates_the_drop_and_writes_the_release_plan()
    {
        _workspace.WritePackage("SkiaSharp", "3.119.0");
        _workspace.WritePackage("HarfBuzzSharp", "8.3.1.5");
        await Plan(new FakeRegistry(Workspace.Build(channels: Libraries)));

        var exit = await Stage();

        Assert.Equal(ExitCodes.Success, exit);

        var plan = ReleasePlanSerializer.DeserializePlan(_workspace.ReadPlan()).Value;
        Assert.Equal(2, plan.Sets[0].Packages.Count);
        Assert.Equal(_workspace.Tool, plan.Tool);

        // The staged directory is what 1ES will glob.
        var staged = Directory.GetFiles(_workspace.StagedSet(StagePlanner.PackagesArtifactName), "*.nupkg");
        Assert.Equal(2, staged.Length);
    }

    [Fact]
    public async Task Stage_prints_the_plan_hash_for_the_pipeline_to_pin()
    {
        _workspace.WritePackage("SkiaSharp", "3.119.0");
        await Plan(new FakeRegistry(Workspace.Build(channels: Libraries)));
        await Stage();

        var expected = ReleasePlanSerializer.ComputeHash(_workspace.ReadPlan());
        Assert.Contains($"Release plan SHA-256: {expected}", _console.AllOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Stage_fails_closed_on_an_empty_drop()
    {
        await Plan(new FakeRegistry(Workspace.Build(channels: Libraries)));

        var exit = await Stage();

        Assert.Equal(ExitCodes.ReleaseError, exit);
        Assert.Contains(ErrorCodes.PackageSetEmpty, _console.AllErrors, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Stage_applies_exclude_filters()
    {
        _workspace.WritePackage("SkiaSharp", "3.119.0");
        _workspace.WritePackage("HarfBuzzSharp", "8.3.1.5");
        await Plan(new FakeRegistry(Workspace.Build(channels: Libraries)));

        await Stage(new StageOptions { Exclude = ["HarfBuzz*"] });

        var plan = ReleasePlanSerializer.DeserializePlan(_workspace.ReadPlan()).Value;
        Assert.Equal("SkiaSharp", Assert.Single(plan.Sets[0].Packages).Id);
    }

    [Fact]
    public async Task Stage_rejects_a_workload_manifest_in_a_non_workload_release()
    {
        _workspace.WritePackage("SkiaSharp", "3.119.0");
        _workspace.WritePackage("Microsoft.NET.Sdk.Maui.Manifest-10.0.100", "10.0.0");
        await Plan(new FakeRegistry(Workspace.Build(channels: Libraries)));

        var exit = await Stage();

        Assert.Equal(ExitCodes.ReleaseError, exit);
        Assert.Contains(ErrorCodes.ManifestInNonWorkload, _console.AllErrors, StringComparison.Ordinal);
    }

    // ---- filter ----

    private async Task<ReleasePlan> StagedPlanAsync(params (string Id, string Version)[] packages)
    {
        foreach (var (id, version) in packages)
        {
            _workspace.WritePackage(id, version);
        }

        await Plan(new FakeRegistry(Workspace.Build(channels: Libraries)));
        Assert.Equal(ExitCodes.Success, await Stage());

        return ReleasePlanSerializer.DeserializePlan(_workspace.ReadPlan()).Value;
    }

    private Task<int> Filter(IPackageAvailabilityProbe probe, string[]? skip = null, string? expectedHash = null, string? set = null) =>
        Verbs.FilterAsync(
            _console, probe, _workspace.ReadPlan(), _workspace.Out, skip ?? [], expectedHash, set, CancellationToken.None);

    [Fact]
    public async Task Filter_removes_already_published_packages_from_the_push_set()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"), ("HarfBuzzSharp", "8.3.1.5"));

        var exit = await Filter(new FakeProbe("skiasharp/3.119.0"));

        Assert.Equal(ExitCodes.Success, exit);

        var directory = _workspace.StagedSet(StagePlanner.PackagesArtifactName);
        Assert.False(File.Exists(Path.Combine(directory, "SkiaSharp.3.119.0.nupkg")));
        Assert.True(File.Exists(Path.Combine(directory, "HarfBuzzSharp.8.3.1.5.nupkg")));

        Assert.Contains("##vso[task.setvariable variable=NuGetPackagesToPublish]true", _console.Output);
    }

    [Fact]
    public async Task Filter_writes_the_disposition_sidecar_without_touching_the_plan()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"));
        var beforeFilter = _workspace.ReadPlan();

        await Filter(new FakeProbe("skiasharp/3.119.0"));

        var sidecar = Path.Combine(_workspace.StagedSet(StagePlanner.PackagesArtifactName), Verbs.FilterReportFileName);
        Assert.True(File.Exists(sidecar));
        Assert.Contains("AlreadyPublished", File.ReadAllText(sidecar), StringComparison.Ordinal);

        // The plan is immutable, so the hash pinned by the preparing stage stays valid even
        // though filter deleted a file the plan still lists.
        Assert.Equal(beforeFilter, _workspace.ReadPlan());
    }

    [Fact]
    public async Task Filter_reports_nothing_to_publish_when_everything_is_live()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"));

        await Filter(new FakeProbe("skiasharp/3.119.0"));

        Assert.Contains("##vso[task.setvariable variable=NuGetPackagesToPublish]false", _console.Output);
    }

    [Fact]
    public async Task Filter_honours_recovery_filters_before_consulting_the_feed()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"), ("HarfBuzzSharp", "8.3.1.5"));

        await Filter(new FakeProbe(), skip: ["SkiaSharp.*"]);

        var directory = _workspace.StagedSet(StagePlanner.PackagesArtifactName);
        Assert.False(File.Exists(Path.Combine(directory, "SkiaSharp.3.119.0.nupkg")));

        var sidecar = File.ReadAllText(Path.Combine(directory, Verbs.FilterReportFileName));
        Assert.Contains("PreviouslyAttempted", sidecar, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Filter_fails_closed_when_a_recovery_filter_matches_nothing()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"));

        var exit = await Filter(new FakeProbe(), skip: ["Typo.*"]);

        Assert.Equal(ExitCodes.ReleaseError, exit);
        Assert.Contains(ErrorCodes.FilterUnmatched, _console.AllErrors, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Filter_fails_closed_when_the_plan_hash_does_not_match()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"));

        var exit = await Filter(new FakeProbe(), expectedHash: new string('0', 64));

        Assert.Equal(ExitCodes.ReleaseError, exit);
        Assert.Contains(ErrorCodes.PlanHashMismatch, _console.AllErrors, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Filter_accepts_the_matching_plan_hash()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"));
        var hash = ReleasePlanSerializer.ComputeHash(_workspace.ReadPlan());

        Assert.Equal(ExitCodes.Success, await Filter(new FakeProbe(), expectedHash: hash));
    }

    /// <summary>
    /// 1ES pushes whatever its glob matches, so a file the plan does not list would be
    /// published without ever having been validated.
    /// </summary>
    [Fact]
    public async Task Filter_fails_closed_on_an_unlisted_file_in_the_staging_directory()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"));
        File.WriteAllText(
            Path.Combine(_workspace.StagedSet(StagePlanner.PackagesArtifactName), "Sneaky.1.0.0.nupkg"),
            "not reviewed");

        var exit = await Filter(new FakeProbe());

        Assert.Equal(ExitCodes.ReleaseError, exit);
        Assert.Contains(ErrorCodes.PackageFileUnexpected, _console.AllErrors, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Filter_fails_closed_when_a_staged_package_was_tampered_with()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"));
        File.WriteAllText(
            Path.Combine(_workspace.StagedSet(StagePlanner.PackagesArtifactName), "SkiaSharp.3.119.0.nupkg"),
            "swapped");

        var exit = await Filter(new FakeProbe());

        Assert.Equal(ExitCodes.ReleaseError, exit);
        Assert.Contains(ErrorCodes.PackageHashMismatch, _console.AllErrors, StringComparison.Ordinal);
    }

    // ---- verify ----

    private Task<int> Verify(IPackageAvailabilityProbe probe, int maxMinutes = 30, string? set = null, int pollSeconds = 20)
    {
        var now = Workspace.Now;

        return Verbs.VerifyAsync(
            _console, probe, _workspace.ReadPlan(),
            TimeSpan.FromMinutes(maxMinutes), TimeSpan.FromSeconds(pollSeconds),
            () => now,
            (delay, _) =>
            {
                now = now.Add(delay);
                return Task.CompletedTask;
            },
            set,
            CancellationToken.None);
    }

    [Fact]
    public async Task Verify_succeeds_once_every_package_is_indexed()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"));

        Assert.Equal(ExitCodes.Success, await Verify(new FakeProbe("skiasharp/3.119.0")));
    }

    [Fact]
    public async Task Verify_polls_until_indexing_completes()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"));

        var probe = new FakeProbe { AvailableAfterCall = { ["skiasharp/3.119.0"] = 3 } };

        Assert.Equal(ExitCodes.Success, await Verify(probe));
        Assert.Equal(3, probe.Calls);
    }

    [Fact]
    public async Task Verify_fails_with_the_missing_identities_at_the_deadline()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"), ("HarfBuzzSharp", "8.3.1.5"));

        var exit = await Verify(new FakeProbe("skiasharp/3.119.0"), maxMinutes: 1);

        Assert.Equal(ExitCodes.ReleaseError, exit);
        Assert.Contains("HarfBuzzSharp 8.3.1.5", _console.AllErrors, StringComparison.Ordinal);
        Assert.DoesNotContain("SkiaSharp 3.119.0,", _console.AllErrors, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verification covers packages `filter` withheld: they were withheld because they were
    /// already live, so their absence would mean that claim was wrong.
    /// </summary>
    [Fact]
    public async Task Verify_covers_packages_that_filter_removed()
    {
        await StagedPlanAsync(("SkiaSharp", "3.119.0"), ("HarfBuzzSharp", "8.3.1.5"));
        await Filter(new FakeProbe("skiasharp/3.119.0"));

        var exit = await Verify(new FakeProbe("harfbuzzsharp/8.3.1.5"), maxMinutes: 1);

        Assert.Equal(ExitCodes.ReleaseError, exit);
        Assert.Contains("SkiaSharp 3.119.0", _console.AllErrors, StringComparison.Ordinal);
    }
}
