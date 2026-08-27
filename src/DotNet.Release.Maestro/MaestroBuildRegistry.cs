using DotNet.Release.Core;
using Microsoft.DotNet.ProductConstructionService.Client;

namespace DotNet.Release.Maestro;

/// <summary>
/// Read-only <see cref="IBuildRegistry"/> over the typed Product Construction Service client.
/// </summary>
/// <remarks>
/// <para>
/// Replaces <c>darc get-build</c>. That call is why this adapter exists: darc writes
/// "Could not any builds matching the given criteria" to <b>stdout</b>, the current script
/// captures it into the variable it expects JSON in, checks only <c>$LASTEXITCODE</c>, and
/// throws a generic message — so the actual reason is discarded and the run is
/// undiagnosable from the log. There is no stdout here to misinterpret: a missing build is
/// an empty result and a transport failure is a typed exception.
/// </para>
/// <para>
/// This type touches exactly two members of <see cref="IBuilds"/>,
/// <see cref="IBuilds.GetBuildAsync"/> and <see cref="IBuilds.ListBuildsAsync"/>. It never
/// calls <c>CreateAsync</c> or <c>UpdateAsync</c>, which are BAR mutations; promotion stays
/// an explicit <c>darc add-build-to-channel</c> step in pipeline YAML.
/// </para>
/// </remarks>
public sealed class MaestroBuildRegistry : IBuildRegistry
{
    private const int NotFound = 404;

    private readonly IBuilds _builds;

    /// <param name="builds">The BAR builds endpoint. Narrow by design, so it is trivially fakeable.</param>
    public MaestroBuildRegistry(IBuilds builds)
    {
        ArgumentNullException.ThrowIfNull(builds);
        _builds = builds;
    }

    /// <summary>Creates a registry from a configured API client.</summary>
    public static MaestroBuildRegistry Create(IProductConstructionServiceApi api)
    {
        ArgumentNullException.ThrowIfNull(api);
        return new MaestroBuildRegistry(api.Builds);
    }

    /// <inheritdoc />
    /// <remarks>
    /// The client returns a single build or throws, so this yields either zero or one
    /// result. Core still applies its "exactly one" rule to both resolution paths.
    /// </remarks>
    public async Task<IReadOnlyList<BarBuild>> GetBuildAsync(int barBuildId, CancellationToken cancellationToken)
    {
        if (barBuildId <= 0)
        {
            return [];
        }

        try
        {
            // Asset locations are not needed: the drop is gathered by `darc gather-drop`,
            // and asking for them makes the response substantially larger.
            var build = await _builds
                .GetBuildAsync(barBuildId, includeAssetLocation: false, cancellationToken)
                .ConfigureAwait(false);

            return build is null ? [] : [BarBuildMapper.Map(build)];
        }
        catch (RestApiException ex) when (ex.Response?.Status == NotFound)
        {
            // "No such build" is an expected answer to a lookup, not a transport failure, so
            // it becomes an empty result and Core produces the BAR_BUILD_NOT_FOUND message.
            // Every other status stays an exception rather than being flattened into "none".
            return [];
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BarBuild>> GetBuildsAsync(
        RepositoryId repository,
        string commit,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(commit))
        {
            return [];
        }

        var builds = new List<BarBuild>();

        // BAR records the repository as its GitHub URL, which is what the current pipeline
        // passes to `darc get-build --repo`.
        //
        // loadCollections MUST be true. The service only eager-loads BuildChannels when it
        // is set:
        //
        //     if (loadCollections ?? false) { query = query.Include(b => b.BuildChannels)... }
        //
        // With it false, Build.Channels comes back empty and every required-channel check
        // fails with BAR_CHANNEL_MISSING while BAR shows the build correctly assigned. It is
        // a separate concern from includeAssetLocation on GetBuildAsync, which stays false
        // because `darc gather-drop` downloads the assets.
        var page = _builds.ListBuildsAsync(
            commit: commit.Trim(),
            repository: repository.GitHubUrl,
            loadCollections: true,
            cancellationToken: cancellationToken);

        await foreach (var build in page.ConfigureAwait(false))
        {
            if (build is not null)
            {
                builds.Add(BarBuildMapper.Map(build));
            }
        }

        return builds;
    }
}
