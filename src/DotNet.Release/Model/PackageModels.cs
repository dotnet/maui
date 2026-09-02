using System.Text.Json.Serialization;

namespace DotNet.Release;

/// <summary>Package identity and content hash used from drop inspection through publication.</summary>
internal sealed record ReleasePackage
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("version")]
    public required string Version { get; init; }

    [JsonPropertyName("normalizedVersion")]
    public required string NormalizedVersion { get; init; }

    [JsonPropertyName("fileName")]
    public required string FileName { get; init; }

    [JsonPropertyName("sha256")]
    public required string Sha256 { get; init; }

    [JsonIgnore]
    public string IdentityKey => $"{Id.ToLowerInvariant()}/{NormalizedVersion.ToLowerInvariant()}";
}

/// <summary>One independently validated and published package set.</summary>
internal sealed record ReleasePackageSet
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("order")]
    public required int Order { get; init; }

    [JsonPropertyName("artifactName")]
    public required string ArtifactName { get; init; }

    [JsonPropertyName("packages")]
    public required IReadOnlyList<ReleasePackage> Packages { get; init; }

    /// <summary>Gets this set's validated directory beneath a release staging directory.</summary>
    public string GetDirectory(string stageDirectory)
    {
        if (!ReleaseArtifact.IsSinglePathComponent(ArtifactName))
        {
            throw new DotNetReleaseException($"Package set artifact name '{ArtifactName}' must be one directory name.");
        }

        return Path.Combine(stageDirectory, ArtifactName);
    }
}
