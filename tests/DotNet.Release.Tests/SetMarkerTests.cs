using Xunit;

namespace DotNet.Release.Tests;

/// <summary>Verifies that each artifact directory is bound to its planned package set.</summary>
public class SetMarkerTests : IDisposable
{
    private readonly Workspace _workspace = new();
    private readonly RecordingConsole _console = new();

    /// <summary>The pin the preparing stage computes. Production always supplies it.</summary>
    private string PlanHash => ReleasePlanSerializer.ComputeHash(_workspace.ReadPlan());

    public void Dispose() => _workspace.Dispose();

    private const string Pack = "Microsoft.Maui.Controls";
    private const string Manifest = "Microsoft.NET.Sdk.Maui.Manifest-10.0.100";

    private async Task StageWorkloadAsync()
    {
        _workspace.WritePackage(Pack, "10.0.0");
        _workspace.WritePackage(Manifest, "10.0.0");

        await PlanCommand.ExecuteAsync(
            _console, new FakeRegistry(Workspace.Build("https://github.com/dotnet/maui")),
            Workspace.PolicyJson, "dotnet/maui", Workspace.Commit, null,
            _workspace.Out, Workspace.Now, "1.0.0-test", CancellationToken.None);

        Assert.Equal(ExitCodes.Success, await StageCommand.ExecuteAsync(
            _console, Workspace.PolicyJson,
            File.ReadAllText(Path.Combine(_workspace.Out, PlanCommand.FileName)),
            _workspace.Drop, _workspace.Out, new StageOptions(),
            Workspace.Now, "1.0.0-test", CancellationToken.None));
    }

    private Task<int> Filter(string? set, IReadOnlyList<string>? recovery = null) =>
        PrunePublishedCommand.ExecuteAsync(
            _console,
            new FakeProbe(),
            _workspace.ReadPlan(),
            _workspace.Out,
            recovery ?? [],
            PlanHash,
            set,
            CancellationToken.None);

    private string SetDirectory(string artifactName) => Path.Combine(_workspace.Out, artifactName);

    // ---- the root plan and per-set markers bind the artifact ----

    [Fact]
    public async Task The_artifact_contains_one_root_plan_and_a_marker_in_each_set()
    {
        await StageWorkloadAsync();

        Assert.True(File.Exists(Path.Combine(_workspace.Out, ReleaseArtifact.PlanFileName)));
        foreach (var artifact in new[] { StagePlanner.PacksArtifactName, StagePlanner.ManifestsArtifactName })
        {
            Assert.False(File.Exists(Path.Combine(SetDirectory(artifact), ReleaseArtifact.PlanFileName)));
            Assert.True(File.Exists(Path.Combine(SetDirectory(artifact), ReleaseSetMarker.FileName)));
        }
    }

    [Fact]
    public async Task The_marker_declares_the_set_and_the_release()
    {
        await StageWorkloadAsync();

        var marker = ReleasePlanSerializer.DeserializeSetMarker(
            File.ReadAllText(Path.Combine(SetDirectory(StagePlanner.PacksArtifactName), ReleaseSetMarker.FileName))).Value;

        Assert.Equal(StagePlanner.PacksArtifactName, marker.ArtifactName);
        Assert.Equal("Workload packs", marker.SetName);
        Assert.Equal(4242, marker.BarBuildId);
        Assert.Equal(Workspace.Commit, marker.Commit);
    }

    // ---- the hole PACKAGE_SET_NOT_FOUND does not close ----

