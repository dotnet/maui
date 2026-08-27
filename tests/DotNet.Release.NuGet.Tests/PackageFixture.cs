using System.IO.Compression;
using System.Text;

namespace DotNet.Release.NuGet.Tests;

/// <summary>
/// Builds real <c>.nupkg</c> files on disk.
/// </summary>
/// <remarks>
/// The reader's whole job is to interpret a real package archive, so faking the archive
/// would test nothing. These are genuine zips with a genuine nuspec; no network is involved.
/// </remarks>
internal sealed class PackageFixture : IDisposable
{
    public PackageFixture()
    {
        Root = Directory.CreateTempSubdirectory("release-tool-tests-").FullName;
    }

    public string Root { get; }

    /// <summary>Writes a well-formed package and returns its path.</summary>
    public string WritePackage(
        string id = "SkiaSharp",
        string version = "3.119.0",
        string? fileName = null,
        string? nuspecOverride = null,
        string nuspecEntryName = "SkiaSharp.nuspec")
    {
        var nuspec = nuspecOverride ?? $"""
            <?xml version="1.0" encoding="utf-8"?>
            <package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
              <metadata>
                <id>{id}</id>
                <version>{version}</version>
                <authors>Test</authors>
                <description>Test package</description>
              </metadata>
            </package>
            """;

        var path = Path.Combine(Root, fileName ?? $"{id}.{version}.nupkg");

        using var file = File.Create(path);
        using var archive = new ZipArchive(file, ZipArchiveMode.Create);

        WriteEntry(archive, nuspecEntryName, nuspec);
        WriteEntry(archive, "lib/net10.0/placeholder.txt", "placeholder");

        return path;
    }

    /// <summary>Writes a file that is not a valid zip at all.</summary>
    public string WriteCorruptPackage(string fileName = "Corrupt.1.0.0.nupkg")
    {
        var path = Path.Combine(Root, fileName);
        File.WriteAllText(path, "this is not a zip archive");
        return path;
    }

    private static void WriteEntry(ZipArchive archive, string name, string content)
    {
        using var stream = archive.CreateEntry(name).Open();
        stream.Write(Encoding.UTF8.GetBytes(content));
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(Root, recursive: true);
        }
        catch (IOException)
        {
            // A leftover temp directory must never fail a test run.
        }
    }
}

/// <summary>A feed whose answers are decided by the test, so no network is involved.</summary>
internal sealed class FakeExistenceChecker(params string[] published) : IPackageExistenceChecker
{
    private readonly HashSet<string> _published =
        new(published.Select(p => p.ToLowerInvariant()), StringComparer.Ordinal);

    private int _calls;

    public int Calls => _calls;

    public List<string> Queried { get; } = [];

    public Func<string, string, bool>? Throw { get; init; }

    public Task<bool> ExistsAsync(string id, string normalizedVersion, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _calls);

        lock (Queried)
        {
            Queried.Add($"{id}/{normalizedVersion}");
        }

        if (Throw?.Invoke(id, normalizedVersion) == true)
        {
            throw new HttpRequestException("feed unavailable");
        }

        return Task.FromResult(_published.Contains($"{id}/{normalizedVersion}".ToLowerInvariant()));
    }
}
