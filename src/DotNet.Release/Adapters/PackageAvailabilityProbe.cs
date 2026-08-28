namespace DotNet.Release;

/// <summary>Read-only availability for a batch of planned NuGet packages.</summary>
internal interface IPackageAvailabilityProbe
{
    Task<IReadOnlyDictionary<string, bool>> GetAvailabilityAsync(
        IReadOnlyList<PlannedPackage> packages,
        CancellationToken cancellationToken);
}

/// <summary>
/// Read-only NuGet.org availability for a set of planned packages.
/// </summary>
/// <remarks>
/// There is no publishing counterpart to this type anywhere in the codebase.
/// <c>1ES.PublishNuget@1</c> performs every push, so the tool never holds a NuGet.org
/// credential (docs/design.md section 3).
/// </remarks>
internal sealed class PackageAvailabilityProbe : IPackageAvailabilityProbe
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
