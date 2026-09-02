using System.Security.Cryptography;
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

/// <summary>
/// One independently validated and published package set, including its staged-file
/// integrity rules.
/// </summary>
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
        if (string.IsNullOrWhiteSpace(ArtifactName) ||
            Path.IsPathRooted(ArtifactName) ||
            Path.GetFileName(ArtifactName) != ArtifactName)
        {
            throw new DotNetReleaseException($"Package set artifact name '{ArtifactName}' must be one directory name.");
        }

        return Path.Combine(stageDirectory, ArtifactName);
    }

    /// <summary>
    /// Validates the package files in a staging directory, optionally after pruning.
    /// </summary>
    /// <remarks>
    /// Every expected pending package must be present with its recorded hash, withheld
    /// packages must be absent, and no unlisted <c>.nupkg</c> may enter the publish glob.
    /// </remarks>
    public void ValidateDirectory(string directory, PruneReport? report = null) =>
        ValidateFiles(ReadPackageHashes(directory), report);

    internal void ValidateFiles(IReadOnlyDictionary<string, string> observed, PruneReport? report = null)
    {
        ArgumentNullException.ThrowIfNull(observed);

        var dispositions = report?.Decisions.ToDictionary(
            decision => decision.Package.FileName,
            decision => decision.Disposition,
            StringComparer.OrdinalIgnoreCase);
        var errors = new List<string>();
        var expected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var package in Packages)
        {
            expected.Add(package.FileName);

            var disposition = dispositions is not null && dispositions.TryGetValue(package.FileName, out var recorded)
                ? recorded
                : PackageDisposition.Pending;
            var shouldBePresent = disposition == PackageDisposition.Pending;
            var isPresent = observed.TryGetValue(package.FileName, out var actualHash);

            if (shouldBePresent && !isPresent)
            {
                errors.Add($"'{Name}' expects '{package.FileName}' to be staged for publication, but it is not present.");
                continue;
            }

            if (!shouldBePresent && isPresent)
            {
                errors.Add($"'{package.FileName}' was withheld from publication but is still staged; it would be pushed by the publish glob.");
                continue;
            }

            if (isPresent && !string.Equals(actualHash, package.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"'{package.FileName}' has hash '{actualHash}' but the manifest recorded " +
                    $"'{package.Sha256}'. It is not the file that was validated.");
            }
        }

        foreach (var fileName in observed.Keys.Where(fileName => !expected.Contains(fileName)).Order(StringComparer.Ordinal))
        {
            errors.Add($"Staging directory for '{Name}' contains '{fileName}', which the release manifest does not list.");
        }

        if (errors.Count > 0)
        {
            throw new DotNetReleaseException(errors);
        }
    }

    private static Dictionary<string, string> ReadPackageHashes(string directory)
    {
        var hashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!Directory.Exists(directory))
        {
            return hashes;
        }

        foreach (var file in Directory.EnumerateFiles(directory, "*.nupkg", SearchOption.AllDirectories))
        {
            var hash = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(file)));
            hashes[Path.GetRelativePath(directory, file)] = hash;
        }

        return hashes;
    }
}
