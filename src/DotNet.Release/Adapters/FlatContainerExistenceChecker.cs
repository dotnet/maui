using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace DotNet.Release;

/// <summary>Asks whether one package identity is resolvable on a NuGet feed.</summary>
internal interface IPackageExistenceChecker
{
    Task<bool> ExistsAsync(
        string id,
        string normalizedVersion,
        CancellationToken cancellationToken);
}

/// <summary>Checks package existence through the feed's flat-container resource.</summary>
internal sealed class FlatContainerExistenceChecker : IPackageExistenceChecker, IDisposable
{
    public const string NuGetOrgIndex = "https://api.nuget.org/v3/index.json";

    private readonly SourceCacheContext _cache;
    private readonly ILogger _logger;
    private readonly Task<FindPackageByIdResource> _resource;

    public FlatContainerExistenceChecker(string? sourceIndexUrl = null, ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;

        // A cached negative can cause a package that just became visible to be submitted
        // again, and the NuGet publish task treats the resulting HTTP 409 as fatal.
        _cache = new SourceCacheContext { NoCache = true, DirectDownload = true };

        var repository = Repository.Factory.GetCoreV3(sourceIndexUrl ?? NuGetOrgIndex);
        _resource = repository.GetResourceAsync<FindPackageByIdResource>();
    }

    public async Task<bool> ExistsAsync(
        string id,
        string normalizedVersion,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedVersion);

        if (!NuGetVersion.TryParse(normalizedVersion, out var version))
        {
            throw new ArgumentException(
                $"'{normalizedVersion}' is not a valid NuGet version.",
                nameof(normalizedVersion));
        }

        var resource = await _resource.ConfigureAwait(false);

        return await resource
            .DoesPackageExistAsync(id, version, _cache, _logger, cancellationToken)
            .ConfigureAwait(false);
    }

    public void Dispose() => _cache.Dispose();
}
