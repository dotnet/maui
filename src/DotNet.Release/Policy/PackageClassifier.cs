using System.Text.RegularExpressions;

namespace DotNet.Release;

/// <summary>Distinguishes workload manifests from packs and derives the .NET band.</summary>
internal static partial class PackageClassifier
{
    /// <summary>True when the package filename identifies a workload manifest.</summary>
    public static bool IsWorkloadManifest(string fileName) =>
        fileName.Contains("Manifest", StringComparison.OrdinalIgnoreCase) &&
        fileName.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Derives the single .NET band shared by every manifest from the
    /// <c>.Manifest-&lt;major&gt;.</c> filename segment.
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
