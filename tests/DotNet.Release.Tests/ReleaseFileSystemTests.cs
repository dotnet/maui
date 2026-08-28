using Xunit;

namespace DotNet.Release.Tests;

/// <summary>
/// Proves the design's organising constraint — <i>the tool mutates nothing outside its own
/// output directory</i> — instead of asserting it in prose.
/// </summary>
/// <remarks>
/// Before the filesystem seam existed these tests could not be written. Verbs called
/// <c>File.*</c> directly, so there was no way to observe what a run did or, more
/// importantly, to prove what it did not do.
/// </remarks>
public class ReleaseFileSystemTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("release-fs-tests-").FullName;

    public void Dispose()
    {
        try
        {
            Directory.Delete(_root, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    private string Inside => Path.Combine(_root, "stage");

    /// <summary>A stable sibling of the root, not a fresh path per access.</summary>
    private readonly string _outside = Path.Combine(Path.GetTempPath(), $"escaped-{Guid.NewGuid():N}.txt");

    private PhysicalReleaseFileSystem Rooted => new(_root);

    // ---- containment ----

    [Fact]
    public async Task Writes_inside_the_root_are_allowed()
    {
        var fs = Rooted;
        fs.CreateDirectory(Inside);

        await fs.WriteAllTextAsync(Path.Combine(Inside, "plan.json"), "{}", CancellationToken.None);

        Assert.True(File.Exists(Path.Combine(Inside, "plan.json")));
    }

    [Fact]
    public async Task A_write_outside_the_root_is_refused()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => Rooted.WriteAllTextAsync(_outside, "should not exist", CancellationToken.None));

        Assert.False(File.Exists(_outside));
    }

    /// <summary>
    /// The case the containment exists for. `filter` resolves deletion targets from the plan,
    /// and a plan is data: a crafted `fileName` or `artifactName` containing `..` would
    /// otherwise reach outside the staging tree. Containment holds independently of the hash
    /// chain, of validation order, and of the caller.
    /// </summary>
    [Theory]
    [InlineData("../escaped.nupkg")]
    [InlineData("stage/../../escaped.nupkg")]
    [InlineData("stage/../../../etc/passwd")]
    public void A_traversal_out_of_the_root_is_refused(string relative)
    {
        var target = Path.Combine(_root, relative);

        Assert.Throws<UnauthorizedAccessException>(() => Rooted.DeleteFile(target));
    }

    [Fact]
    public void A_delete_outside_the_root_is_refused_even_when_the_file_exists()
    {
        File.WriteAllText(_outside, "innocent bystander");

        try
        {
            Assert.Throws<UnauthorizedAccessException>(() => Rooted.DeleteFile(_outside));
            Assert.True(File.Exists(_outside), "The file must survive a refused delete.");
        }
        finally
        {
            File.Delete(_outside);
        }
    }

    [Fact]
    public void A_sibling_directory_with_the_same_prefix_is_not_inside_the_root()
    {
        // '/tmp/root-evil' must not count as inside '/tmp/root'.
        var sibling = _root + "-evil";

        Assert.Throws<UnauthorizedAccessException>(() => Rooted.CreateDirectory(sibling));
    }

    [Fact]
    public async Task Reads_are_deliberately_unrooted()
    {
        // The tool legitimately reads the policy file, the gathered drop and the published
        // tool, all outside its output directory. Only effects are confined.
        var external = Path.Combine(Path.GetTempPath(), $"read-{Guid.NewGuid():N}.txt");
        await File.WriteAllTextAsync(external, "readable", CancellationToken.None);

        try
        {
            Assert.Equal("readable", await Rooted.ReadAllTextAsync(external, CancellationToken.None));
        }
        finally
        {
            File.Delete(external);
        }
    }
}

/// <summary>
/// Proves what each verb does to the filesystem, and what it leaves alone.
/// </summary>
public class VerbEffectTests : IDisposable
{
    private readonly Workspace _workspace = new();
    private readonly RecordingConsole _console = new();

    public void Dispose() => _workspace.Dispose();

    private RecordingReleaseFileSystem Recorder(string root) =>
        new(new PhysicalReleaseFileSystem(root));

    private static readonly ChannelReference Libraries = new(".NET Libraries", 1648);

