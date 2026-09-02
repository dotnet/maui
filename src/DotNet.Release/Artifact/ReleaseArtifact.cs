namespace DotNet.Release;

/// <summary>Reads and validates shared package-set artifact structure.</summary>
internal static class ReleaseArtifact
{
    public const string ManifestFileName = "release-manifest.json";

    internal static bool IsSinglePathComponent(string value) =>
        !string.IsNullOrWhiteSpace(value) &&
        !Path.IsPathRooted(value) &&
        Path.GetFileName(value) == value;

    internal static Dictionary<string, string> ReadPackageHashes(string directory)
    {
        var hashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!Directory.Exists(directory))
        {
            return hashes;
        }

        foreach (var file in Directory.EnumerateFiles(directory, "*.nupkg", SearchOption.AllDirectories))
        {
            var hash = Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(file)));
            hashes[Path.GetRelativePath(directory, file)] = hash;
        }

        return hashes;
    }

}
