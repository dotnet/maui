using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNet.Release;

/// <summary>Policy for one releasable repository and any historical identities BAR may report for it.</summary>
internal sealed record RepositoryPolicy(
    RepositoryId Repository,
    bool Workload,
    ChannelReference? Channel,
    IReadOnlyList<RepositoryId> BarRepositoryAliases)
{
    /// <summary>Whether a repository identity reported by BAR belongs to this policy.</summary>
    public bool MatchesBarRepository(RepositoryId repository) =>
        repository == Repository || BarRepositoryAliases.Contains(repository);
}

/// <summary>
/// The declarative, checked-in release policy. Anything not listed fails closed.
/// </summary>
/// <remarks>
/// Repository enablement, historical BAR identities, workload classification, channel
/// requirements, and workload-set targets are versioned together in
/// <c>config/repositories.json</c>.
/// </remarks>
internal sealed class ReleasePolicy
{
    private readonly IReadOnlyDictionary<string, RepositoryPolicy> _repositories;
    private readonly IReadOnlyDictionary<int, WorkloadSet> _workloadSets;

    private ReleasePolicy(IReadOnlyDictionary<string, RepositoryPolicy> repositories, IReadOnlyDictionary<int, WorkloadSet> workloadSets)
    {
        _repositories = repositories;
        _workloadSets = workloadSets;
    }

    public IReadOnlyCollection<RepositoryPolicy> Repositories => (IReadOnlyCollection<RepositoryPolicy>)_repositories.Values;

    /// <summary>Parses and validates repository and workload-set policy JSON.</summary>
    /// <remarks>The caller owns reading the policy file.</remarks>
    public static ReleasePolicy Parse(string json)
    {
        PolicyDocument? document;
        try
        {
            document = JsonSerializer.Deserialize<PolicyDocument>(json, new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            });
        }
        catch (JsonException ex)
        {
            throw new DotNetReleaseException($"The release policy is not valid JSON: {ex.Message}");
        }

        if (document is null)
        {
            throw new DotNetReleaseException("The release policy is empty.");
        }

        if (document.SchemaVersion != 1)
        {
            throw new DotNetReleaseException($"Unsupported release policy schemaVersion '{document.SchemaVersion}'; expected 1.");
        }

        if (document.Repositories is not { Count: > 0 })
        {
            throw new DotNetReleaseException("The release policy lists no repositories, so every release would fail closed.");
        }

        var errors = new List<string>();
        var repositories = new Dictionary<string, RepositoryPolicy>(StringComparer.Ordinal);

        foreach (var (key, entry) in document.Repositories)
        {
            RepositoryId id;
            try
            {
                id = RepositoryId.Parse(key);
            }
            catch (DotNetReleaseException ex)
            {
                errors.Add(ex.Message);
                continue;
            }

            ChannelReference? channel = null;
            if (entry.Channel is not null)
            {
                if (string.IsNullOrWhiteSpace(entry.Channel.Name) || entry.Channel.Id <= 0)
                {
                    errors.Add($"Repository '{key}' has a channel without a name or a positive ID.");
                    continue;
                }

                channel = new ChannelReference(entry.Channel.Name.Trim(), entry.Channel.Id);
            }

            var aliases = new List<RepositoryId>();
            foreach (var value in entry.BarRepositoryAliases ?? [])
            {
                RepositoryId alias;
                try
                {
                    alias = RepositoryId.Parse(value);
                }
                catch (DotNetReleaseException ex)
                {
                    errors.Add($"Repository '{id.FullName}' has invalid BAR repository alias: {ex.Message}");
                    continue;
                }

                if (alias == id)
                {
                    errors.Add($"Repository '{id.FullName}' lists itself as a BAR repository alias.");
                }
                else if (aliases.Contains(alias))
                {
                    errors.Add($"Repository '{id.FullName}' lists BAR repository alias '{alias}' more than once.");
                }
                else
                {
                    aliases.Add(alias);
                }
            }

            if (!repositories.TryAdd(id.FullName, new RepositoryPolicy(id, entry.Workload, channel, [.. aliases])))
            {
                errors.Add($"Repository '{id.FullName}' is listed more than once.");
            }
        }

        var repositoryOwners = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var repository in repositories.Values)
        {
            foreach (var identity in repository.BarRepositoryAliases.Prepend(repository.Repository))
            {
                if (repositoryOwners.TryGetValue(identity.FullName, out var owner))
                {
                    errors.Add($"BAR repository identity '{identity}' is assigned to both '{owner}' and '{repository.Repository}'.");
                }
                else
                {
                    repositoryOwners.Add(identity.FullName, repository.Repository.FullName);
                }
            }
        }

        var workloadSets = new Dictionary<int, WorkloadSet>();
        foreach (var (key, entry) in document.WorkloadSets ?? [])
        {
            if (!int.TryParse(key, out var band) || band <= 0)
            {
                errors.Add($"Workload set key '{key}' is not a positive .NET band number.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(entry.Channel) || string.IsNullOrWhiteSpace(entry.Feed))
            {
                errors.Add($"Workload set '{key}' must declare both a channel and a feed.");
                continue;
            }

            workloadSets[band] = new WorkloadSet(band, entry.Channel.Trim(), entry.Feed.Trim());
        }

        if (errors.Count > 0)
        {
            throw new DotNetReleaseException(errors);
        }

        return new ReleasePolicy(repositories, workloadSets);
    }

    /// <summary>Looks up a repository. An unlisted repository is an error, never a default.</summary>
    public RepositoryPolicy GetRepository(RepositoryId repository) => _repositories.TryGetValue(repository.FullName, out var policy) ? policy
            : throw new DotNetReleaseException($"Repository '{repository.FullName}' is not enabled for release. " +
                $"Enabled repositories: {string.Join(", ", _repositories.Keys.Order(StringComparer.Ordinal))}.");

    /// <summary>Looks up the workload-set target for a .NET band.</summary>
    public WorkloadSet GetWorkloadSet(int band) => _workloadSets.TryGetValue(band, out var policy) ? policy : throw new DotNetReleaseException(
                $"No workload set channel is configured for .NET {band}.");

    /// <summary>Raw JSON representation of the repository and workload-set policy document.</summary>
    internal sealed class PolicyDocument
    {
        [JsonPropertyName("schemaVersion")]
        public int SchemaVersion { get; init; }

        [JsonPropertyName("repositories")]
        public Dictionary<string, RepositoryEntry>? Repositories { get; init; }

        [JsonPropertyName("workloadSets")]
        public Dictionary<string, WorkloadSetEntry>? WorkloadSets { get; init; }
    }

    /// <summary>Raw JSON policy for one repository.</summary>
    internal sealed class RepositoryEntry
    {
        [JsonPropertyName("workload")]
        public bool Workload { get; init; }

        [JsonPropertyName("channel")]
        public ChannelEntry? Channel { get; init; }

        [JsonPropertyName("barRepositoryAliases")]
        public List<string>? BarRepositoryAliases { get; init; }
    }

    /// <summary>Raw JSON reference to a required BAR channel.</summary>
    internal sealed class ChannelEntry
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("id")]
        public int Id { get; init; }
    }

    /// <summary>Raw JSON workload-set channel and feed entry.</summary>
    internal sealed class WorkloadSetEntry
    {
        [JsonPropertyName("channel")]
        public string? Channel { get; init; }

        [JsonPropertyName("feed")]
        public string? Feed { get; init; }
    }
}
