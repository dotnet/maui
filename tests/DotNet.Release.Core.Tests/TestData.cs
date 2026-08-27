using System.Security.Cryptography;
using System.Text;

namespace DotNet.Release.Core.Tests;

internal static class TestData
{
    public const string Commit = "f14581760a1c9e4b0f1e2d3c4b5a6978899aabbc";
    public const string ToolVersion = "1.0.0-test";

    public static readonly DateTimeOffset Now = new(2026, 8, 27, 18, 0, 0, TimeSpan.Zero);

    public const string PolicyJson = """
    {
      "schemaVersion": 1,
      "repositories": {
        "dotnet/maui": { "workload": true },
        "dotnet/android-libraries": { "workload": false, "channel": { "name": ".NET 10", "id": 5172 } },
        "dotnet/skiasharp": { "workload": false, "channel": { "name": ".NET Libraries", "id": 1648 } }
      },
      "workloadSets": {
        "10": { "channel": ".NET 10 Workload Release", "feed": "dotnet10-workloads" }
      }
    }
    """;

    public static RepositoryId Repo(string fullName) => RepositoryId.Parse(fullName).Value;

    public static ReleasePolicy Policy(string? json = null) => ReleasePolicy.Parse(json ?? PolicyJson).Value;

    public static RepositoryPolicy RepoPolicy(string fullName) =>
        Policy().GetRepository(Repo(fullName)).Value;

    public static ToolReference Tool => new("release", Hash("tool"));

    /// <summary>A stable fake content hash, so expectations stay readable.</summary>
    public static string Hash(string seed) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(seed)));

    public static DropPackage Drop(string id, string version, string? fileName = null) =>
        new(
            fileName ?? $"{id}.{version}.nupkg",
            id,
            version,
            PackageVersions.Normalize(version).IsSuccess ? PackageVersions.Normalize(version).Value : version,
            Hash($"{id}/{version}"));

    public static BarBuild Build(
        int id = 4242,
        string? commit = null,
        string? gitHubRepository = "https://github.com/dotnet/skiasharp",
        string? azureDevOpsRepository = null,
        params ChannelReference[] channels) =>
        new(id, commit ?? Commit, gitHubRepository, azureDevOpsRepository, channels);

    public static ReleaseRequest Request(string repo = "dotnet/skiasharp", string? commit = null, int? barId = null) =>
        new(Repo(repo), commit ?? Commit, barId);

    public static ResolvedRelease Resolved(bool workload = false, string repo = "dotnet/skiasharp") => new()
    {
        ToolVersion = ToolVersion,
        CreatedUtc = Now,
        Repository = repo,
        RepositoryUrl = $"https://github.com/{repo}",
        Commit = Commit,
        BarBuildId = 4242,
        RepositoryOrigin = RepositoryOrigin.GitHubRepository,
        Workload = workload,
        Channel = workload ? null : new ChannelReference(".NET Libraries", 1648),
    };

    public static ReleasePackageSet Set(params PlannedPackage[] packages) => new()
    {
        Name = "NuGet packages",
        Order = 0,
        ArtifactName = StagePlanner.PackagesArtifactName,
        Packages = packages,
    };

    public static PlannedPackage Planned(string id, string version) => new()
    {
        Id = id,
        Version = version,
        NormalizedVersion = PackageVersions.Normalize(version).Value,
        FileName = $"{id}.{version}.nupkg",
        Sha256 = Hash($"{id}/{version}"),
    };

    /// <summary>Availability map keyed the way <see cref="PlannedPackage.IdentityKey"/> is.</summary>
    public static Dictionary<string, bool> Availability(params (PlannedPackage Package, bool Published)[] entries) =>
        entries.ToDictionary(e => e.Package.IdentityKey, e => e.Published);
}
