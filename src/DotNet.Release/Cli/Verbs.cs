
namespace DotNet.Release;

/// <summary>
/// The verb bodies, expressed as pure-ish functions over injected collaborators so the whole
/// CLI is testable without a network, a BAR account, or a real feed.
/// </summary>
internal static class Verbs
{
    public const string PlanFileName = "plan.json";
    public const string ReleasePlanFileName = "release-plan.json";
    public const string FilterReportFileName = "release-filter.json";

    /// <summary>Subdirectory holding the tool the publish job executes under `checkout: none`.</summary>
    public const string ToolDirectoryName = "_tool";

    // ---- release plan ----

    /// <summary>
    /// Resolves and verifies the BAR build, then writes <c>plan.json</c>.
    /// </summary>
    /// <remarks>
    /// Emits the <c>BarId</c> pipeline variable because the <c>darc gather-drop</c> step that
    /// follows needs the ID this step discovered.
    /// </remarks>
    public static async Task<int> PlanAsync(
        IReleaseConsole console,
        IReleaseFileSystem fs,
        IBuildRegistry registry,
        string policyJson,
        string repository,
        string commit,
        int? barBuildId,
        bool? expectWorkload,
        string outputDirectory,
        DateTimeOffset now,
        string toolVersion,
        CancellationToken cancellationToken)
    {
        var policy = ReleasePolicy.Parse(policyJson);
        if (policy.IsFailure)
        {
            return ConsoleReporting.Fail(console, policy.Errors);
        }

        var repositoryId = RepositoryId.Parse(repository);
        if (repositoryId.IsFailure)
        {
            return ConsoleReporting.Fail(console, repositoryId.Errors);
        }

        var repositoryPolicy = policy.Value.GetRepository(repositoryId.Value);
        if (repositoryPolicy.IsFailure)
        {
            return ConsoleReporting.Fail(console, repositoryPolicy.Errors);
        }

        // The pipeline chose its stage structure from its own repository list. If that
        // disagrees with the checked-in policy, stop before anything is gathered.
        var classification = ReleasePolicy.VerifyWorkloadClassification(repositoryPolicy.Value, expectWorkload);
        if (classification.IsFailure)
        {
            return ConsoleReporting.Fail(console, classification.Errors);
        }

        var request = new ReleaseRequest(repositoryId.Value, commit, barBuildId);

        var candidates = barBuildId is { } id
            ? await registry.GetBuildAsync(id, cancellationToken).ConfigureAwait(false)
            : await registry.GetBuildsAsync(repositoryId.Value, commit, cancellationToken).ConfigureAwait(false);

        var resolved = BuildResolver.Resolve(request, repositoryPolicy.Value, candidates, now, toolVersion);
        if (resolved.IsFailure)
        {
            return ConsoleReporting.Fail(console, resolved.Errors);
        }

        fs.CreateDirectory(outputDirectory);
        var planPath = Path.Combine(outputDirectory, PlanFileName);
        await fs.WriteAllTextAsync(planPath, ReleasePlanSerializer.Serialize(resolved.Value), cancellationToken)
            .ConfigureAwait(false);

        console.WriteLine($"Resolved BAR build {resolved.Value.BarBuildId} for {resolved.Value.Repository} at {resolved.Value.Commit}.");
        console.WriteLine($"Identity established from: {resolved.Value.RepositoryOrigin}.");
        console.WriteLine($"Wrote {planPath}.");

        // The only pipeline variable this tool emits from `plan`.
        console.WriteLine(AzurePipelineCommand.SetBarId(resolved.Value.BarBuildId));

        return ExitCodes.Success;
    }

    // ---- release stage ----

