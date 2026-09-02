using System.Security.Cryptography;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace DotNet.Release;

/// <summary>Provides read-only availability for a batch of exact NuGet package identities.</summary>
internal interface INuGetClient
{
    /// <summary>Returns feed availability keyed by normalized package identity.</summary>
    Task<IReadOnlyDictionary<string, bool>> GetAvailabilityAsync(IReadOnlyList<PlannedPackage> packages, CancellationToken cancellationToken);
}

/// <summary>
/// Reads local package identity through <c>NuGet.Packaging</c> and queries package
/// availability through <c>NuGet.Protocol</c>.
/// </summary>
internal sealed class NuGetClient : INuGetClient, IDisposable
{
    public const string NuGetOrgIndex = "https://api.nuget.org/v3/index.json";

    private readonly SourceCacheContext? _cache;
    private readonly int _maxConcurrency;
    private readonly Func<string, NuGetVersion, CancellationToken, Task<bool>> _lookup;

    public NuGetClient(string? sourceIndexUrl = null, ILogger? logger = null, int maxConcurrency = 8)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxConcurrency, 1);

        _maxConcurrency = maxConcurrency;
        _cache = new SourceCacheContext { NoCache = true, DirectDownload = true };
        var resource = Repository.Factory.GetCoreV3(sourceIndexUrl ?? NuGetOrgIndex).GetResourceAsync<FindPackageByIdResource>();
        var actualLogger = logger ?? NullLogger.Instance;

        _lookup = async (id, version, cancellationToken) =>
            await (await resource.ConfigureAwait(false))
                .DoesPackageExistAsync(id, version, _cache, actualLogger, cancellationToken)
                .ConfigureAwait(false);
    }

    internal NuGetClient(Func<string, NuGetVersion, CancellationToken, Task<bool>> lookup, int maxConcurrency = 8)
    {
        ArgumentNullException.ThrowIfNull(lookup);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxConcurrency, 1);

        _lookup = lookup;
        _maxConcurrency = maxConcurrency;
    }

    public async Task<IReadOnlyDictionary<string, bool>> GetAvailabilityAsync(IReadOnlyList<PlannedPackage> packages, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(packages);

        var distinct = packages
            .GroupBy(package => package.IdentityKey, StringComparer.Ordinal)
            .Select(group => group.First()).ToList();

        using var throttle = new SemaphoreSlim(_maxConcurrency);
        var lookups = distinct.Select(async package =>
        {
            if (!NuGetVersion.TryParse(package.NormalizedVersion, out var version))
            {
                throw new DotNetReleaseException($"Package version '{package.NormalizedVersion}' is not a valid NuGet version.");
            }

            await throttle.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var exists = await _lookup(package.Id, version, cancellationToken).ConfigureAwait(false);
                return (package.IdentityKey, Exists: exists);
            }
            finally
            {
                throttle.Release();
            }
        });

        return (await Task.WhenAll(lookups).ConfigureAwait(false))
            .ToDictionary(result => result.IdentityKey, result => result.Exists, StringComparer.Ordinal);
    }

    /// <summary>Reads identity, normalized version, and SHA-256 from one local package archive.</summary>
    public static async Task<DropPackage> ReadPackageAsync(string packageFilePath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageFilePath);

        var fileName = Path.GetFileName(packageFilePath);

        if (!File.Exists(packageFilePath))
        {
            throw new DotNetReleaseException($"Package file '{packageFilePath}' was not found.");
        }

        string id;
        string version;
        try
        {
            using var stream = File.OpenRead(packageFilePath);
            using var reader = new PackageArchiveReader(stream);
            var identity = reader.NuspecReader.GetIdentity();

            if (identity is null || string.IsNullOrWhiteSpace(identity.Id) || identity.Version is null)
            {
                throw new DotNetReleaseException($"Package '{fileName}' has no ID or version.");
            }

            id = identity.Id.Trim();
            // Preserve nuspec text for audit; normalized form is used for feed lookup.
            version = identity.Version.ToFullString();
        }
        catch (Exception ex) when (ex is InvalidDataException
            or global::NuGet.Packaging.Core.PackagingException
            or System.Xml.XmlException)
        {
            throw new DotNetReleaseException($"Package '{fileName}' could not be read: {ex.Message}");
        }

        var normalized = PackageVersions.Normalize(version);
        await using var hashStream = File.OpenRead(packageFilePath);
        var hash = await SHA256.HashDataAsync(hashStream, cancellationToken).ConfigureAwait(false);
        return new DropPackage(fileName, id, version, normalized, Convert.ToHexStringLower(hash));
    }

    public void Dispose() => _cache?.Dispose();
}
