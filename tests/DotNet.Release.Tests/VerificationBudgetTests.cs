using Xunit;

namespace DotNet.Release.Tests;

/// <summary>Verifies the whole-set polling budget against production indexing latency.</summary>
public class VerificationBudgetTests : IDisposable
{
    private const int DefaultDeadlineMinutes = 30;
    private const int DefaultPollSeconds = 20;

    private readonly Workspace _workspace = new();
    private readonly RecordingConsole _console = new();

    private string PlanHash => ReleasePlanSerializer.ComputeHash(_workspace.ReadPlan());

    public void Dispose() => _workspace.Dispose();

    private async Task StageManyAsync(int count)
    {
        for (var i = 0; i < count; i++)
        {
            _workspace.WritePackage($"Package{i:D2}", "1.0.0");
        }

        var registry = new FakeRegistry(
            Workspace.Build(channels: new ChannelReference(".NET Libraries", 1648)));

        await PlanCommand.ExecuteAsync(
            _console,
            registry,
            Workspace.PolicyJson,
            "dotnet/skiasharp",
            Workspace.Commit,
            null,
            _workspace.Out,
            Workspace.Now,
            "1.0.0-test",
            CancellationToken.None);

        Assert.Equal(ExitCodes.Success, await StageCommand.ExecuteAsync(
            _console,
            Workspace.PolicyJson,
            File.ReadAllText(Path.Combine(_workspace.Out, PlanCommand.FileName)),
            _workspace.Drop,
            _workspace.Out,
            new StageOptions(),
            Workspace.Now,
            "1.0.0-test",
            CancellationToken.None));
    }

    private Task<int> Verify(
        IPackageAvailabilityProbe probe,
        int deadlineMinutes,
        int pollSeconds = DefaultPollSeconds)
    {
        var now = Workspace.Now;

        return VerifyCommand.ExecuteAsync(
            _console,
            probe,
            _workspace.ReadPlan(),
            TimeSpan.FromMinutes(deadlineMinutes),
            TimeSpan.FromSeconds(pollSeconds),
            () => now,
            (delay, _) =>
            {
                now = now.Add(delay);
                return Task.CompletedTask;
            },
            null,
            _workspace.Out,
            PlanHash,
            CancellationToken.None);
    }

    [Fact]
    public async Task The_production_latency_sample_succeeds_on_the_default_budget()
    {
        await StageManyAsync(41);

        var probe = new FakeProbe { AllAvailableAfterCall = 29 };

        Assert.Equal(ExitCodes.Success, await Verify(probe, DefaultDeadlineMinutes));
        Assert.Equal(29, probe.Calls);
    }

    [Fact]
    public async Task The_default_budget_allows_at_least_ninety_polls()
    {
        await StageManyAsync(2);

        var probe = new FakeProbe { AllAvailableAfterCall = 89 };

        Assert.Equal(ExitCodes.Success, await Verify(probe, DefaultDeadlineMinutes));
        Assert.Equal(89, probe.Calls);
    }

    [Fact]
    public async Task A_five_minute_budget_rejects_the_production_latency_sample()
    {
        await StageManyAsync(41);

        var exit = await Verify(
            new FakeProbe { AllAvailableAfterCall = 29 },
            deadlineMinutes: 5);

        Assert.Equal(ExitCodes.ReleaseError, exit);
    }

    [Fact]
    public async Task A_single_straggler_uses_the_set_budget()
    {
        await StageManyAsync(41);

        var straggler = ReleasePlanSerializer
            .DeserializePlan(_workspace.ReadPlan())
            .Value
            .AllPackages
            .Last();

        var probe = new FakeProbe
        {
            AllAvailableAfterCall = 18,
            AvailableAfterCall = { [straggler.IdentityKey] = 29 },
        };

        Assert.Equal(ExitCodes.Success, await Verify(probe, DefaultDeadlineMinutes));
        Assert.Equal(29, probe.Calls);
    }

    [Fact]
    public async Task Verification_failure_returns_missing_identities()
    {
        await StageManyAsync(3);

        var exit = await Verify(new FakeProbe(), deadlineMinutes: 1);

        Assert.Equal(ExitCodes.ReleaseError, exit);
        Assert.Contains("not available from NuGet.org", _console.AllErrors, StringComparison.Ordinal);
        Assert.Contains("Package00 1.0.0", _console.AllErrors, StringComparison.Ordinal);
    }
}

internal sealed class FlakyProbe(
    int failUntilCall,
    string[] published) : IPackageAvailabilityProbe
{
    private readonly HashSet<string> _published =
        new(published, StringComparer.OrdinalIgnoreCase);

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
            package => package.IdentityKey,
            package => _published.Contains(package.IdentityKey),
            StringComparer.Ordinal);

        return Task.FromResult(result);
    }
}
