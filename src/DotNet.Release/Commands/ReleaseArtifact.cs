namespace DotNet.Release;

/// <summary>Reads and validates shared package-set artifact structure.</summary>
internal static class ReleaseArtifact
{
    public const string PlanFileName = "release-plan.json";

    internal static string GetSetDirectory(string stageDirectory, ReleasePackageSet set)
    {
        if (!IsSinglePathComponent(set.ArtifactName))
        {
            throw new DotNetReleaseException($"Package set artifact name '{set.ArtifactName}' must be one directory name.");
        }

        return Path.Combine(stageDirectory, set.ArtifactName);
    }

    internal static bool IsSinglePathComponent(string value) => !string.IsNullOrWhiteSpace(value) && !Path.IsPathRooted(value) &&
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
            hashes[Path.GetRelativePath(directory, file)] = Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(file)));
        }

        return hashes;
    }

    internal static IReadOnlyList<ReleasePackageSet> SelectSets(ReleasePlan plan, string? setName)
    {
        var ordered = plan.Sets.OrderBy(set => set.Order).ToList();

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
                $"The release plan has no package set named '{setName}'. It contains: " +
                $"{string.Join(", ", ordered.Select(set => set.ArtifactName))}.");
        }

        return matched;
    }
}
