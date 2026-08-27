using NuGet.Versioning;

namespace DotNet.Release.Core;

/// <summary>
/// Package version normalization.
/// </summary>
/// <remarks>
/// <para>
/// This replaces the current pipeline's normalized-version derivation, which takes a
/// substring of the file name after the package ID:
/// </para>
/// <code>normalizedVersion = package.BaseName.Substring(("$id.").Length)</code>
/// <para>
/// That is <b>not</b> broken today. NuGet feeds serve packages under normalized file names,
/// so the file name already is the normalized version and the substring returns the right
/// answer. It is correct by coincidence rather than by construction: it depends on a
/// property of the feeds upstream that nothing in the release checks.
/// </para>
/// <para>
/// The reason it is worth replacing is the failure mode when that coincidence does not
/// hold. The NuGet.org availability query is built from the normalized version, so a wrong
/// value reports a published package as missing and verification can never succeed — with
/// nothing in the log pointing at the cause. <see cref="IsNormalizedForm"/> exists so
/// staging can turn that silent wrong answer into a loud one.
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
