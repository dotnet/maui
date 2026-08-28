namespace DotNet.Release;

/// <summary>Reads and validates shared package-set artifact structure.</summary>
internal static class ReleaseArtifact
{
    public const string PlanFileName = "release-plan.json";

    internal static Result<string> GetSetDirectory(
        string stageDirectory,
        ReleasePackageSet set)
    {
        if (!IsSinglePathComponent(set.ArtifactName))
        {
            return Result<string>.Failure(
                ErrorCodes.PlanSchemaInvalid,
                $"Package set artifact name '{set.ArtifactName}' must be one directory name.");
        }

        return Result<string>.Success(Path.Combine(stageDirectory, set.ArtifactName));
    }

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

        foreach (var file in Directory.EnumerateFiles(
            directory,
            "*.nupkg",
            SearchOption.AllDirectories))
        {
            hashes[Path.GetRelativePath(directory, file)] = Convert.ToHexStringLower(
                System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(file)));
        }

        return hashes;
    }

    internal static Result<bool> ValidateSetMarker(
        string setDirectory,
        ReleasePackageSet set,
        ResolvedRelease source,
        string? requestedSetName)
    {
        if (string.IsNullOrWhiteSpace(requestedSetName))
        {
            return Result<bool>.Success(true);
        }

        var markerPath = Path.Combine(setDirectory, ReleaseSetMarker.FileName);
        if (!File.Exists(markerPath))
        {
            return ReleaseSetMarker.Validate(null, set, source);
        }

        var marker = ReleasePlanSerializer.DeserializeSetMarker(File.ReadAllText(markerPath));
        return marker.IsFailure
            ? marker.ToFailure<bool>()
            : ReleaseSetMarker.Validate(marker.Value, set, source);
    }

    internal static Result<IReadOnlyList<ReleasePackageSet>> SelectSets(
        ReleasePlan plan,
        string? setName)
    {
        var ordered = plan.Sets.OrderBy(set => set.Order).ToList();

        if (string.IsNullOrWhiteSpace(setName))
        {
            return Result<IReadOnlyList<ReleasePackageSet>>.Success(ordered);
        }

        var matched = ordered
            .Where(set => string.Equals(
                set.ArtifactName,
                setName.Trim(),
                StringComparison.OrdinalIgnoreCase))
            .ToList();

        return matched.Count > 0
            ? Result<IReadOnlyList<ReleasePackageSet>>.Success(matched)
            : Result<IReadOnlyList<ReleasePackageSet>>.Failure(
                ErrorCodes.PackageSetNotFound,
                $"The release plan has no package set named '{setName}'. It contains: " +
                $"{string.Join(", ", ordered.Select(set => set.ArtifactName))}.");
    }
}
