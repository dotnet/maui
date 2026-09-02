using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace DotNet.Release;

internal interface INuGetPackageLookup
{
    Task<IReadOnlyDictionary<string, bool>> GetAvailabilityAsync(IReadOnlyList<PlannedPackage> packages, CancellationToken cancellationToken);
}

internal sealed class NuGetPackageLookup : INuGetPackageLookup, IDisposable
{
    public const string NuGetOrgIndex = "https://api.nuget.org/v3/index.json";

    private readonly SourceCacheContext? _cache;
    private readonly int _maxConcurrency;
    private readonly Func<string, NuGetVersion, CancellationToken, Task<bool>> _lookup;

    public NuGetPackageLookup(string? sourceIndexUrl = null, ILogger? logger = null, int maxConcurrency = 8)
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

    internal NuGetPackageLookup(Func<string, NuGetVersion, CancellationToken, Task<bool>> lookup, int maxConcurrency = 8)
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

    public void Dispose() => _cache?.Dispose();
}
