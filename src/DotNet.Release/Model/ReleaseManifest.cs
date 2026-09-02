using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNet.Release;

/// <summary>The immutable contract artifact serialized as <c>release-manifest.json</c>.</summary>
internal sealed record ReleaseManifest
{
    public const string FileName = "release-manifest.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    [JsonPropertyName("toolVersion")]
    public required string ToolVersion { get; init; }

    [JsonPropertyName("createdUtc")]
    public required DateTimeOffset CreatedUtc { get; init; }

    [JsonPropertyName("source")]
    public required ReleaseSource Source { get; init; }

    [JsonPropertyName("workloadSet")]
    public WorkloadSet? WorkloadSet { get; init; }

    [JsonPropertyName("sets")]
    public required IReadOnlyList<ReleasePackageSet> Sets { get; init; }

    /// <summary>Every package across every set, in publication order.</summary>
    public IEnumerable<ReleasePackage> AllPackages =>
        Sets.OrderBy(s => s.Order).SelectMany(s => s.Packages);

    /// <summary>Selects all package sets or the one matching an artifact name.</summary>
    public IReadOnlyList<ReleasePackageSet> SelectSets(string? setName)
    {
        var ordered = Sets.OrderBy(set => set.Order).ToList();

        if (string.IsNullOrWhiteSpace(setName))
        {
            return ordered;
        }

        var matched = ordered
            .Where(set => string.Equals(set.ArtifactName, setName.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matched.Count == 0)
        {
            throw new DotNetReleaseException(
                $"The release manifest has no package set named '{setName}'. It contains: " +
                $"{string.Join(", ", ordered.Select(set => set.ArtifactName))}.");
        }

        return matched;
    }

    /// <summary>Serializes the manifest into its deterministic JSON representation.</summary>
    public string Serialize() => JsonSerializer.Serialize(this, JsonOptions);

    /// <summary>Computes the lower-case SHA-256 of the manifest's deterministic JSON bytes.</summary>
    public string ComputeHash() => ComputeHash(Serialize());

    /// <summary>Deserializes a release manifest.</summary>
    public static ReleaseManifest Deserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<ReleaseManifest>(json, JsonOptions) ??
                throw new DotNetReleaseException("The release manifest is empty.");
        }
        catch (JsonException ex)
        {
            throw new DotNetReleaseException($"The release manifest is not valid: {ex.Message}");
        }
    }

    /// <summary>Verifies the exact input bytes against the expected hash, then deserializes them.</summary>
    public static ReleaseManifest Deserialize(string json, string expectedHash)
    {
        var actualHash = ComputeHash(json);
        if (!string.Equals(actualHash, expectedHash?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new DotNetReleaseException($"Release manifest hash '{actualHash}' does not match the prepared hash '{expectedHash}'.");
        }

        return Deserialize(json);
    }

    private static string ComputeHash(string content) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(content)));
}

/// <summary>The workload-set channel a workload build is promoted to.</summary>
internal sealed record WorkloadSet(
    [property: JsonPropertyName("band")] int Band,
    [property: JsonPropertyName("channel")] string Channel,
    [property: JsonPropertyName("feed")] string Feed);

/// <summary>
/// The verified source of a release.
/// </summary>
internal sealed record ReleaseSource
{
    [JsonPropertyName("repository")]
    public required string Repository { get; init; }

    [JsonPropertyName("repositoryUrl")]
    public required string RepositoryUrl { get; init; }

    [JsonPropertyName("commit")]
    public required string Commit { get; init; }

    [JsonPropertyName("barBuildId")]
    public required int BarBuildId { get; init; }

    [JsonPropertyName("repositoryOrigin")]
    public required RepositoryOrigin RepositoryOrigin { get; init; }

    [JsonPropertyName("workload")]
    public required bool Workload { get; init; }

    [JsonPropertyName("channel")]
    public ChannelReference? Channel { get; init; }
}
