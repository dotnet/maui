namespace DotNet.Release;

/// <summary>
/// Verifies a BAR build against the request and the policy. Pure.
/// </summary>
internal static class BuildResolver
{
    /// <summary>
    /// Resolves and verifies exactly one BAR build.
    /// </summary>
    /// <param name="request">What the operator asked for.</param>
    /// <param name="policy">Policy for <see cref="ReleaseRequest.Repository"/>.</param>
    /// <param name="candidates">Builds the registry returned for the query.</param>
    /// <param name="createdUtc">Timestamp to stamp on the result.</param>
    /// <param name="toolVersion">Version to stamp on the result.</param>
    public static ResolvedRelease Resolve(
        ReleaseRequest request,
        RepositoryPolicy policy,
        IReadOnlyList<BarBuild> candidates,
        DateTimeOffset createdUtc,
        string toolVersion)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(candidates);

        if (policy.Repository != request.Repository)
        {
            throw new DotNetReleaseException(
                $"Policy for '{policy.Repository}' was supplied for a release of '{request.Repository}'.");
        }

        if (string.IsNullOrWhiteSpace(request.Commit))
        {
            throw new DotNetReleaseException(
                "A commit must be supplied; a release is always pinned to an exact commit.");
        }

        if (candidates.Count == 0)
        {
            throw new DotNetReleaseException(
                request.BarBuildId is { } id
                    ? $"BAR has no build with ID {id}."
                    : $"BAR has no build for '{request.Repository}' at commit '{request.Commit}'. " +
                      "If the build predates a verified AzDO-to-GitHub mapping it has no GitHub " +
                      "URL and must be resolved with --bar-id.");
        }

        if (candidates.Count > 1)
        {
            throw new DotNetReleaseException(
                $"Expected exactly one BAR build for '{request.Repository}' at commit " +
                $"'{request.Commit}', found {candidates.Count}: " +
                $"{string.Join(", ", candidates.Select(b => b.Id))}.");
        }

        var build = candidates[0];

        // A build resolved by ID has not been matched against the requested repository or
        // commit yet, so both are verified on every resolution path.
        if (!string.Equals(build.Commit?.Trim(), request.Commit.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new DotNetReleaseException(
                $"BAR build {build.Id} is for commit '{build.Commit}', not the requested '{request.Commit}'.");
        }

        var (identity, origin) = ResolveIdentity(build);

        if (identity != request.Repository)
        {
            throw new DotNetReleaseException(
                $"BAR build {build.Id} belongs to '{identity}', not '{request.Repository}'.");
        }

        if (policy.Channel is { } required)
        {
            // Both name and ID must match, and exactly one channel may do so: a name that
            // matches a different ID means the channel was renamed or duplicated, which is
            // not something a release should silently accept.
            var matches = build.Channels
                .Where(c => string.Equals(c.Name, required.Name, StringComparison.Ordinal) && c.Id == required.Id)
                .ToList();

            if (matches.Count != 1)
            {
                throw new DotNetReleaseException(
                    $"BAR build {build.Id} must be assigned to '{required.Name}' (channel {required.Id}), " +
                    $"but has {matches.Count} such assignment. Its channels are: " +
                    $"{(build.Channels.Count == 0 ? "(none)" : string.Join(", ", build.Channels.Select(c => $"{c.Name} ({c.Id})")))}.");
            }
        }

        return new ResolvedRelease
        {
            ToolVersion = toolVersion,
            CreatedUtc = createdUtc.ToUniversalTime(),
            Repository = request.Repository.FullName,
            RepositoryUrl = request.Repository.GitHubUrl,
            Commit = request.Commit.Trim(),
            BarBuildId = build.Id,
            RepositoryOrigin = origin,
            Workload = policy.Workload,
            Channel = policy.Channel,
        };
    }

    /// <summary>
    /// Establishes the build's repository identity, preferring BAR's recorded GitHub URL and
    /// falling back to Arcade's mirror-name convention when BAR has none.
    /// </summary>
    private static (RepositoryId Identity, RepositoryOrigin Origin) ResolveIdentity(BarBuild build) =>
        string.IsNullOrWhiteSpace(build.GitHubRepository)
            ? (RepositoryId.FromAzureDevOpsMirror(build.AzureDevOpsRepository), RepositoryOrigin.AzureDevOpsMirrorConvention)
            : (RepositoryId.FromGitHubUrl(build.GitHubRepository), RepositoryOrigin.GitHubRepository);
}
