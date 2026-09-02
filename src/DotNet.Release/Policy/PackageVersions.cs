using NuGet.Versioning;

namespace DotNet.Release;

/// <summary>
/// Package version normalization.
/// </summary>
/// <remarks>
/// NuGet.org availability queries require the normalized version. The value is parsed from
/// nuspec metadata with <c>NuGetVersion</c>, not inferred from the package filename.
/// <see cref="IsNormalizedForm"/> lets staging reject an adapter result that disagrees with
/// NuGet's normalization rules.
/// </remarks>
internal static class PackageVersions
{
    /// <summary>Parses a nuspec version and returns its NuGet-normalized form.</summary>
    public static string Normalize(string? version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new DotNetReleaseException("The package version is empty.");
        }

        if (!NuGetVersion.TryParse(version.Trim(), out var parsed))
        {
            throw new DotNetReleaseException($"Package version '{version}' is not a valid NuGet version.");
        }

        return parsed.ToNormalizedString();
    }

    /// <summary>True when <paramref name="normalized"/> is the normalized form of <paramref name="version"/>.</summary>
    public static bool IsNormalizedForm(string? version, string? normalized) =>
        NuGetVersion.TryParse(version?.Trim(), out var parsed) &&
        string.Equals(parsed.ToNormalizedString(), normalized?.Trim(), StringComparison.OrdinalIgnoreCase);
}
