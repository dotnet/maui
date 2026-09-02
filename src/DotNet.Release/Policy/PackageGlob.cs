using System.IO.Enumeration;

namespace DotNet.Release;

/// <summary>
/// File-name glob matching for package selection filters.
/// </summary>
/// <remarks>
/// Supports <c>*</c> and <c>?</c> only, case-insensitively, via
/// <see cref="FileSystemName.MatchesSimpleExpression"/>.
/// <para>
/// This is a deliberate narrowing of PowerShell's <c>-like</c>, which also supports
/// <c>[a-z]</c> character classes. Release selection supports only simple expressions so
/// patterns remain predictable.
/// </para>
/// </remarks>
internal static class PackageGlob
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
        (include.Count == 0 || IsAnyMatch(fileName, include)) && !(exclude.Count > 0 && IsAnyMatch(fileName, exclude));
}
