namespace DotNet.Release;

/// <summary>
/// Read-only access to the Build Asset Registry.
/// </summary>
/// <remarks>
/// <para>
/// Implemented by <see cref="MaestroBuildRegistry"/> over
/// <c>Microsoft.DotNet.ProductConstructionService.Client</c>. The interface exposes only
/// build lookup; channel mutation is an explicit gated pipeline operation.
/// </para>
/// <para>
/// Typed results preserve the distinction between no matching build and service failure.
/// </para>
/// </remarks>
internal interface IBuildRegistry
{
    /// <summary>Gets a single build by its BAR ID, or an empty list if there is none.</summary>
    Task<IReadOnlyList<BarBuild>> GetBuildAsync(int barBuildId, CancellationToken cancellationToken);

    /// <summary>Gets the builds registered for a repository at an exact commit.</summary>
    Task<IReadOnlyList<BarBuild>> GetBuildsAsync(
        RepositoryId repository,
        string commit,
        CancellationToken cancellationToken);
}

/// <summary>
/// Read-only NuGet.org availability queries.
/// </summary>
/// <remarks>
/// <para>
/// Implemented by <see cref="PackageAvailabilityProbe"/> over
/// <c>FindPackageByIdResource</c>.
/// </para>
/// <para>
/// There is no publishing counterpart to this interface anywhere in the codebase.
/// <c>1ES.PublishNuget@1</c> performs every push, so the tool never holds a NuGet.org
/// credential (see "Credential boundary" in docs/design.md).
/// </para>
/// </remarks>
internal interface IPackageAvailabilityProbe
{
    /// <summary>
    /// Returns whether each identity is currently resolvable on NuGet.org, keyed by
    /// <see cref="PlannedPackage.IdentityKey"/>.
    /// </summary>
    Task<IReadOnlyDictionary<string, bool>> GetAvailabilityAsync(
        IReadOnlyList<PlannedPackage> packages,
        CancellationToken cancellationToken);
}
