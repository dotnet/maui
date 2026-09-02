using Xunit;

namespace DotNet.Release.Tests;

/// <summary>Verifies the whole-set polling budget against production indexing latency.</summary>
public class VerificationBudgetTests : IDisposable
{
    private const int DefaultDeadlineMinutes = 30;
    private const int DefaultPollSeconds = 20;

    private readonly Workspace _workspace = new();
    private readonly RecordingWriter _output = new();

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
            _output,
            registry,
            Workspace.PolicyJson,
            "dotnet/skiasharp",
            Workspace.Commit,
            null,
            _workspace.Out,
            Workspace.Now,
            "1.0.0-test",
            CancellationToken.None);

        await StageCommand.ExecuteAsync(
            _output,
            Workspace.PolicyJson,
            File.ReadAllText(Path.Combine(_workspace.Out, PlanCommand.FileName)),
            _workspace.Drop,
            _workspace.Out,
            new StageOptions(),
            Workspace.Now,
            "1.0.0-test",
            CancellationToken.None);
    }

    private Task Verify(
        INuGetPackageLookup probe,
        int deadlineMinutes,
        int pollSeconds = DefaultPollSeconds)
    {
        var now = Workspace.Now;

        return VerifyCommand.ExecuteAsync(
            _output,
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
            PlanHash,
            CancellationToken.None);
    }

    [Fact]
    public async Task The_production_latency_sample_succeeds_on_the_default_budget()
    {
        await StageManyAsync(41);

        var probe = new FakeProbe { AllAvailableAfterCall = 29 };

        await Verify(probe, DefaultDeadlineMinutes);
        Assert.Equal(29, probe.Calls);
    }

    [Fact]
    public async Task The_default_budget_allows_at_least_ninety_polls()
    {
        await StageManyAsync(2);

        var probe = new FakeProbe { AllAvailableAfterCall = 89 };

        await Verify(probe, DefaultDeadlineMinutes);
        Assert.Equal(89, probe.Calls);
    }

    [Fact]
    public async Task A_five_minute_budget_rejects_the_production_latency_sample()
    {
        await StageManyAsync(41);

        await Assert.ThrowsAsync<DotNetReleaseException>(() => Verify(
            new FakeProbe { AllAvailableAfterCall = 29 },
            deadlineMinutes: 5));
    }

    [Fact]
    public async Task A_single_straggler_uses_the_set_budget()
    {
        await StageManyAsync(41);

        var straggler = ReleasePlanSerializer
            .DeserializePlan(_workspace.ReadPlan())
            .AllPackages
            .Last();

        var probe = new FakeProbe
        {
            AllAvailableAfterCall = 18,
            AvailableAfterCall = { [straggler.IdentityKey] = 29 },
        };

        await Verify(probe, DefaultDeadlineMinutes);
        Assert.Equal(29, probe.Calls);
    }

    [Fact]
    public async Task Verification_failure_returns_missing_identities()
    {
        await StageManyAsync(3);

        var exception = await Assert.ThrowsAsync<DotNetReleaseException>(
            () => Verify(new FakeProbe(), deadlineMinutes: 1));
        Assert.Contains("not available from NuGet.org", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Package00 1.0.0", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Verification_treats_false_and_unknown_identities_as_missing()
    {
        var skia = TestData.Planned("SkiaSharp", "3.119.0");
        var harfBuzz = TestData.Planned("HarfBuzzSharp", "8.3.1");

        var missing = VerifyCommand.GetMissing(
            [skia, harfBuzz],
            TestData.Availability((skia, false)));

        Assert.Equal(["SkiaSharp", "HarfBuzzSharp"], missing.Select(package => package.Id));
    }

    [Fact]
    public void Verification_failure_message_has_a_stable_identity_order()
    {
        var skia = TestData.Planned("SkiaSharp", "3.119.0");
        var harfBuzz = TestData.Planned("HarfBuzzSharp", "8.3.1");

        Assert.Equal(
            "The following packages are not available from NuGet.org: " +
            "HarfBuzzSharp 8.3.1, SkiaSharp 3.119.0",
            VerifyCommand.DescribeMissing([skia, harfBuzz]));
    }
}

internal sealed class FlakyProbe(
    int failUntilCall,
    string[] published) : INuGetPackageLookup
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