    /// <summary>
    /// Reads the gathered drop, validates it, stages the files, and writes the final
    /// <c>release-plan.json</c>.
    /// </summary>
    public static async Task<int> StageAsync(
        IReleaseConsole console,
        IReleaseFileSystem fs,
        IPackageIdentityReader reader,
        string policyJson,
        string resolvedPlanJson,
        string dropDirectory,
        string outputDirectory,
        StageOptions options,
        ToolReference tool,
        string toolFilePath,
        DateTimeOffset now,
        string toolVersion,
        CancellationToken cancellationToken)
    {
        var policy = ReleasePolicy.Parse(policyJson);
        if (policy.IsFailure)
        {
            return ConsoleReporting.Fail(console, policy.Errors);
        }

        var resolved = ReleasePlanSerializer.DeserializeResolved(resolvedPlanJson);
        if (resolved.IsFailure)
        {
            return ConsoleReporting.Fail(console, resolved.Errors);
        }

        var packageFiles = FindShippingPackages(fs, dropDirectory);
        if (packageFiles.Count == 0)
        {
            return ConsoleReporting.Fail(console, [new ReleaseError(
                ErrorCodes.PackageSetEmpty,
                $"No shipping .nupkg files were found under '{dropDirectory}'. " +
                "Check that `darc gather-drop` succeeded and produced 'shipping/packages'.")]);
        }

        var drop = new List<DropPackage>(packageFiles.Count);
        var readErrors = new List<ReleaseError>();

        foreach (var file in packageFiles)
        {
            var read = await reader.ReadAsync(file, cancellationToken).ConfigureAwait(false);
            if (read.IsFailure)
            {
                readErrors.AddRange(read.Errors);
                continue;
            }

            drop.Add(read.Value);
        }

        if (readErrors.Count > 0)
        {
            return ConsoleReporting.Fail(console, readErrors);
        }

        var plan = StagePlanner.Create(resolved.Value, policy.Value, drop, options, tool, now, toolVersion);
        if (plan.IsFailure)
        {
            return ConsoleReporting.Fail(console, plan.Errors);
        }

        // The drop is enumerated recursively, so two same-named packages can exist in
        // different subdirectories. StagePlanner rejects duplicates within a *selected* set,
        // but this lookup spans everything discovered, so a filter that removed one of the
        // pair would otherwise reach ToDictionary and throw a raw ArgumentException.
        var sourceByFileName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in packageFiles)
        {
            var name = Path.GetFileName(file);
            if (!sourceByFileName.TryAdd(name, file))
            {
                return ConsoleReporting.Fail(console, [new ReleaseError(
                    ErrorCodes.PackageDuplicateFileName,
                    $"The gathered drop contains more than one '{name}': " +
                    $"'{sourceByFileName[name]}' and '{file}'.")]);
            }
        }

        fs.CreateDirectory(outputDirectory);
        var planJson = ReleasePlanSerializer.Serialize(plan.Value);

        foreach (var set in plan.Value.Sets.OrderBy(s => s.Order))
        {
            var setDirectory = Path.Combine(outputDirectory, set.ArtifactName);
            fs.CreateDirectory(setDirectory);

            foreach (var package in set.Packages)
            {
                fs.CopyFile(sourceByFileName[package.FileName], Path.Combine(setDirectory, package.FileName));
            }

            // Each set directory is published as its own pipeline artifact and consumed by a
            // job running `checkout: none`, so the artifact has to be self-contained. The
            // plan bytes are identical in every set, so one hash still pins them all.
            await fs.WriteAllTextAsync(Path.Combine(setDirectory, ReleasePlanFileName), planJson, cancellationToken)
                .ConfigureAwait(false);

            // Makes the directory declare which set it is, so a publish stage wired to the
            // wrong set is caught immediately instead of surfacing as missing files.
            await fs.WriteAllTextAsync(
                Path.Combine(setDirectory, ReleaseSetMarker.FileName),
                ReleasePlanSerializer.Serialize(ReleaseSetMarker.For(set, resolved.Value)),
                cancellationToken).ConfigureAwait(false);

            // The publish job runs `checkout: none`, so the tool it executes can only come
            // from this artifact. Its hash is recorded in the plan, so verifying the plan
            // transitively pins the binary that is about to run.
            var toolDirectory = Path.Combine(setDirectory, ToolDirectoryName);
            fs.CreateDirectory(toolDirectory);
            fs.CopyFile(toolFilePath, Path.Combine(toolDirectory, tool.FileName));
        }

