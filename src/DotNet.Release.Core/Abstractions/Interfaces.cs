namespace DotNet.Release.Core;

/// <summary>
/// Read-only access to the Build Asset Registry.
/// </summary>
/// <remarks>
/// <para>
/// Implemented in <c>DotNet.Release.Maestro</c> over
/// <c>Microsoft.DotNet.ProductConstructionService.Client</c>. Deliberately read-only: adding
/// a build to a channel is a mutation and stays an explicit <c>darc add-build-to-channel</c>
/// step in pipeline YAML (docs/design.md section 2).
/// </para>
/// <para>
/// Replacing <c>darc get-build</c> with a typed client removes a real production failure
/// mode: darc writes "Could not any builds matching the given criteria" to <b>stdout</b>,
/// which the current script captures into its JSON variable and discards, leaving only a
/// generic error. There is no stdout here to misinterpret.
/// </para>
/// </remarks>
public interface IBuildRegistry
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
/// Reads package identities out of <c>.nupkg</c> files in a gathered drop.
/// </summary>
/// <remarks>
/// Implemented in <c>DotNet.Release.NuGet</c> using <c>PackageArchiveReader</c> and
/// <c>NuGetVersion.ToNormalizedString()</c>, replacing roughly 60 lines of hand-rolled zip,
/// XML and XPath plus a substring-based version hack.
/// </remarks>
public interface IPackageIdentityReader
{
    /// <summary>Reads the identity and content hash of one package file.</summary>
    Task<Result<DropPackage>> ReadAsync(string packageFilePath, CancellationToken cancellationToken);
}

/// <summary>
/// Read-only NuGet.org availability queries.
/// </summary>
/// <remarks>
/// <para>
/// Implemented in <c>DotNet.Release.NuGet</c> over <c>FindPackageByIdResource</c>, replacing
/// a hand-rolled HTTP HEAD and retry loop against the flat-container API.
/// </para>
/// <para>
/// There is no publishing counterpart to this interface anywhere in the codebase.
/// <c>1ES.PublishNuget@1</c> performs every push, so the tool never holds a NuGet.org
/// credential (docs/design.md section 3).
/// </para>
/// </remarks>
public interface IPackageAvailabilityProbe
{
    /// <summary>
    /// Returns whether each identity is currently resolvable on NuGet.org, keyed by
    /// <see cref="PlannedPackage.IdentityKey"/>.
    /// </summary>
    Task<IReadOnlyDictionary<string, bool>> GetAvailabilityAsync(
        IReadOnlyList<PlannedPackage> packages,
        CancellationToken cancellationToken);
}
