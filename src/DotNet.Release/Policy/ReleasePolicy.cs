using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNet.Release;

/// <summary>Policy for one releasable repository.</summary>
public sealed record RepositoryPolicy(RepositoryId Repository, bool Workload, ChannelReference? Channel);

/// <summary>The workload-set channel and feed for one .NET band.</summary>
public sealed record WorkloadSetPolicy(int Band, string Channel, string Feed);

/// <summary>
/// The declarative, checked-in release policy. Anything not listed fails closed.
/// </summary>
/// <remarks>
/// Repository enablement, workload classification, channel requirements, and workload-set
/// targets are versioned together in <c>config/repositories.json</c>.
/// </remarks>
public sealed class ReleasePolicy
{
    private readonly IReadOnlyDictionary<string, RepositoryPolicy> _repositories;
    private readonly IReadOnlyDictionary<int, WorkloadSetPolicy> _workloadSets;

    private ReleasePolicy(
        IReadOnlyDictionary<string, RepositoryPolicy> repositories,
        IReadOnlyDictionary<int, WorkloadSetPolicy> workloadSets)
    {
        _repositories = repositories;
        _workloadSets = workloadSets;
    }

    public IReadOnlyCollection<RepositoryPolicy> Repositories => (IReadOnlyCollection<RepositoryPolicy>)_repositories.Values;

    /// <summary>Parses policy JSON. Pure: the caller owns reading the file.</summary>
    public static Result<ReleasePolicy> Parse(string json)
    {
        PolicyDocument? document;
        try
        {
            document = JsonSerializer.Deserialize(json, PolicyJsonContext.Default.PolicyDocument);
        }
        catch (JsonException ex)
        {
            return Result<ReleasePolicy>.Failure(
                ErrorCodes.PolicyInvalid,
                $"The release policy is not valid JSON: {ex.Message}");
        }

        if (document is null)
        {
            return Result<ReleasePolicy>.Failure(ErrorCodes.PolicyInvalid, "The release policy is empty.");
        }

        if (document.SchemaVersion != 1)
        {
            return Result<ReleasePolicy>.Failure(
                ErrorCodes.PolicyInvalid,
                $"Unsupported release policy schemaVersion '{document.SchemaVersion}'; expected 1.");
        }

        if (document.Repositories is not { Count: > 0 })
        {
            return Result<ReleasePolicy>.Failure(
                ErrorCodes.PolicyInvalid,
                "The release policy lists no repositories, so every release would fail closed.");
        }

        var errors = new List<ReleaseError>();
        var repositories = new Dictionary<string, RepositoryPolicy>(StringComparer.Ordinal);

        foreach (var (key, entry) in document.Repositories)
        {
            var id = RepositoryId.Parse(key);
            if (id.IsFailure)
            {
                errors.AddRange(id.Errors);
                continue;
            }

            ChannelReference? channel = null;
            if (entry.Channel is not null)
            {
                if (string.IsNullOrWhiteSpace(entry.Channel.Name) || entry.Channel.Id <= 0)
                {
                    errors.Add(new ReleaseError(
                        ErrorCodes.PolicyInvalid,
                        $"Repository '{key}' has a channel without a name or a positive ID."));
                    continue;
                }

                channel = new ChannelReference(entry.Channel.Name.Trim(), entry.Channel.Id);
            }

            if (!repositories.TryAdd(id.Value.FullName, new RepositoryPolicy(id.Value, entry.Workload, channel)))
            {
                errors.Add(new ReleaseError(
                    ErrorCodes.PolicyInvalid,
                    $"Repository '{id.Value.FullName}' is listed more than once."));
            }
        }

        var workloadSets = new Dictionary<int, WorkloadSetPolicy>();
        foreach (var (key, entry) in document.WorkloadSets ?? [])
        {
            if (!int.TryParse(key, out var band) || band <= 0)
            {
                errors.Add(new ReleaseError(
                    ErrorCodes.PolicyInvalid,
                    $"Workload set key '{key}' is not a positive .NET band number."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(entry.Channel) || string.IsNullOrWhiteSpace(entry.Feed))
            {
                errors.Add(new ReleaseError(
                    ErrorCodes.PolicyInvalid,
                    $"Workload set '{key}' must declare both a channel and a feed."));
                continue;
            }

            workloadSets[band] = new WorkloadSetPolicy(band, entry.Channel.Trim(), entry.Feed.Trim());
        }

        return errors.Count > 0
            ? Result<ReleasePolicy>.Failure(errors)
            : Result<ReleasePolicy>.Success(new ReleasePolicy(repositories, workloadSets));
    }

    /// <summary>Looks up a repository. An unlisted repository is an error, never a default.</summary>
    public Result<RepositoryPolicy> GetRepository(RepositoryId repository) =>
        _repositories.TryGetValue(repository.FullName, out var policy)
            ? Result<RepositoryPolicy>.Success(policy)
            : Result<RepositoryPolicy>.Failure(
                ErrorCodes.RepositoryNotAllowed,
                $"Repository '{repository.FullName}' is not enabled for release. " +
                $"Enabled repositories: {string.Join(", ", _repositories.Keys.Order(StringComparer.Ordinal))}.");

    /// <summary>Looks up the workload-set target for a .NET band.</summary>
    public Result<WorkloadSetPolicy> GetWorkloadSet(int band) =>
        _workloadSets.TryGetValue(band, out var policy)
            ? Result<WorkloadSetPolicy>.Success(policy)
            : Result<WorkloadSetPolicy>.Failure(
                ErrorCodes.WorkloadSetNotConfigured,
                $"No workload set channel is configured for .NET {band}.");

    internal sealed class PolicyDocument
    {
        [JsonPropertyName("schemaVersion")]
        public int SchemaVersion { get; init; }

        [JsonPropertyName("repositories")]
        public Dictionary<string, RepositoryEntry>? Repositories { get; init; }

        [JsonPropertyName("workloadSets")]
        public Dictionary<string, WorkloadSetEntry>? WorkloadSets { get; init; }
    }

    internal sealed class RepositoryEntry
    {
        [JsonPropertyName("workload")]
        public bool Workload { get; init; }

        [JsonPropertyName("channel")]
        public ChannelEntry? Channel { get; init; }
    }

    internal sealed class ChannelEntry
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("id")]
        public int Id { get; init; }
    }

    internal sealed class WorkloadSetEntry
    {
        [JsonPropertyName("channel")]
        public string? Channel { get; init; }

        [JsonPropertyName("feed")]
        public string? Feed { get; init; }
    }
}

[JsonSourceGenerationOptions(ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true)]
[JsonSerializable(typeof(ReleasePolicy.PolicyDocument))]
internal sealed partial class PolicyJsonContext : JsonSerializerContext;
