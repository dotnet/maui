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
    private readonly RecordingConsole _console = new();

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

        Assert.Equal(ExitCodes.Success, await Verbs.PlanAsync(
            _console, registry, Workspace.PolicyJson, "dotnet/maui", Workspace.Commit, null,
            _workspace.Out, Workspace.Now, "1.0.0-test", CancellationToken.None));

        Assert.Equal(ExitCodes.Success, await Verbs.StageAsync(
            _console, Workspace.PolicyJson,
            File.ReadAllText(Path.Combine(_workspace.Out, Verbs.PlanFileName)),
            _workspace.Drop, _workspace.Out, new StageOptions(),
            Workspace.Now, "1.0.0-test", CancellationToken.None));

        var plan = ReleasePlanSerializer.DeserializePlan(_workspace.ReadPlan()).Value;
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

    private Task<int> Filter(string? set, IPackageAvailabilityProbe probe) =>
        Verbs.FilterAsync(_console, probe, _workspace.ReadPlan(), _workspace.Out, [], PlanHash, set, CancellationToken.None);

    private Task<int> Verify(string? set, IPackageAvailabilityProbe probe, int maxMinutes = 30)
    {
        var now = Workspace.Now;

        return Verbs.VerifyAsync(
            _console, probe, _workspace.ReadPlan(),
            TimeSpan.FromMinutes(maxMinutes), TimeSpan.FromSeconds(20),
            () => now,
            (delay, _) => { now = now.Add(delay); return Task.CompletedTask; },
            set,
            _workspace.Out,
            PlanHash,
            CancellationToken.None);
    }

    [Fact]
    public async Task Packs_stage_filters_only_its_own_set()
    {
        await StageWorkloadAsync();
        KeepOnly(StagePlanner.PacksArtifactName);

        // Staging already logged both set names, so only the filter's own output is examined.
        _console.Output.Clear();

        var exit = await Filter(StagePlanner.PacksArtifactName, new FakeProbe());

        Assert.Equal(ExitCodes.Success, exit);
        Assert.Contains("Workload packs: 1 of 1", _console.AllOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Workload manifests", _console.AllOutput, StringComparison.Ordinal);
    }

    /// <summary>
    /// Without scoping this is the failure: the packs stage has no manifests directory, so
    /// every manifest reads as a missing pending file.
    /// </summary>
    [Fact]
    public async Task Unscoped_filter_in_a_packs_only_stage_fails_closed()
    {
        await StageWorkloadAsync();
        KeepOnly(StagePlanner.PacksArtifactName);

        var exit = await Filter(set: null, new FakeProbe());

        Assert.Equal(ExitCodes.ReleaseError, exit);
        Assert.Contains(ErrorCodes.PackageFileMissing, _console.AllErrors, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Packs_stage_verifies_only_its_own_set()
    {
        await StageWorkloadAsync();

        // Packs are live; manifests have not been pushed yet, which is correct at this point.
        var probe = new FakeProbe($"{Pack.ToLowerInvariant()}/10.0.0");

        Assert.Equal(ExitCodes.Success, await Verify(StagePlanner.PacksArtifactName, probe));
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

        var exit = await Verify(set: null, probe, maxMinutes: 1);

        Assert.Equal(ExitCodes.ReleaseError, exit);
        Assert.Contains(Manifest, _console.AllErrors, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Manifests_stage_verifies_only_its_own_set()
    {
        await StageWorkloadAsync();

        var probe = new FakeProbe($"{Manifest.ToLowerInvariant()}/10.0.0");

        Assert.Equal(ExitCodes.Success, await Verify(StagePlanner.ManifestsArtifactName, probe));
    }

    /// <summary>
    /// A typo in the template must not silently select nothing and report success on a
    /// publish stage that verified zero packages.
    /// </summary>
    [Fact]
    public async Task An_unknown_set_name_fails_closed()
    {
        await StageWorkloadAsync();

        var exit = await Verify("MauiPacksForNuGet", new FakeProbe());

        Assert.Equal(ExitCodes.ReleaseError, exit);
        Assert.Contains(ErrorCodes.PackageSetNotFound, _console.AllErrors, StringComparison.Ordinal);
        Assert.Contains(StagePlanner.PacksArtifactName, _console.AllErrors, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Filter_with_an_unknown_set_name_fails_closed()
    {
        await StageWorkloadAsync();

        Assert.Equal(ExitCodes.ReleaseError, await Filter("NotASet", new FakeProbe()));
        Assert.Contains(ErrorCodes.PackageSetNotFound, _console.AllErrors, StringComparison.Ordinal);
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

        Assert.Equal(ExitCodes.Success, await Verify(StagePlanner.PacksArtifactName, probe));
        Assert.True(probe.Calls >= 3);
    }

    [Fact]
    public async Task Packs_are_ordered_before_manifests_in_the_plan()
    {
        var plan = await StageWorkloadAsync();

        Assert.Equal(StagePlanner.PacksArtifactName, plan.Sets.OrderBy(s => s.Order).First().ArtifactName);
    }
}

/// <summary>
/// Reconciles the verification budget against observed production behaviour.
/// </summary>
/// <remarks>
/// Azure DevOps build 3059242 released 41 packages. NuGet.org indexing was neither fast nor
/// uniform: the set completed after 10m30s and 29 polls, with a single package holding the
/// tail alone for 4m11s across 11 polls. These tests exist so a future tuning change cannot
/// quietly make a healthy release fail.
/// </remarks>
public class VerificationBudgetTests : IDisposable
{
    private readonly Workspace _workspace = new();
    private readonly RecordingConsole _console = new();

    /// <summary>The pin the preparing stage computes. Production always supplies it.</summary>
    private string PlanHash => ReleasePlanSerializer.ComputeHash(_workspace.ReadPlan());

    public void Dispose() => _workspace.Dispose();

    /// <summary>The template's defaults.</summary>
    private const int DefaultDeadlineMinutes = 30;

    private const int DefaultPollSeconds = 20;

    private async Task StageManyAsync(int count)
    {
        for (var i = 0; i < count; i++)
        {
            _workspace.WritePackage($"Package{i:D2}", "1.0.0");
        }

        var registry = new FakeRegistry(Workspace.Build(channels: new ChannelReference(".NET Libraries", 1648)));

        await Verbs.PlanAsync(
            _console, registry, Workspace.PolicyJson, "dotnet/skiasharp", Workspace.Commit, null,
            _workspace.Out, Workspace.Now, "1.0.0-test", CancellationToken.None);

        Assert.Equal(ExitCodes.Success, await Verbs.StageAsync(
            _console, Workspace.PolicyJson,
            File.ReadAllText(Path.Combine(_workspace.Out, Verbs.PlanFileName)),
            _workspace.Drop, _workspace.Out, new StageOptions(),
            Workspace.Now, "1.0.0-test", CancellationToken.None));
    }

    private Task<int> Verify(IPackageAvailabilityProbe probe, int deadlineMinutes, int pollSeconds = DefaultPollSeconds)
    {
        var now = Workspace.Now;

        return Verbs.VerifyAsync(
            _console, probe, _workspace.ReadPlan(),
            TimeSpan.FromMinutes(deadlineMinutes), TimeSpan.FromSeconds(pollSeconds),
            () => now,
            (delay, _) => { now = now.Add(delay); return Task.CompletedTask; },
            null,
            _workspace.Out,
            PlanHash,
            CancellationToken.None);
    }

    /// <summary>The observed production run must pass comfortably on the default budget.</summary>
    [Fact]
    public async Task The_observed_production_run_succeeds_on_the_default_budget()
    {
        await StageManyAsync(41);

        var probe = new FakeProbe { AllAvailableAfterCall = 29 };

        Assert.Equal(ExitCodes.Success, await Verify(probe, DefaultDeadlineMinutes));
        Assert.Equal(29, probe.Calls);
    }

    /// <summary>
    /// The margin is real, not incidental: the default budget allows roughly three times the
    /// polls the observed run needed.
    /// </summary>
    [Fact]
    public async Task The_default_budget_allows_at_least_ninety_polls()
    {
        await StageManyAsync(2);

        var probe = new FakeProbe { AllAvailableAfterCall = 89 };

        Assert.Equal(ExitCodes.Success, await Verify(probe, DefaultDeadlineMinutes));
        Assert.Equal(89, probe.Calls);
    }

    /// <summary>
    /// Demonstrates why the budget is not tuned down. A five-minute ceiling would have
    /// failed the healthy production run above.
    /// </summary>
    [Fact]
    public async Task A_five_minute_budget_would_have_failed_a_healthy_release()
    {
        await StageManyAsync(41);

        var exit = await Verify(new FakeProbe { AllAvailableAfterCall = 29 }, deadlineMinutes: 5);

        Assert.Equal(ExitCodes.ReleaseError, exit);
    }

    /// <summary>
    /// The budget covers the set, not each package: one straggler must be able to consume a
    /// large share of the window without its own clock.
    /// </summary>
    [Fact]
    public async Task A_single_straggler_is_covered_by_the_set_budget()
    {
        await StageManyAsync(41);

        var plan = ReleasePlanSerializer.DeserializePlan(_workspace.ReadPlan()).Value;
        var straggler = plan.AllPackages.Last();

        var probe = new FakeProbe
        {
            AllAvailableAfterCall = 18,
            AvailableAfterCall = { [straggler.IdentityKey] = 29 },
        };

        Assert.Equal(ExitCodes.Success, await Verify(probe, DefaultDeadlineMinutes));
        Assert.Equal(29, probe.Calls);
    }

    /// <summary>Verification fails with a non-zero exit code, never a warning.</summary>
    /// <remarks>
    /// The surrounding job result is not a reliable oracle: a production release job on a
    /// non-production branch reports partiallySucceeded from 1ES branch-validation checks
    /// even when the release itself is correct. The verify step's own exit code is the
    /// authoritative signal, so it must be unambiguous.
    /// </remarks>
    [Fact]
    public async Task Verification_failure_is_a_non_zero_exit_code_with_the_missing_identities()
    {
        await StageManyAsync(3);

        var exit = await Verify(new FakeProbe(), deadlineMinutes: 1);

        Assert.NotEqual(ExitCodes.Success, exit);
        Assert.Equal(ExitCodes.ReleaseError, exit);
        Assert.Contains("not available from NuGet.org", _console.AllErrors, StringComparison.Ordinal);
        Assert.Contains("Package00 1.0.0", _console.AllErrors, StringComparison.Ordinal);
    }
}

/// <summary>A probe that throws for the first few calls, then answers normally.</summary>
internal sealed class FlakyProbe(int failUntilCall, string[] published) : IPackageAvailabilityProbe
{
    private readonly HashSet<string> _published = new(published, StringComparer.OrdinalIgnoreCase);

    public int Calls { get; private set; }

    public Task<IReadOnlyDictionary<string, bool>> GetAvailabilityAsync(
        IReadOnlyList<PlannedPackage> packages,
        CancellationToken cancellationToken)
    {
        Calls++;

        if (Calls < failUntilCall)
        {
            throw new HttpRequestException("transient feed failure");
        }

        IReadOnlyDictionary<string, bool> result = packages.ToDictionary(
            p => p.IdentityKey, p => _published.Contains(p.IdentityKey), StringComparer.Ordinal);

        return Task.FromResult(result);
    }
}
