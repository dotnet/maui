namespace DotNet.Release;

/// <summary>Package selection filters supplied to <c>release stage</c>.</summary>
internal sealed record StageOptions
{
    public IReadOnlyList<string> Include { get; init; } = [];

    public IReadOnlyList<string> Exclude { get; init; } = [];
}

/// <summary>
/// Turns a gathered drop into a <see cref="ReleasePlan"/>. Pure: the caller has already read
/// the nupkgs and hashed them.
/// </summary>
internal static class StagePlanner
{
    /// <summary>Repository-neutral artifact names consumed by the shared pipeline.</summary>
    public const string PacksArtifactName = "ReleasePacks";
    public const string ManifestsArtifactName = "ReleaseManifests";
    public const string PackagesArtifactName = "ReleasePackages";

    public static Result<ReleasePlan> Create(
        ResolvedRelease source,
        ReleasePolicy policy,
        IReadOnlyList<DropPackage> drop,
        StageOptions options,
        DateTimeOffset createdUtc,
        string toolVersion)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(drop);
        ArgumentNullException.ThrowIfNull(options);

        if (drop.Count == 0)
        {
            return Result<ReleasePlan>.Failure(
                ErrorCodes.PackageSetEmpty,
                "The gathered drop contains no shipping NuGet packages.");
        }

        var malformed = ValidateIdentities(drop);
        if (malformed.Count > 0)
        {
            return Result<ReleasePlan>.Failure(malformed);
        }

