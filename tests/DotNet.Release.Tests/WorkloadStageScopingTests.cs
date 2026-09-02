using Xunit;

namespace DotNet.Release.Tests;

/// <summary>
/// Covers the two-stage workload release, where packs and manifests are published by
/// separate stages that each download only their own artifact.
/// </summary>
/// <remarks>
/// This is the shape that a single-set release cannot exercise. Without set scoping, the
/// packs stage looks for manifest files in a directory that does not exist and waits for
/// manifests that have not been pushed yet — so a workload release could never succeed,
/// while a non-workload one worked by accident.
/// </remarks>
public class WorkloadStageScopingTests : IDisposable
{
    private readonly Workspace _workspace = new();
    private readonly RecordingWriter _output = new();

    /// <summary>The pin the preparing stage computes. Production always supplies it.</summary>
    private string PlanHash => ReleasePlanSerializer.ComputeHash(_workspace.ReadPlan());

    public void Dispose() => _workspace.Dispose();

    private const string Pack = "Microsoft.Maui.Controls";
    private const string Manifest = "Microsoft.NET.Sdk.Maui.Manifest-10.0.100";

    /// <summary>Stages a workload release with one pack and one manifest.</summary>
    private async Task<ReleasePlan> StageWorkloadAsync()
    {
        _workspace.WritePackage(Pack, "10.0.0");
        _workspace.WritePackage(Manifest, "10.0.0");

        var registry = new FakeRegistry(Workspace.Build("https://github.com/dotnet/maui"));

        await PlanCommand.ExecuteAsync(_output, registry, Workspace.PolicyJson, "dotnet/maui", Workspace.Commit, null,
            _workspace.Out, Workspace.Now, "1.0.0-test", CancellationToken.None);

        await StageCommand.ExecuteAsync(_output, Workspace.PolicyJson, File.ReadAllText(Path.Combine(_workspace.Out, PlanCommand.FileName)),
            _workspace.Drop, _workspace.Out, new StageOptions(), Workspace.Now, "1.0.0-test", CancellationToken.None);

        var plan = ReleasePlanSerializer.DeserializePlan(_workspace.ReadPlan());
        Assert.Equal(2, plan.Sets.Count);
        return plan;
    }

