using System.Text.Json.Serialization;

namespace DotNet.Release;

/// <summary>Package identity and hash read from a gathered nupkg.</summary>
internal sealed record DropPackage(
    string FileName,
    string Id,
    string Version,
    string NormalizedVersion,
    string Sha256);

/// <summary>A package selected for release.</summary>
internal sealed record PlannedPackage
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
    public required IReadOnlyList<PlannedPackage> Packages { get; init; }
}
