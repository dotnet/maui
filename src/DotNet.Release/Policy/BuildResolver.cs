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
    public static ReleaseSource Resolve(ReleaseRequest request, RepositoryPolicy policy, IReadOnlyList<BarBuild> candidates)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(candidates);

        if (policy.Repository != request.Repository)
        {
            throw new DotNetReleaseException($"Policy for '{policy.Repository}' was supplied for a release of '{request.Repository}'.");
        }

        if (request.BarBuildId is <= 0)
        {
            throw new DotNetReleaseException($"BAR build ID '{request.BarBuildId}' must be positive.");
        }

        if (request.Commit is { } requestedCommit &&
            (requestedCommit.Length != 40 || !requestedCommit.All(Uri.IsHexDigit)))
        {
            throw new DotNetReleaseException($"Commit '{requestedCommit}' must be a full 40-character SHA.");
        }

        if (request.Commit is null && request.BarBuildId is null)
        {
            throw new DotNetReleaseException("A commit SHA or BAR build ID must be supplied.");
        }

        var matchingBuilds = request.Commit is null
            ? candidates
            : candidates.Where(candidate =>
                    string.Equals(candidate.Commit?.Trim(), request.Commit, StringComparison.OrdinalIgnoreCase) &&
                    TryResolveIdentity(candidate, out var identity, out _) &&
                    identity == request.Repository)
                .ToList();

        if (matchingBuilds.Count == 0)
        {
            throw new DotNetReleaseException(
                request.BarBuildId is { } id
                    ? $"BAR has no build with ID {id}."
                    : $"BAR has no build for '{request.Repository}' at commit '{request.Commit}'.");
        }

        if (matchingBuilds.Count > 1)
        {
            throw new DotNetReleaseException(
                $"Expected exactly one BAR build for '{request.Repository}', found {matchingBuilds.Count}: " +
                $"{string.Join(", ", matchingBuilds.Select(b => b.Id))}.");
        }

        var build = matchingBuilds[0];

        if (request.BarBuildId is { } requestedId && build.Id != requestedId)
        {
            throw new DotNetReleaseException($"BAR returned build {build.Id} for requested build ID {requestedId}.");
        }

        var commit = build.Commit?.Trim();
        if (commit is not { Length: 40 } || !commit.All(Uri.IsHexDigit))
        {
            throw new DotNetReleaseException($"BAR build {build.Id} has invalid commit '{build.Commit}'; expected a full 40-character SHA.");
        }

        if (request.Commit is { } expectedCommit &&
            !string.Equals(commit, expectedCommit, StringComparison.OrdinalIgnoreCase))
        {
            throw new DotNetReleaseException($"BAR build {build.Id} is for commit '{commit}', not the requested '{expectedCommit}'.");
        }

        var (identity, origin) = ResolveIdentity(build);

        if (identity != request.Repository)
        {
            throw new DotNetReleaseException($"BAR build {build.Id} belongs to '{identity}', not '{request.Repository}'.");
        }

        if (policy.Channel is { } required)
        {
            // Both name and ID must match, and exactly one channel may do so: a name that
            // matches a different ID means the channel was renamed or duplicated, which is
            // not something a release should silently accept.
            var matches = build.Channels
                .Where(c => string.Equals(c.Name, required.Name, StringComparison.Ordinal))
                .ToList();

            if (matches.Count != 1 || matches[0].Id != required.Id)
            {
                throw new DotNetReleaseException(
                    $"BAR build {build.Id} must be assigned to '{required.Name}' (channel {required.Id}), " +
                    "without conflicting same-name channel IDs. Its channels are: " +
                    $"{(build.Channels.Count == 0 ? "(none)" : string.Join(", ", build.Channels.Select(c => $"{c.Name} ({c.Id})")))}.");
            }
        }

        return new ReleaseSource
        {
            Repository = request.Repository.FullName,
            RepositoryUrl = request.Repository.GitHubUrl,
            Commit = commit,
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

    private static bool TryResolveIdentity(BarBuild build, out RepositoryId identity, out RepositoryOrigin origin)
    {
        try
        {
            (identity, origin) = ResolveIdentity(build);
            return true;
        }
        catch (DotNetReleaseException)
        {
            identity = default;
            origin = default;
            return false;
        }
    }
}
