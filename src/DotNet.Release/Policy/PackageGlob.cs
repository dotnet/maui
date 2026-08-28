using System.IO.Enumeration;
using System.Text.RegularExpressions;

namespace DotNet.Release;

/// <summary>
/// File-name glob matching for package selection filters.
/// </summary>
/// <remarks>
/// Supports <c>*</c> and <c>?</c> only, case-insensitively, via
/// <see cref="FileSystemName.MatchesSimpleExpression"/>.
/// <para>
/// This is a deliberate narrowing of PowerShell's <c>-like</c>, which also supports
/// <c>[a-z]</c> character classes. No existing release filter uses one, and a character
/// class in a release package filter is a footgun. See docs/design.md section 12.
/// </para>
/// </remarks>
public static class PackageGlob
{
    /// <summary>
    /// Parses a semicolon-separated filter list. Null or whitespace means no filters.
    /// </summary>
    public static IReadOnlyList<string> ParseList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }

    public static bool IsMatch(string fileName, string pattern) =>
        FileSystemName.MatchesSimpleExpression(pattern, fileName, ignoreCase: true);

    public static bool IsAnyMatch(string fileName, IReadOnlyList<string> patterns)
    {
        for (var i = 0; i < patterns.Count; i++)
        {
            if (IsMatch(fileName, patterns[i]))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Applies include/exclude semantics: an empty include list includes everything, and
    /// exclude always wins.
    /// </summary>
    public static bool IsSelected(string fileName, IReadOnlyList<string> include, IReadOnlyList<string> exclude) =>
        (include.Count == 0 || IsAnyMatch(fileName, include)) &&
        !(exclude.Count > 0 && IsAnyMatch(fileName, exclude));
}

/// <summary>Distinguishes workload manifests from workload packs, and derives the .NET band.</summary>
public static partial class PackageClassifier
{
    /// <summary>
    /// True when the package is a workload manifest.
    /// </summary>
    /// <remarks>
    /// Workload artifacts encode manifest classification in the package filename; this
    /// classification selects the separately gated manifest stage.
    /// </remarks>
    public static bool IsWorkloadManifest(string fileName) =>
        fileName.Contains("Manifest", StringComparison.OrdinalIgnoreCase) &&
        fileName.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Derives the single .NET band shared by every manifest, from the
    /// <c>.Manifest-&lt;major&gt;.</c> segment of the file name.
    /// </summary>
    public static Result<int> GetWorkloadBand(IReadOnlyList<string> manifestFileNames)
    {
        ArgumentNullException.ThrowIfNull(manifestFileNames);

        if (manifestFileNames.Count == 0)
        {
            return Result<int>.Failure(
                ErrorCodes.WorkloadBandUnresolved,
                "A workload release must contain at least one manifest to derive the .NET band from.");
        }

        var bands = new SortedSet<int>();
        foreach (var fileName in manifestFileNames)
        {
            var match = BandPattern().Match(fileName);
            if (!match.Success)
            {
                return Result<int>.Failure(
                    ErrorCodes.WorkloadBandUnresolved,
                    $"Could not determine the workload band from manifest '{fileName}'; " +
                    "expected a '.Manifest-<major>.' segment.");
            }

            bands.Add(int.Parse(match.Groups["major"].ValueSpan));
        }

        return bands.Count == 1
            ? Result<int>.Success(bands.Min)
            : Result<int>.Failure(
                ErrorCodes.WorkloadBandAmbiguous,
                $"Expected one workload major version, found: {string.Join(", ", bands)}.");
    }

    [GeneratedRegex(@"\.Manifest-(?<major>\d+)\.", RegexOptions.IgnoreCase)]
    private static partial Regex BandPattern();
}