        return source.Workload
            ? CreateWorkloadPlan(source, policy, drop, options, createdUtc, toolVersion)
            : CreateNonWorkloadPlan(source, drop, options, createdUtc, toolVersion);
    }

    private static Result<ReleasePlan> CreateWorkloadPlan(
        ResolvedRelease source,
        ReleasePolicy policy,
        IReadOnlyList<DropPackage> drop,
        StageOptions options,
        DateTimeOffset createdUtc,
        string toolVersion)
    {
        // Include filters select packs. Manifests remain selected unless explicitly excluded,
        // because a valid workload release requires its manifests independently of pack
        // selection.
        var packs = drop
            .Where(p => !PackageClassifier.IsWorkloadManifest(p.FileName))
            .Where(p => PackageGlob.IsSelected(p.FileName, options.Include, options.Exclude))
            .ToList();

        var manifests = drop
            .Where(p => PackageClassifier.IsWorkloadManifest(p.FileName))
            .Where(p => PackageGlob.IsSelected(p.FileName, include: [], options.Exclude))
            .ToList();

        if (packs.Count == 0 || manifests.Count == 0)
        {
            return Result<ReleasePlan>.Failure(
                ErrorCodes.PackageSetEmpty,
                $"Package filtering must select both workload packs and manifests, but selected " +
                $"{packs.Count} pack(s) and {manifests.Count} manifest(s).");
        }

        var band = PackageClassifier.GetWorkloadBand([.. manifests.Select(m => m.FileName)]);
        if (band.IsFailure)
        {
            return band.ToFailure<ReleasePlan>();
        }

        var workloadSet = policy.GetWorkloadSet(band.Value);
        if (workloadSet.IsFailure)
        {
            return workloadSet.ToFailure<ReleasePlan>();
        }

        var packSet = BuildSet("Workload packs", order: 0, PacksArtifactName, packs);
        var manifestSet = BuildSet("Workload manifests", order: 1, ManifestsArtifactName, manifests);

        if (packSet.IsFailure || manifestSet.IsFailure)
        {
            return Result<ReleasePlan>.Failure([.. packSet.Errors, .. manifestSet.Errors]);
        }

        return Result<ReleasePlan>.Success(new ReleasePlan
        {
            ToolVersion = toolVersion,
            CreatedUtc = createdUtc.ToUniversalTime(),
            Source = source,
            WorkloadSet = new WorkloadSetTarget(
                workloadSet.Value.Band,
                workloadSet.Value.Channel,
                workloadSet.Value.Feed),
            Sets = [packSet.Value, manifestSet.Value],
        });
    }

    private static Result<ReleasePlan> CreateNonWorkloadPlan(
        ResolvedRelease source,
        IReadOnlyList<DropPackage> drop,
        StageOptions options,
        DateTimeOffset createdUtc,
        string toolVersion)
    {
        // A workload manifest requires the dedicated pack-before-manifest stage topology.
        // Reject it from a non-workload release instead of publishing it in the single set.
        var manifests = drop.Where(p => PackageClassifier.IsWorkloadManifest(p.FileName)).ToList();
        if (manifests.Count > 0)
        {
            return Result<ReleasePlan>.Failure(
                ErrorCodes.ManifestInNonWorkload,
                $"A non-workload release cannot contain workload manifest packages: " +
                $"{string.Join(", ", manifests.Select(m => m.FileName).Order(StringComparer.Ordinal))}.");
        }

        var selected = drop
            .Where(p => PackageGlob.IsSelected(p.FileName, options.Include, options.Exclude))
            .ToList();

        if (selected.Count == 0)
        {
            return Result<ReleasePlan>.Failure(
                ErrorCodes.PackageSetEmpty,
                "Package filtering selected no non-workload NuGet packages.");
        }

        var set = BuildSet("NuGet packages", order: 0, PackagesArtifactName, selected);
        if (set.IsFailure)
        {
            return set.ToFailure<ReleasePlan>();
        }

        return Result<ReleasePlan>.Success(new ReleasePlan
        {
            ToolVersion = toolVersion,
            CreatedUtc = createdUtc.ToUniversalTime(),
            Source = source,
            WorkloadSet = null,
            Sets = [set.Value],
        });
    }

    private static Result<ReleasePackageSet> BuildSet(
        string name,
        int order,
        string artifactName,
        IReadOnlyList<DropPackage> packages)
    {
        var errors = new List<ReleaseError>();

        var duplicateFileNames = packages
            .GroupBy(p => p.FileName, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .Order(StringComparer.Ordinal)
            .ToList();

        if (duplicateFileNames.Count > 0)
        {
            errors.Add(new ReleaseError(
                ErrorCodes.PackageDuplicateFileName,
                $"{name} contains duplicate package file names: {string.Join(", ", duplicateFileNames)}."));
        }

        // Two different files can still carry the same identity, which NuGet.org would
        // reject mid-publish, leaving the release half-applied.
        var duplicateIdentities = packages
            .GroupBy(p => $"{p.Id}/{p.Version}", StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .Order(StringComparer.Ordinal)
            .ToList();

        if (duplicateIdentities.Count > 0)
        {
            errors.Add(new ReleaseError(
                ErrorCodes.PackageDuplicateIdentity,
                $"{name} contains duplicate package identities: {string.Join(", ", duplicateIdentities)}."));
        }

        if (errors.Count > 0)
        {
            return Result<ReleasePackageSet>.Failure(errors);
        }

        return Result<ReleasePackageSet>.Success(new ReleasePackageSet
        {
            Name = name,
            Order = order,
            ArtifactName = artifactName,
            Packages = [.. packages
                .OrderBy(p => p.Id, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.Version, StringComparer.OrdinalIgnoreCase)
                .Select(p => new PlannedPackage
                {
                    Id = p.Id,
                    Version = p.Version,
                    NormalizedVersion = p.NormalizedVersion,
                    FileName = p.FileName,
                    Sha256 = p.Sha256,
                })],
        });
    }

    private static List<ReleaseError> ValidateIdentities(IReadOnlyList<DropPackage> drop)
    {
        var errors = new List<ReleaseError>();

        foreach (var package in drop)
        {
            if (string.IsNullOrWhiteSpace(package.Id) ||
                string.IsNullOrWhiteSpace(package.Version) ||
                string.IsNullOrWhiteSpace(package.NormalizedVersion))
            {
                errors.Add(new ReleaseError(
                    ErrorCodes.PackageMalformed,
                    $"Package '{package.FileName}' has no ID or version."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(package.Sha256))
            {
                errors.Add(new ReleaseError(
                    ErrorCodes.PackageMalformed,
                    $"Package '{package.FileName}' was staged without a content hash."));
            }

            // The availability query NuGet.org is asked is built from the normalized version,
            // so a reader that got it wrong would report published packages as missing.
            if (!PackageVersions.IsNormalizedForm(package.Version, package.NormalizedVersion))
            {
                errors.Add(new ReleaseError(
                    ErrorCodes.PackageMalformed,
                    $"Package '{package.FileName}' declares version '{package.Version}' but " +
                    $"normalized version '{package.NormalizedVersion}'."));
            }

            // A file whose name disagrees with its nuspec identity would be pushed under one
            // name and indexed under another, so the mismatch is rejected rather than resolved.
            if (!package.FileName.StartsWith($"{package.Id}.", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(new ReleaseError(
                    ErrorCodes.PackageMalformed,
                    $"Package file '{package.FileName}' does not start with its ID '{package.Id}'."));
            }

            if (Path.GetFileName(package.FileName) != package.FileName)
            {
                errors.Add(new ReleaseError(
                    ErrorCodes.PackageMalformed,
                    $"Package file name '{package.FileName}' must not contain a directory."));
            }
        }

        return errors;
    }
}
