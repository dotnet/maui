using System.IO.Compression;
using System.Text;
using DotNet.Release.Core;

namespace DotNet.Release.Cli.Tests;

internal sealed class RecordingConsole : IReleaseConsole
{
    public List<string> Output { get; } = [];

    public List<string> Errors { get; } = [];

    public string AllOutput => string.Join("\n", Output);

    public string AllErrors => string.Join("\n", Errors);

    public void WriteLine(string message) => Output.Add(message);

    public void WriteError(string message) => Errors.Add(message);
}

internal sealed class FakeRegistry(params BarBuild[] builds) : IBuildRegistry
{
    public int? RequestedBarId { get; private set; }

    public string? RequestedCommit { get; private set; }

    public Task<IReadOnlyList<BarBuild>> GetBuildAsync(int barBuildId, CancellationToken cancellationToken)
    {
        RequestedBarId = barBuildId;
        return Task.FromResult<IReadOnlyList<BarBuild>>(builds);
    }

    public Task<IReadOnlyList<BarBuild>> GetBuildsAsync(RepositoryId repository, string commit, CancellationToken cancellationToken)
    {
        RequestedCommit = commit;
        return Task.FromResult<IReadOnlyList<BarBuild>>(builds);
    }
}

internal sealed class FakeProbe(params string[] published) : IPackageAvailabilityProbe
{
    private readonly HashSet<string> _published = new(published, StringComparer.OrdinalIgnoreCase);

    public int Calls { get; private set; }

    /// <summary>Identities that become available after the given number of calls.</summary>
    public Dictionary<string, int> AvailableAfterCall { get; init; } = [];

    public Task<IReadOnlyDictionary<string, bool>> GetAvailabilityAsync(
        IReadOnlyList<PlannedPackage> packages,
        CancellationToken cancellationToken)
    {
        Calls++;

        IReadOnlyDictionary<string, bool> result = packages.ToDictionary(
            p => p.IdentityKey,
            p => _published.Contains(p.IdentityKey) ||
                 (AvailableAfterCall.TryGetValue(p.IdentityKey, out var call) && Calls >= call),
            StringComparer.Ordinal);

        return Task.FromResult(result);
    }
}

internal sealed class Workspace : IDisposable
{
    public const string Commit = "f14581760a1c9e4b0f1e2d3c4b5a6978899aabbc";

    public const string PolicyJson = """
    {
      "schemaVersion": 1,
      "repositories": {
        "dotnet/maui": { "workload": true },
        "dotnet/skiasharp": { "workload": false, "channel": { "name": ".NET Libraries", "id": 1648 } }
      },
      "workloadSets": {
        "10": { "channel": ".NET 10 Workload Release", "feed": "dotnet10-workloads" }
      }
    }
    """;

    public Workspace()
    {
        Root = Directory.CreateTempSubdirectory("release-cli-tests-").FullName;
        Directory.CreateDirectory(DropPackages);
    }

    public string Root { get; }

    public string Out => Path.Combine(Root, "stage");

    public string Drop => Path.Combine(Root, "drop");

    public string DropPackages => Path.Combine(Drop, "shipping", "packages");

    public ToolReference Tool { get; } = new("release", new string('c', 64));

    public static readonly DateTimeOffset Now = new(2026, 8, 27, 18, 0, 0, TimeSpan.Zero);

    public static BarBuild Build(
        string? gitHubRepository = "https://github.com/dotnet/skiasharp",
        params ChannelReference[] channels) =>
        new(4242, Commit, gitHubRepository, null, channels);

    /// <summary>Writes a real .nupkg into the simulated gather-drop output.</summary>
    public string WritePackage(string id, string version)
    {
        var path = Path.Combine(DropPackages, $"{id}.{version}.nupkg");

        using var file = File.Create(path);
        using var archive = new ZipArchive(file, ZipArchiveMode.Create);
        using var entry = archive.CreateEntry($"{id}.nuspec").Open();

        entry.Write(Encoding.UTF8.GetBytes($"""
            <?xml version="1.0" encoding="utf-8"?>
            <package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
              <metadata>
                <id>{id}</id>
                <version>{version}</version>
                <authors>Test</authors>
                <description>Test</description>
              </metadata>
            </package>
            """));

        return path;
    }

    public string ReadPlan() => File.ReadAllText(Path.Combine(Out, Verbs.ReleasePlanFileName));

    public string StagedSet(string artifactName) => Path.Combine(Out, artifactName);

    public void Dispose()
    {
        try
        {
            Directory.Delete(Root, recursive: true);
        }
        catch (IOException)
        {
        }
    }
}
