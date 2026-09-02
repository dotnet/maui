namespace DotNet.Release;

/// <summary>Package selection filters supplied to <c>release stage</c>.</summary>
internal sealed record StageOptions
{
    public IReadOnlyList<string> Include { get; init; } = [];

    public IReadOnlyList<string> Exclude { get; init; } = [];
}

/// <summary>
/// Applies package selection, workload splitting, identity validation, and ordering to create
/// a <see cref="ReleaseManifest"/>.
/// </summary>
/// <remarks>The caller reads and hashes package files before invoking this type.</remarks>
internal static class ReleaseManifestBuilder
{
    /// <summary>Repository-neutral artifact names consumed by the shared pipeline.</summary>
    public const string PacksArtifactName = "ReleasePacks";
    public const string ManifestsArtifactName = "ReleaseManifests";
    public const string PackagesArtifactName = "ReleasePackages";

    public static ReleaseManifest Build(ReleaseSource source, ReleasePolicy policy, IReadOnlyList<ReleasePackage> drop, StageOptions options,
        DateTimeOffset createdUtc, string toolVersion)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(drop);
        ArgumentNullException.ThrowIfNull(options);

        if (drop.Count == 0)
        {
            throw new DotNetReleaseException("The gathered drop contains no shipping NuGet packages.");
        }

        var malformed = ValidateIdentities(drop);
        if (malformed.Count > 0)
        {
            throw new DotNetReleaseException(malformed);
        }

