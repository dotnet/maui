using NuGet.Versioning;

namespace DotNet.Release.Core;

/// <summary>
/// Package version normalization.
/// </summary>
/// <remarks>
/// <para>
/// This replaces the current pipeline's normalized-version hack, which takes a substring of
/// the file name after the package ID:
/// </para>
/// <code>normalizedVersion = package.BaseName.Substring(("$id.").Length)</code>
/// <para>
/// That is wrong whenever the file name's casing differs from the nuspec ID, and it
/// propagates whatever version string the file name happens to carry rather than the
/// normalized form NuGet.org indexes under. Since the normalized version is what the
/// availability query is built from, a wrong value silently reports a published package as
/// missing.
/// </para>
/// <para>
/// <c>NuGet.Versioning</c> is a pure parser with no I/O, so this stays in Core.
/// </para>
/// </remarks>
public static class PackageVersions
{
    /// <summary>Parses a nuspec version and returns its NuGet-normalized form.</summary>
    public static Result<string> Normalize(string? version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return Result<string>.Failure(ErrorCodes.PackageMalformed, "The package version is empty.");
        }

        if (!NuGetVersion.TryParse(version.Trim(), out var parsed))
        {
            return Result<string>.Failure(
                ErrorCodes.PackageMalformed,
                $"Package version '{version}' is not a valid NuGet version.");
        }

        return Result<string>.Success(parsed.ToNormalizedString());
    }

    /// <summary>True when <paramref name="normalized"/> is the normalized form of <paramref name="version"/>.</summary>
    public static bool IsNormalizedForm(string? version, string? normalized)
    {
        var expected = Normalize(version);
        return expected.IsSuccess &&
            string.Equals(expected.Value, normalized?.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
