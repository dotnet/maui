using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace DotNet.Release;

/// <summary>
/// Asks whether one package identity is resolvable on a feed.
/// </summary>
/// <remarks>
/// A seam, so the aggregation in <see cref="PackageAvailabilityProbe"/> can be tested
/// without a network. Read-only by construction: it answers a question and exposes nothing
/// that could publish.
/// </remarks>
public interface IPackageExistenceChecker
{
    Task<bool> ExistsAsync(string id, string normalizedVersion, CancellationToken cancellationToken);
}

/// <summary>
/// Checks package existence through the feed's flat-container resource.
/// </summary>
/// <remarks>
/// Replaces the current pipeline's hand-rolled <c>Invoke-WebRequest -Method Head</c> against
/// <c>api.nuget.org/v3-flatcontainer/...</c> with its own retry and status-code table.
/// <c>FindPackageByIdResource</c> already implements the protocol, the retries and the
/// transient-status handling.
/// </remarks>
public sealed class FlatContainerExistenceChecker : IPackageExistenceChecker, IDisposable
{
    /// <summary>The production feed. Queried read-only; nothing is ever pushed here.</summary>
    public const string NuGetOrgIndex = "https://api.nuget.org/v3/index.json";

    private readonly SourceCacheContext _cache;
    private readonly ILogger _logger;
    private readonly Task<FindPackageByIdResource> _resource;

    public FlatContainerExistenceChecker(string? sourceIndexUrl = null, ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;

        // Availability must reflect the feed right now: a cached "missing" would let the
        // publish job re-push a package that landed moments earlier, and NuGet's task treats
        // the resulting 409 as fatal.
        _cache = new SourceCacheContext { NoCache = true, DirectDownload = true };

        var repository = Repository.Factory.GetCoreV3(sourceIndexUrl ?? NuGetOrgIndex);
        _resource = repository.GetResourceAsync<FindPackageByIdResource>();
    }

    public async Task<bool> ExistsAsync(string id, string normalizedVersion, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedVersion);

        if (!NuGetVersion.TryParse(normalizedVersion, out var version))
        {
            throw new ArgumentException(
                $"'{normalizedVersion}' is not a valid NuGet version.", nameof(normalizedVersion));
        }

        var resource = await _resource.ConfigureAwait(false);

        return await resource
            .DoesPackageExistAsync(id, version, _cache, _logger, cancellationToken)
            .ConfigureAwait(false);
    }

    public void Dispose() => _cache.Dispose();
}

/// <summary>
/// Read-only NuGet.org availability for a set of planned packages.
/// </summary>
/// <remarks>
/// There is no publishing counterpart to this type anywhere in the codebase.
/// <c>1ES.PublishNuget@1</c> performs every push, so the tool never holds a NuGet.org
/// credential (docs/design.md section 3).
/// </remarks>
public sealed class PackageAvailabilityProbe : IPackageAvailabilityProbe
{
    private readonly IPackageExistenceChecker _checker;
    private readonly int _maxConcurrency;

    public PackageAvailabilityProbe(IPackageExistenceChecker checker, int maxConcurrency = 8)
    {
        ArgumentNullException.ThrowIfNull(checker);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxConcurrency, 1);

        _checker = checker;
        _maxConcurrency = maxConcurrency;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, bool>> GetAvailabilityAsync(
        IReadOnlyList<PlannedPackage> packages,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(packages);

        var results = new Dictionary<string, bool>(StringComparer.Ordinal);

        // Distinct because the same identity can legitimately appear in more than one set,
        // and querying it twice would be wasted work with a chance of disagreeing answers.
        var distinct = packages
            .GroupBy(p => p.IdentityKey, StringComparer.Ordinal)
            .Select(g => g.First())
            .ToList();

        using var throttle = new SemaphoreSlim(_maxConcurrency);
        var lookups = distinct.Select(async package =>
        {
            await throttle.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var exists = await _checker
                    .ExistsAsync(package.Id, package.NormalizedVersion, cancellationToken)
                    .ConfigureAwait(false);

                return (package.IdentityKey, Exists: exists);
            }
            finally
            {
                throttle.Release();
            }
        });

        foreach (var (key, exists) in await Task.WhenAll(lookups).ConfigureAwait(false))
        {
            results[key] = exists;
        }

        return results;
    }
}