    private async Task<RecordingReleaseFileSystem> PlanAsync()
    {
        var fs = Recorder(_workspace.Out);

        Assert.Equal(ExitCodes.Success, await Verbs.PlanAsync(
            _console, fs, new FakeRegistry(Workspace.Build(channels: Libraries)),
            Workspace.PolicyJson, "dotnet/skiasharp", Workspace.Commit, null, false,
            _workspace.Out, Workspace.Now, "1.0.0-test", CancellationToken.None));

        return fs;
    }

    private async Task<RecordingReleaseFileSystem> StageAsync()
    {
        _workspace.WritePackage("SkiaSharp", "3.119.0");
        await PlanAsync();

        var fs = Recorder(_workspace.Out);

        Assert.Equal(ExitCodes.Success, await Verbs.StageAsync(
            _console, fs, new NupkgIdentityReader(), Workspace.PolicyJson,
            await File.ReadAllTextAsync(Path.Combine(_workspace.Out, Verbs.PlanFileName), CancellationToken.None),
            _workspace.Drop, _workspace.Out, new StageOptions(),
            _workspace.Tool, _workspace.ToolFilePath, Workspace.Now, "1.0.0-test", CancellationToken.None));

        return fs;
    }

    /// <summary>A dry run invokes only `plan` and `stage`. Neither may delete anything.</summary>
    [Fact]
    public async Task Plan_deletes_nothing()
    {
        Assert.Empty((await PlanAsync()).Deleted);
    }

    [Fact]
    public async Task Plan_writes_exactly_one_file()
    {
        var fs = await PlanAsync();

        Assert.Equal(Path.Combine(_workspace.Out, Verbs.PlanFileName), Assert.Single(fs.Written));
        Assert.Empty(fs.Copied);
    }

    [Fact]
    public async Task Stage_deletes_nothing()
    {
        Assert.Empty((await StageAsync()).Deleted);
    }

    /// <summary>
    /// The claim the whole dry-run safety case rests on, now mechanically checked for the two
    /// verbs a dry run actually runs.
    /// </summary>
    [Fact]
    public async Task A_dry_run_mutates_nothing_outside_its_output_directory()
    {
        var plan = await PlanAsync();
        var stage = await StageAsync();

        foreach (var path in plan.Mutations.Concat(stage.Mutations))
        {
            Assert.StartsWith(_workspace.Out, Path.GetFullPath(path), StringComparison.Ordinal);
        }

        Assert.Empty(plan.Deleted);
        Assert.Empty(stage.Deleted);
    }

    [Fact]
    public async Task Stage_writes_the_plan_the_marker_and_the_tool_into_each_set()
    {
        var fs = await StageAsync();

        var written = fs.Written.Select(Path.GetFileName).ToList();
        Assert.Contains(Verbs.ReleasePlanFileName, written);
        Assert.Contains(ReleaseSetMarker.FileName, written);

        // The publish job runs `checkout: none`, so the tool must travel in the artifact.
        Assert.Contains(fs.Copied, c => c.Contains(Verbs.ToolDirectoryName, StringComparison.Ordinal));
    }

    /// <summary>
    /// `filter` is the only verb that deletes, and it may delete only the packages the plan
    /// marked as not needing publication.
    /// </summary>
    [Fact]
    public async Task Filter_deletes_only_the_already_published_packages()
    {
        _workspace.WritePackage("SkiaSharp", "3.119.0");
        _workspace.WritePackage("HarfBuzzSharp", "8.3.1.5");
        await PlanAsync();

        Assert.Equal(ExitCodes.Success, await Verbs.StageAsync(
            _console, new PhysicalReleaseFileSystem(_workspace.Out), new NupkgIdentityReader(),
            Workspace.PolicyJson,
            await File.ReadAllTextAsync(Path.Combine(_workspace.Out, Verbs.PlanFileName), CancellationToken.None),
            _workspace.Drop, _workspace.Out, new StageOptions(),
            _workspace.Tool, _workspace.ToolFilePath, Workspace.Now, "1.0.0-test", CancellationToken.None));

        var fs = Recorder(_workspace.Out);

        Assert.Equal(ExitCodes.Success, await Verbs.FilterAsync(
            _console, fs, new FakeProbe("skiasharp/3.119.0"), _workspace.ReadPlan(),
            _workspace.Out, [], ReleasePlanSerializer.ComputeHash(_workspace.ReadPlan()), StagePlanner.PackagesArtifactName, CancellationToken.None));

        Assert.Equal("SkiaSharp.3.119.0.nupkg", Path.GetFileName(Assert.Single(fs.Deleted)));
    }
}