        var planPath = Path.Combine(outputDirectory, ReleasePlanFileName);
        await fs.WriteAllTextAsync(planPath, planJson, cancellationToken).ConfigureAwait(false);

        // Self-check: the directories just written must satisfy the same invariant the
        // publish job will enforce. Cheap here, and catches a staging bug before the
        // artifact is published rather than inside a production release job.
        foreach (var set in plan.Value.Sets)
        {
            var staged = StagedSetIntegrity.ValidateStaged(
                set, ReadStagedHashes(fs, Path.Combine(outputDirectory, set.ArtifactName)));

            if (staged.IsFailure)
            {
                return ConsoleReporting.Fail(console, staged.Errors);
            }
        }

        ConsoleReporting.WriteSummary(console, plan.Value);
        console.WriteLine(string.Empty);
        console.WriteLine($"Wrote {planPath}.");
        console.WriteLine($"Release plan SHA-256: {ReleasePlanSerializer.ComputeHash(planJson)}");

        return ExitCodes.Success;
    }

    /// <summary>
    /// Finds the shipping packages a <c>darc gather-drop</c> produced.
    /// </summary>
    /// <remarks>
    /// Only <c>shipping/packages</c>. Non-shipping packages are build inputs, and symbol
    /// blobs are excluded upstream by the gather's asset filter.
    /// </remarks>
    internal static List<string> FindShippingPackages(IReleaseFileSystem fs, string dropDirectory)
    {
        var shipping = Path.Combine(dropDirectory, "shipping", "packages");
        var root = fs.DirectoryExists(shipping) ? shipping : dropDirectory;

        return [.. fs.EnumerateFiles(root, "*.nupkg", recursive: true).Order(StringComparer.Ordinal)];
    }

    // ---- release filter ----

    /// <summary>
    /// Removes already-published packages from the staging directory so the 1ES push glob
    /// picks up only what still needs publishing.
    /// </summary>
    public static async Task<int> FilterAsync(
        IReleaseConsole console,
        IReleaseFileSystem fs,
        IPackageAvailabilityProbe probe,
        string planJson,
        string stageDirectory,
        IReadOnlyList<string> skipPatterns,
        string? expectedPlanHash,
        string? setName,
        CancellationToken cancellationToken)
    {
        // An empty hash is a misconfiguration, not permission to skip the check that pins
        // the artifact this job is about to publish.
        var plan = ReleasePlanSerializer.VerifyAndDeserialize(planJson, expectedPlanHash ?? string.Empty);
        if (plan.IsFailure)
        {
            return ConsoleReporting.Fail(console, plan.Errors);
        }

        var sets = SelectSets(plan.Value, setName);
        if (sets.IsFailure)
        {
            return ConsoleReporting.Fail(console, sets.Errors);
        }

        var pending = 0;

        foreach (var set in sets.Value)
        {
            var setDirectory = Path.Combine(stageDirectory, set.ArtifactName);

            var marker = ValidateSetMarker(fs, setDirectory, set, plan.Value.Source, setName);
            if (marker.IsFailure)
            {
                return ConsoleReporting.Fail(console, marker.Errors);
            }

            var availability = await probe.GetAvailabilityAsync(set.Packages, cancellationToken).ConfigureAwait(false);

            var report = FilterPlanner.Plan(set, skipPatterns, availability);
            if (report.IsFailure)
            {
                return ConsoleReporting.Fail(console, report.Errors);
            }

            foreach (var fileName in report.Value.FilesToRemove)
            {
                var path = Path.Combine(setDirectory, fileName);
                if (fs.FileExists(path))
                {
                    fs.DeleteFile(path);
                }

                console.WriteLine($"Withheld {fileName}.");
            }

            // The directory is now the input to a production push, so it is checked against
            // the plan before anything is handed to 1ES: a missing pending file, a tampered
            // file, or an unlisted extra file all fail here.
            var integrity = StagedSetIntegrity.ValidateFiltered(set, ReadStagedHashes(fs, setDirectory), report.Value);
            if (integrity.IsFailure)
            {
                return ConsoleReporting.Fail(console, integrity.Errors);
            }

            var reportPath = Path.Combine(setDirectory, FilterReportFileName);
            await fs.WriteAllTextAsync(reportPath, ReleasePlanSerializer.Serialize(report.Value), cancellationToken)
                .ConfigureAwait(false);

            console.WriteLine($"{set.Name}: {report.Value.PendingCount} of {set.Packages.Count} remain to publish.");
            pending += report.Value.PendingCount;
        }

        console.WriteLine(AzurePipelineCommand.SetPackagesToPublish(pending > 0));

        return ExitCodes.Success;
    }

    /// <summary>
    /// Hashes the staged packages so they can be compared with the plan.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>The <c>*.nupkg</c> filter is the unexpected-file rule's scope, not an
    /// optimisation.</b> Only packages are observed, so the companion files that share the
    /// directory — <c>release-plan.json</c> and <c>release-set.json</c> — are structurally
    /// incapable of being reported as unexpected. Adding a further companion file later
    /// requires no change here and no allow-list.
    /// </para>
    /// <para>
    /// This matches how the pipeline being replaced scopes the same rule, and how
    /// <c>1ES.PublishNuget@1</c> scopes its own <c>packagesToPush</c> glob: in build 3059242
    /// the staged directory held 41 nupkgs plus a JSON manifest plus a PowerShell script,
    /// and exactly 41 packages were pushed.
    /// </para>
    /// </remarks>
    internal static Dictionary<string, string> ReadStagedHashes(IReleaseFileSystem fs, string directory)
    {
        var hashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!fs.DirectoryExists(directory))
        {
            return hashes;
        }

        // AllDirectories, deliberately. The publish glob is `*.nupkg`, which does not cross a
        // directory separator, so a nested package would be invisible to both the push and
        // this check. Enumerating recursively means a nested package is reported as
        // unexpected rather than silently ignored, and it decouples this rule from the exact
        // glob written in the YAML.
        foreach (var file in fs.EnumerateFiles(directory, "*.nupkg", recursive: true))
        {
            hashes[Path.GetRelativePath(directory, file)] = Convert.ToHexStringLower(
                System.Security.Cryptography.SHA256.HashData(
                    fs.ReadAllBytesAsync(file, CancellationToken.None).GetAwaiter().GetResult()));
        }

        return hashes;
    }

    /// <summary>
    /// Confirms a staged directory declares itself to be the set that was requested.
    /// </summary>
    /// <remarks>
    /// Only enforced when a set was named explicitly. That is the case where the caller has
    /// asserted something that can be wrong, and it is the case the pipeline always uses.
    /// </remarks>
    internal static Result<bool> ValidateSetMarker(
        IReleaseFileSystem fs,
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
        if (!fs.FileExists(markerPath))
        {
            return ReleaseSetMarker.Validate(null, set, source);
        }

        var marker = ReleasePlanSerializer.DeserializeSetMarker(
            fs.ReadAllTextAsync(markerPath, CancellationToken.None).GetAwaiter().GetResult());

        return marker.IsFailure
            ? marker.ToFailure<bool>()
            : ReleaseSetMarker.Validate(marker.Value, set, source);
    }

    /// <summary>
    /// Selects the sets a publish stage operates on.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This scoping is load-bearing for workload releases. Packs and manifests are published
    /// by two separate stages, each of which downloads only its own artifact — so a stage
    /// that operated on every set in the plan would look for the other stage's files in a
    /// directory that does not exist, and would wait for packages that have not been pushed
    /// yet. A single-set release works either way, which is exactly why the mistake is easy
    /// to miss.
    /// </para>
    /// <para>
    /// An unknown set name fails closed rather than silently selecting nothing: a typo in
    /// the template would otherwise turn into a publish stage that verifies zero packages
    /// and reports success.
    /// </para>
    /// </remarks>
    internal static Result<IReadOnlyList<ReleasePackageSet>> SelectSets(ReleasePlan plan, string? setName)
    {
        var ordered = plan.Sets.OrderBy(s => s.Order).ToList();

        if (string.IsNullOrWhiteSpace(setName))
        {
            return Result<IReadOnlyList<ReleasePackageSet>>.Success(ordered);
        }

        var matched = ordered
            .Where(s => string.Equals(s.ArtifactName, setName.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        return matched.Count > 0
            ? Result<IReadOnlyList<ReleasePackageSet>>.Success(matched)
            : Result<IReadOnlyList<ReleasePackageSet>>.Failure(
                ErrorCodes.PackageSetNotFound,
                $"The release plan has no package set with artifact name '{setName}'. " +
                $"It contains: {string.Join(", ", ordered.Select(s => s.ArtifactName))}.");
    }

    // ---- release verify ----

    /// <summary>
    /// Polls until every package in scope is indexed on NuGet.org, or the deadline expires.
    /// </summary>
    /// <remarks>
    /// The deadline covers the whole set, not each package. Observed production behaviour
    /// (build 3059242, 41 packages) was a long tail: one package alone held the run for
    /// 4m11s across 11 attempts, with the set completing in 10m30s over 29 attempts. A
    /// per-package budget would have to be large enough for that tail and would then be far
    /// too generous for the set.
    /// </remarks>
    public static async Task<int> VerifyAsync(
        IReleaseConsole console,
        IReleaseFileSystem fs,
        IPackageAvailabilityProbe probe,
        string planJson,
        TimeSpan maxDuration,
        TimeSpan pollInterval,
        Func<DateTimeOffset> clock,
        Func<TimeSpan, CancellationToken, Task> delay,
        string? setName,
        string? stageDirectory,
        string? expectedPlanHash,
        CancellationToken cancellationToken)
    {
        // `verify` is the authoritative "did the release succeed" signal, so it must not
        // read an unpinned plan even though the same job already verified it.
        var plan = ReleasePlanSerializer.VerifyAndDeserialize(planJson, expectedPlanHash ?? string.Empty);
        if (plan.IsFailure)
        {
            return ConsoleReporting.Fail(console, plan.Errors);
        }

        var sets = SelectSets(plan.Value, setName);
        if (sets.IsFailure)
        {
            return ConsoleReporting.Fail(console, sets.Errors);
        }

        // The same wiring risk applies here as in `filter`: a stage that verified the wrong
        // set would poll for packages another stage is responsible for.
        if (stageDirectory is { Length: > 0 })
        {
            foreach (var set in sets.Value)
            {
                var marker = ValidateSetMarker(
                    fs, Path.Combine(stageDirectory, set.ArtifactName), set, plan.Value.Source, setName);

                if (marker.IsFailure)
                {
                    return ConsoleReporting.Fail(console, marker.Errors);
                }
            }
        }

        // Every package in scope, including ones `filter` removed: those were withheld on
        // the grounds that they were already live, so their absence here means that was
        // wrong. Scoped to this stage's sets, because the other stage has not published yet.
        var packages = sets.Value.SelectMany(s => s.Packages).ToList();
        var deadline = clock() + maxDuration;

        IReadOnlyList<PlannedPackage> missing = packages;

        while (true)
        {
            try
            {
                var availability = await probe.GetAvailabilityAsync(packages, cancellationToken).ConfigureAwait(false);
                missing = VerificationEvaluator.GetMissing(packages, availability);

                if (missing.Count == 0)
                {
                    console.WriteLine($"Verified all {packages.Count} packages on NuGet.org.");
                    return ExitCodes.Success;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // A transient feed failure must not fail the release. This is the
                // authoritative "did it publish" signal, so it should be the most tolerant
                // component, not the least: the recovery for a failed verify is a re-run,
                // which is the path that risks a fatal 409. Only the deadline may fail it.
                console.WriteError($"warning: NuGet.org query failed, will retry: {ex.Message}");
            }

            if (clock() + pollInterval >= deadline)
            {
                return ConsoleReporting.Fail(console, [new ReleaseError(
                    ErrorCodes.PackageFileMissing,
                    VerificationEvaluator.DescribeMissing(missing))]);
            }

            console.WriteLine($"Waiting for {missing.Count} package(s) to become available on NuGet.org.");
            await delay(pollInterval, cancellationToken).ConfigureAwait(false);
        }
    }
}