    /// <summary>
    /// The case that motivated the marker. A misspelled set is caught by
    /// PACKAGE_SET_NOT_FOUND; a <i>valid but wrong</i> one resolves cleanly and would
    /// otherwise only fail later as a generic missing-file error — the same symptom a broken
    /// artifact upload produces.
    /// </summary>
    [Fact]
    public async Task A_valid_but_wrong_set_is_named_precisely_not_reported_as_missing_files()
    {
        await StageWorkloadAsync();

        // Simulate the packs stage: only its own artifact is present, but the template asks
        // for the manifests set.
        Directory.Delete(SetDirectory(StagePlanner.ManifestsArtifactName), recursive: true);
        Directory.Move(SetDirectory(StagePlanner.PacksArtifactName), SetDirectory(StagePlanner.ManifestsArtifactName));

        var exit = await Filter(StagePlanner.ManifestsArtifactName);

        Assert.Equal(ExitCodes.ReleaseError, exit);
        Assert.Contains(ErrorCodes.PackageSetMismatch, _console.AllErrors, StringComparison.Ordinal);

        // The diagnosis must name the wiring, not the symptom.
        Assert.Contains("wired to the wrong package set", _console.AllErrors, StringComparison.Ordinal);
        Assert.DoesNotContain(ErrorCodes.PackageFileMissing, _console.AllErrors, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_missing_marker_fails_closed()
    {
        await StageWorkloadAsync();
        File.Delete(Path.Combine(SetDirectory(StagePlanner.PacksArtifactName), ReleaseSetMarker.FileName));

        var exit = await Filter(StagePlanner.PacksArtifactName);

        Assert.Equal(ExitCodes.ReleaseError, exit);
        Assert.Contains(ErrorCodes.PackageSetMismatch, _console.AllErrors, StringComparison.Ordinal);
    }

    /// <summary>
    /// A directory from a different release would otherwise pass the name check and then
    /// fail as a pile of missing files.
    /// </summary>
    [Fact]
    public async Task A_marker_from_a_different_release_fails_closed()
    {
        await StageWorkloadAsync();

        var markerPath = Path.Combine(SetDirectory(StagePlanner.PacksArtifactName), ReleaseSetMarker.FileName);
        var marker = ReleasePlanSerializer.DeserializeSetMarker(File.ReadAllText(markerPath)).Value;

        File.WriteAllText(markerPath, ReleasePlanSerializer.Serialize(marker with { BarBuildId = 999999 }));

        var exit = await Filter(StagePlanner.PacksArtifactName);

        Assert.Equal(ExitCodes.ReleaseError, exit);
        Assert.Contains(ErrorCodes.PackageSetMismatch, _console.AllErrors, StringComparison.Ordinal);
        Assert.Contains("999999", _console.AllErrors, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_correctly_wired_stage_passes_the_marker_check()
    {
        await StageWorkloadAsync();

        Assert.Equal(ExitCodes.Success, await Filter(StagePlanner.PacksArtifactName));
        Assert.Equal(ExitCodes.Success, await Filter(StagePlanner.ManifestsArtifactName));
    }

    [Fact]
    public async Task One_recovery_filter_can_resume_both_workload_stages()
    {
        await StageWorkloadAsync();
        string[] recovery = ["*Manifest*"];

        Assert.Equal(ExitCodes.Success, await Filter(StagePlanner.PacksArtifactName, recovery));
        Assert.True(File.Exists(Path.Combine(
            SetDirectory(StagePlanner.PacksArtifactName),
            $"{Pack}.10.0.0.nupkg")));

        Assert.Equal(ExitCodes.Success, await Filter(StagePlanner.ManifestsArtifactName, recovery));
        Assert.False(File.Exists(Path.Combine(
            SetDirectory(StagePlanner.ManifestsArtifactName),
            $"{Manifest}.10.0.0.nupkg")));
    }

    /// <summary>
    /// Tampering can only cause a failure, never a silently wrong publish: identities still
    /// come from the hashed plan, so a swapped marker just fails the check.
    /// </summary>
    [Fact]
    public async Task A_tampered_marker_can_only_cause_a_failure()
    {
        await StageWorkloadAsync();

        var packs = Path.Combine(SetDirectory(StagePlanner.PacksArtifactName), ReleaseSetMarker.FileName);
        var manifests = Path.Combine(SetDirectory(StagePlanner.ManifestsArtifactName), ReleaseSetMarker.FileName);
        (var a, var b) = (File.ReadAllText(packs), File.ReadAllText(manifests));
        File.WriteAllText(packs, b);
        File.WriteAllText(manifests, a);

        Assert.Equal(ExitCodes.ReleaseError, await Filter(StagePlanner.PacksArtifactName));
        Assert.Contains(ErrorCodes.PackageSetMismatch, _console.AllErrors, StringComparison.Ordinal);
    }

    /// <summary>Verify carries the same wiring risk, so it applies the same check.</summary>
    [Fact]
    public async Task Verify_also_rejects_a_wrongly_wired_set()
    {
        await StageWorkloadAsync();

        Directory.Delete(SetDirectory(StagePlanner.ManifestsArtifactName), recursive: true);
        Directory.Move(SetDirectory(StagePlanner.PacksArtifactName), SetDirectory(StagePlanner.ManifestsArtifactName));

        var now = Workspace.Now;
        var exit = await VerifyCommand.ExecuteAsync(
            _console, new FakeProbe(), _workspace.ReadPlan(),
            TimeSpan.FromMinutes(30), TimeSpan.FromSeconds(20),
            () => now, (d, _) => { now = now.Add(d); return Task.CompletedTask; },
            StagePlanner.ManifestsArtifactName, _workspace.Out, PlanHash, CancellationToken.None);

        Assert.Equal(ExitCodes.ReleaseError, exit);
        Assert.Contains(ErrorCodes.PackageSetMismatch, _console.AllErrors, StringComparison.Ordinal);
    }

    /// <summary>
    /// The check is only enforced when a set was named, because that is the assertion which
    /// can be wrong. Omitting --set keeps the local, whole-plan workflow usable.
    /// </summary>
    [Fact]
    public async Task Omitting_the_set_does_not_require_markers()
    {
        await StageWorkloadAsync();
        File.Delete(Path.Combine(SetDirectory(StagePlanner.PacksArtifactName), ReleaseSetMarker.FileName));

        Assert.Equal(ExitCodes.Success, await Filter(set: null));
    }

    // ---- the companion files must not trip the unexpected-file rule ----

    /// <summary>
    /// `stage` deliberately places non-package files in a directory guarded by a fail-closed
    /// unexpected-file rule. That rule is scoped by extension, so companion files are never
    /// observed and cannot trip it.
    /// </summary>
    [Fact]
    public async Task The_root_plan_and_set_marker_do_not_trip_the_unexpected_file_rule()
    {
        await StageWorkloadAsync();

        var directory = SetDirectory(StagePlanner.PacksArtifactName);
        Assert.True(File.Exists(Path.Combine(_workspace.Out, ReleaseArtifact.PlanFileName)));
        Assert.True(File.Exists(Path.Combine(directory, ReleaseSetMarker.FileName)));

        Assert.Equal(ExitCodes.Success, await Filter(StagePlanner.PacksArtifactName));
    }

    /// <summary>
    /// The scoping must hold for files nobody has thought of yet — otherwise the first new
    /// companion file forces someone to widen the filter, which is how this kind of check
    /// gets silently disabled.
    /// </summary>
    [Theory]
    [InlineData("release-audit.md")]
    [InlineData("diagnostics/release.log")]
    [InlineData("some-future-file.txt")]
    [InlineData("nupkg-lookalike.nupkg.txt")]
    public async Task An_arbitrary_future_companion_file_does_not_trip_the_rule(string relativePath)
    {
        await StageWorkloadAsync();

        var path = Path.Combine(SetDirectory(StagePlanner.PacksArtifactName), relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, "companion", CancellationToken.None);

        Assert.Equal(ExitCodes.Success, await Filter(StagePlanner.PacksArtifactName));
    }

    /// <summary>
    /// The rule still does its job: a stray package sharing the directory with the companion
    /// files is rejected, because it is a package.
    /// </summary>
    [Fact]
    public async Task A_stray_package_alongside_the_companions_is_still_rejected()
    {
        await StageWorkloadAsync();

        await File.WriteAllTextAsync(
            Path.Combine(SetDirectory(StagePlanner.PacksArtifactName), "Sneaky.1.0.0.nupkg"),
            "not reviewed",
            CancellationToken.None);

        Assert.Equal(ExitCodes.ReleaseError, await Filter(StagePlanner.PacksArtifactName));
        Assert.Contains(ErrorCodes.PackageFileUnexpected, _console.AllErrors, StringComparison.Ordinal);
    }
}