        return source.Workload ? BuildWorkloadManifest(source, policy, drop, options, createdUtc, toolVersion)
            : BuildNonWorkloadManifest(source, drop, options, createdUtc, toolVersion);
    }

    private static ReleaseManifest BuildWorkloadManifest(ReleaseSource source, ReleasePolicy policy, IReadOnlyList<ReleasePackage> drop, StageOptions options,
        DateTimeOffset createdUtc, string toolVersion)
    {
        // Include filters select packs. Manifests remain selected unless explicitly excluded,
        // because a valid workload release requires its manifests independently of pack
        // selection.
        var packs = drop
            .Where(p => !PackageClassifier.IsWorkloadManifest(p.FileName))
            .Where(p => IsSelected(p.FileName, workload: true, options)).ToList();

        var manifests = drop
            .Where(p => PackageClassifier.IsWorkloadManifest(p.FileName))
            .Where(p => IsSelected(p.FileName, workload: true, options)).ToList();

        if (packs.Count == 0 || manifests.Count == 0)
        {
            throw new DotNetReleaseException($"Package filtering must select both workload packs and manifests, but selected " +
                $"{packs.Count} pack(s) and {manifests.Count} manifest(s).");
        }

        var duplicateIdentities = GetDuplicateIdentities([.. packs, .. manifests]);
        if (duplicateIdentities.Count > 0)
        {
            throw new DotNetReleaseException(
                $"Workload release contains duplicate package identities: {string.Join(", ", duplicateIdentities)}.");
        }

        var band = PackageClassifier.GetWorkloadBand([.. manifests.Select(m => m.FileName)]);
        var workloadSet = policy.GetWorkloadSet(band);
        var errors = new List<string>();
        var packSet = BuildSet("Workload packs", order: 0, PacksArtifactName, packs, errors);
        var manifestSet = BuildSet("Workload manifests", order: 1, ManifestsArtifactName, manifests, errors);

        if (errors.Count > 0)
        {
            throw new DotNetReleaseException(errors);
        }

        return new ReleaseManifest
        {
            ToolVersion = toolVersion,
            CreatedUtc = createdUtc.ToUniversalTime(),
            Source = source,
            WorkloadSet = workloadSet,
            Sets = [packSet, manifestSet],
        };
    }

    private static ReleaseManifest BuildNonWorkloadManifest(
        ReleaseSource source,
        IReadOnlyList<ReleasePackage> drop,
        StageOptions options,
        DateTimeOffset createdUtc,
        string toolVersion)
    {
        var selected = drop
            .Where(p => IsSelected(p.FileName, workload: false, options)).ToList();

        // A workload manifest requires the dedicated pack-before-manifest stage topology.
        // Reject it from a non-workload release instead of publishing it in the single set.
        var manifests = selected.Where(p => PackageClassifier.IsWorkloadManifest(p.FileName)).ToList();
        if (manifests.Count > 0)
        {
            throw new DotNetReleaseException($"A non-workload release cannot contain workload manifest packages: " +
                $"{string.Join(", ", manifests.Select(m => m.FileName).Order(StringComparer.Ordinal))}.");
        }

        if (selected.Count == 0)
        {
            throw new DotNetReleaseException("Package filtering selected no non-workload NuGet packages.");
        }

        var errors = new List<string>();
        var set = BuildSet("NuGet packages", order: 0, PackagesArtifactName, selected, errors);
        if (errors.Count > 0)
        {
            throw new DotNetReleaseException(errors);
        }

        return new ReleaseManifest
        {
            ToolVersion = toolVersion,
            CreatedUtc = createdUtc.ToUniversalTime(),
            Source = source,
            WorkloadSet = null,
            Sets = [set],
        };
    }

    private static ReleasePackageSet BuildSet(string name, int order, string artifactName, IReadOnlyList<ReleasePackage> packages, List<string> errors)
    {
        var duplicateFileNames = packages
            .GroupBy(p => p.FileName, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key).Order(StringComparer.Ordinal).ToList();

        if (duplicateFileNames.Count > 0)
        {
            errors.Add($"{name} contains duplicate package file names: {string.Join(", ", duplicateFileNames)}.");
        }

        // Two different files can still carry the same identity, which NuGet.org would
        // reject mid-publish, leaving the release half-applied.
        var duplicateIdentities = GetDuplicateIdentities(packages);

        if (duplicateIdentities.Count > 0)
        {
            errors.Add($"{name} contains duplicate package identities: {string.Join(", ", duplicateIdentities)}.");
        }

        return new ReleasePackageSet
        {
            Name = name,
            Order = order,
            ArtifactName = artifactName,
            Packages = [.. packages
                .OrderBy(p => p.Id, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.NormalizedVersion, StringComparer.OrdinalIgnoreCase)],
        };
    }

    private static List<string> GetDuplicateIdentities(IReadOnlyList<ReleasePackage> packages) =>
        packages
            .GroupBy(p => $"{p.Id}/{p.NormalizedVersion}", StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key).Order(StringComparer.Ordinal).ToList();

    private static List<string> ValidateIdentities(IReadOnlyList<ReleasePackage> drop)
    {
        var errors = new List<string>();

        foreach (var package in drop)
        {
            if (string.IsNullOrWhiteSpace(package.Id) || string.IsNullOrWhiteSpace(package.Version) || string.IsNullOrWhiteSpace(package.NormalizedVersion))
            {
                errors.Add($"Package '{package.FileName}' has no ID or version.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(package.Sha256))
            {
                errors.Add($"Package '{package.FileName}' was staged without a content hash.");
            }

            // The availability query NuGet.org is asked is built from the normalized version,
            // so a reader that got it wrong would report published packages as missing.
            if (!PackageVersions.IsNormalizedForm(package.Version, package.NormalizedVersion))
            {
                errors.Add(
                    $"Package '{package.FileName}' declares version '{package.Version}' but normalized version '{package.NormalizedVersion}'.");
            }

            // A file whose name disagrees with its nuspec identity would be pushed under one
            // name and indexed under another, so the mismatch is rejected rather than resolved.
            if (!package.FileName.StartsWith($"{package.Id}.", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"Package file '{package.FileName}' does not start with its ID '{package.Id}'.");
            }

            if (Path.GetFileName(package.FileName) != package.FileName)
            {
                errors.Add($"Package file name '{package.FileName}' must not contain a directory.");
            }
        }

        return errors;
    }

    internal static bool IsSelected(string fileName, bool workload, StageOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var include = workload && PackageClassifier.IsWorkloadManifest(fileName) ? [] : options.Include;
        return PackageGlob.IsSelected(fileName, include, options.Exclude);
    }
}
