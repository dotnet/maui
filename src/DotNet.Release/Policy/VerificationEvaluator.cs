namespace DotNet.Release;

/// <summary>Evaluates whether every planned package is indexed on NuGet.org.</summary>
internal static class VerificationEvaluator
{
    /// <summary>Returns packages that are not confirmed resolvable on NuGet.org.</summary>
    public static IReadOnlyList<PlannedPackage> GetMissing(
        IEnumerable<PlannedPackage> packages,
        IReadOnlyDictionary<string, bool> availability)
    {
        ArgumentNullException.ThrowIfNull(packages);
        ArgumentNullException.ThrowIfNull(availability);

        return
        [
            .. packages.Where(package =>
                !availability.TryGetValue(package.IdentityKey, out var isPublished) ||
                !isPublished)
        ];
    }

    public static bool IsComplete(
        IEnumerable<PlannedPackage> packages,
        IReadOnlyDictionary<string, bool> availability) =>
        GetMissing(packages, availability).Count == 0;

    public static string DescribeMissing(IReadOnlyList<PlannedPackage> missing) =>
        "The following packages are not available from NuGet.org: " +
        string.Join(
            ", ",
            missing
                .Select(package => $"{package.Id} {package.Version}")
                .Order(StringComparer.Ordinal));
}