    /// <summary>Simulates a publish stage: only its own artifact directory is present.</summary>
    private void KeepOnly(string artifactName)
    {
        foreach (var directory in Directory.GetDirectories(_workspace.Out))
        {
            if (!string.Equals(Path.GetFileName(directory), artifactName, StringComparison.OrdinalIgnoreCase))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    private Task Filter(string? set, INuGetPackageLookup probe) => PrunePublishedCommand.ExecuteAsync(_output, probe, _workspace.ReadPlan(), _workspace.Out,
            [], PlanHash, set, CancellationToken.None);

    private Task Verify(string? set, INuGetPackageLookup probe, int maxMinutes = 30)
    {
        var now = Workspace.Now;

        return VerifyCommand.ExecuteAsync(_output, probe, _workspace.ReadPlan(), TimeSpan.FromMinutes(maxMinutes), TimeSpan.FromSeconds(20),
            () => now,
            (delay, _) => { now = now.Add(delay); return Task.CompletedTask; }, set, PlanHash, CancellationToken.None);
    }

    [Fact]
    public async Task Stage_logs_the_workload_target_sets_and_package_hashes()
    {
        var plan = await StageWorkloadAsync();
        var package = plan.AllPackages.First();
        var output = _output.AllOutput;

        Assert.Contains("Workload target:", output, StringComparison.Ordinal);
        Assert.Contains("Band    : 10", output, StringComparison.Ordinal);
        Assert.Contains("Channel : .NET 10 Workload Release", output, StringComparison.Ordinal);
        Assert.Contains("Feed    : dotnet10-workloads", output, StringComparison.Ordinal);
        Assert.Contains("Name          : Workload packs", output, StringComparison.Ordinal);
        Assert.Contains("Order         : 0", output, StringComparison.Ordinal);
        Assert.Contains($"Artifact name : {StagePlanner.PacksArtifactName}", output, StringComparison.Ordinal);
        Assert.Contains("Name          : Workload manifests", output, StringComparison.Ordinal);
        Assert.Contains("Order         : 1", output, StringComparison.Ordinal);
        Assert.Contains($"Artifact name : {StagePlanner.ManifestsArtifactName}", output, StringComparison.Ordinal);
        Assert.Contains($"File name          : {package.FileName}", output, StringComparison.Ordinal);
        Assert.Contains($"SHA-256            : {package.Sha256}", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Packs_stage_prunes_only_its_own_set()
    {
        await StageWorkloadAsync();
        KeepOnly(StagePlanner.PacksArtifactName);

        // Staging logged both set names, so inspect only the prune command output.
        _output.Output.Clear();

        await Filter(StagePlanner.PacksArtifactName, new FakeProbe());
        Assert.Contains("Set name       : Workload packs", _output.AllOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Workload manifests", _output.AllOutput, StringComparison.Ordinal);
    }

    /// <summary>
    /// Without scoping this is the failure: the packs stage has no manifests directory, so
    /// every manifest reads as a missing pending file.
    /// </summary>
    [Fact]
    public async Task Unscoped_prune_in_a_packs_only_stage_fails_closed()
    {
        await StageWorkloadAsync();
        KeepOnly(StagePlanner.PacksArtifactName);

        await Assert.ThrowsAsync<DotNetReleaseException>(
            () => Filter(set: null, new FakeProbe()));
    }

    [Fact]
    public async Task Packs_stage_verifies_only_its_own_set()
    {
        await StageWorkloadAsync();

        // Packs are live; manifests have not been pushed yet, which is correct at this point.
        var probe = new FakeProbe($"{Pack.ToLowerInvariant()}/10.0.0");

        await Verify(StagePlanner.PacksArtifactName, probe);
    }

    /// <summary>
    /// Without scoping the packs stage waits for manifests that, by design, cannot exist
    /// yet — so it would always burn the full deadline and then fail.
    /// </summary>
    [Fact]
    public async Task Unscoped_verify_in_a_packs_only_stage_waits_for_unpublished_manifests()
    {
        await StageWorkloadAsync();

        var probe = new FakeProbe($"{Pack.ToLowerInvariant()}/10.0.0");

        var exception = await Assert.ThrowsAsync<DotNetReleaseException>(
            () => Verify(set: null, probe, maxMinutes: 1));
        Assert.Contains(Manifest, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Manifests_stage_verifies_only_its_own_set()
    {
        await StageWorkloadAsync();

        var probe = new FakeProbe($"{Manifest.ToLowerInvariant()}/10.0.0");

        await Verify(StagePlanner.ManifestsArtifactName, probe);
    }

    /// <summary>
    /// A typo in the template must not silently select nothing and report success on a
    /// publish stage that verified zero packages.
    /// </summary>
    [Fact]
    public async Task An_unknown_set_name_fails_closed()
    {
        await StageWorkloadAsync();

        var exception = await Assert.ThrowsAsync<DotNetReleaseException>(
            () => Verify("MauiPacksForNuGet", new FakeProbe()));
        Assert.Contains(StagePlanner.PacksArtifactName, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Filter_with_an_unknown_set_name_fails_closed()
    {
        await StageWorkloadAsync();

        await Assert.ThrowsAsync<DotNetReleaseException>(
            () => Filter("NotASet", new FakeProbe()));
    }

    /// <summary>
    /// A transient feed error must not fail the release. `verify` is the authoritative
    /// success signal, and its recovery path is a re-run, which is the path that risks a
    /// fatal 409 - so it must be the most tolerant component, not the least.
    /// </summary>
    [Fact]
    public async Task Verify_keeps_polling_through_a_transient_feed_failure()
    {
        await StageWorkloadAsync();

        var probe = new FlakyProbe(failUntilCall: 3, published: [$"{Pack.ToLowerInvariant()}/10.0.0"]);

        await Verify(StagePlanner.PacksArtifactName, probe);
        Assert.True(probe.Calls >= 3);
    }

    [Fact]
    public async Task Packs_are_ordered_before_manifests_in_the_plan()
    {
        var plan = await StageWorkloadAsync();

        Assert.Equal(StagePlanner.PacksArtifactName, plan.Sets.OrderBy(s => s.Order).First().ArtifactName);
    }
}
