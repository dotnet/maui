using System.Security.Cryptography;
using Xunit;

namespace DotNet.Release.Tests;

public class NupkgIdentityReaderTests : IDisposable
{
    private readonly PackageFixture _fixture = new();
    private readonly NupkgIdentityReader _reader = new();

    public void Dispose() => _fixture.Dispose();

    [Fact]
    public async Task Reads_identity_from_the_nuspec()
    {
        var path = _fixture.WritePackage("SkiaSharp", "3.119.0");

        var result = await _reader.ReadAsync(path, CancellationToken.None);

        Assert.Equal("SkiaSharp", result.Id);
        Assert.Equal("3.119.0", result.Version);
        Assert.Equal("3.119.0", result.NormalizedVersion);
        Assert.Equal("SkiaSharp.3.119.0.nupkg", result.FileName);
    }

    /// <summary>
    /// Package identity is authoritative nuspec metadata; the filename is validated
    /// independently by staging policy.
    /// </summary>
    [Fact]
    public async Task Identity_comes_from_the_nuspec_not_the_file_name()
    {
        var path = _fixture.WritePackage("SkiaSharp", "3.119.0", fileName: "totally-different-name.nupkg");

        var result = await _reader.ReadAsync(path, CancellationToken.None);

        Assert.Equal("SkiaSharp", result.Id);
        Assert.Equal("3.119.0", result.Version);

        // The reader reports metadata as-is; StagePlanner applies filename policy.
        Assert.Equal("totally-different-name.nupkg", result.FileName);
    }

    [Theory]
    [InlineData("3.119.0.0", "3.119.0")]
    [InlineData("1.0", "1.0.0")]
    [InlineData("8.3.1.5", "8.3.1.5")]
    [InlineData("10.0.0-preview.1.25123.4", "10.0.0-preview.1.25123.4")]
    public async Task Normalized_version_is_computed_by_NuGet_not_by_string_slicing(string version, string expected)
    {
        var path = _fixture.WritePackage("SkiaSharp", version, fileName: $"SkiaSharp.{version}.nupkg");

        var result = await _reader.ReadAsync(path, CancellationToken.None);

        Assert.Equal(expected, result.NormalizedVersion);
    }

    [Fact]
    public async Task Content_hash_is_the_sha256_of_the_file()
    {
        var path = _fixture.WritePackage();

        var result = await _reader.ReadAsync(path, CancellationToken.None);

        var expected = Convert.ToHexStringLower(SHA256.HashData(await File.ReadAllBytesAsync(path, CancellationToken.None)));
        Assert.Equal(expected, result.Sha256);
    }

    [Fact]
    public async Task Two_packages_with_different_content_hash_differently()
    {
        var a = await _reader.ReadAsync(_fixture.WritePackage("A", "1.0.0"), CancellationToken.None);
        var b = await _reader.ReadAsync(_fixture.WritePackage("B", "1.0.0"), CancellationToken.None);

        Assert.NotEqual(a.Sha256, b.Sha256);
    }

    [Fact]
    public async Task Missing_file_fails_closed()
    {
        await Assert.ThrowsAsync<DotNetReleaseException>(
            () => _reader.ReadAsync(Path.Combine(_fixture.Root, "nope.nupkg"), CancellationToken.None));
    }

    [Fact]
    public async Task Corrupt_archive_fails_with_a_release_exception()
    {
        await Assert.ThrowsAsync<DotNetReleaseException>(
            () => _reader.ReadAsync(_fixture.WriteCorruptPackage(), CancellationToken.None));
    }

    [Fact]
    public async Task Package_without_a_nuspec_fails_closed()
    {
        var path = _fixture.WritePackage(nuspecEntryName: "not-a-nuspec.txt");

        await Assert.ThrowsAsync<DotNetReleaseException>(
            () => _reader.ReadAsync(path, CancellationToken.None));
    }

    [Fact]
    public async Task Nuspec_with_malformed_xml_fails_closed()
    {
        var path = _fixture.WritePackage(nuspecOverride: "<package><metadata><id>Broken");

        await Assert.ThrowsAsync<DotNetReleaseException>(
            () => _reader.ReadAsync(path, CancellationToken.None));
    }

    [Fact]
    public async Task Nuspec_without_a_version_fails_closed()
    {
        var path = _fixture.WritePackage(nuspecOverride: """
            <?xml version="1.0" encoding="utf-8"?>
            <package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
              <metadata>
                <id>SkiaSharp</id>
                <authors>Test</authors>
                <description>Test</description>
              </metadata>
            </package>
            """);

        await Assert.ThrowsAsync<DotNetReleaseException>(
            () => _reader.ReadAsync(path, CancellationToken.None));
    }

    /// <summary>
    /// A real package read from disk must satisfy staging policy, including normalized-version
    /// agreement.
    /// </summary>
    [Fact]
    public async Task A_real_package_passes_Core_staging_validation()
    {
        var policy = ReleasePolicy.Parse("""
        {
          "schemaVersion": 1,
          "repositories": { "dotnet/skiasharp": { "workload": false, "channel": { "name": ".NET Libraries", "id": 1648 } } }
        }
        """);

        var read = await _reader.ReadAsync(_fixture.WritePackage("SkiaSharp", "3.119.0.0", fileName: "SkiaSharp.3.119.0.nupkg"), CancellationToken.None);
        var resolved = new ResolvedRelease
        {
            ToolVersion = "1.0.0-test",
            CreatedUtc = DateTimeOffset.UnixEpoch,
            Repository = "dotnet/skiasharp",
            RepositoryUrl = "https://github.com/dotnet/skiasharp",
            Commit = new string('a', 40),
            BarBuildId = 4242,
            RepositoryOrigin = RepositoryOrigin.GitHubRepository,
            Workload = false,
            Channel = new ChannelReference(".NET Libraries", 1648),
        };

        var plan = StagePlanner.Create(resolved, policy, [read], new StageOptions(), DateTimeOffset.UnixEpoch, "1.0.0-test");

        // The nuspec said 3.119.0.0; NuGet normalizes it to 3.119.0, which is what the
        // availability query will use.
        Assert.Equal("3.119.0", plan.Sets[0].Packages[0].NormalizedVersion);
    }
}
