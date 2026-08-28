using System.CommandLine;

namespace DotNet.Release;

internal static class StageCommand
{
    public static Command Build(IReleaseConsole console, string toolVersion)
    {
        var config = new Option<FileInfo>("--config")
        {
            Description = "Release policy JSON.",
            Required = true,
        };
        var plan = new Option<FileInfo>("--plan")
        {
            Description = "plan.json written by release plan.",
            Required = true,
        };
        var drop = new Option<DirectoryInfo>("--drop")
        {
            Description = "Directory produced by darc gather-drop.",
            Required = true,
        };
        var output = new Option<DirectoryInfo>("--out")
        {
            Description = "Output directory.",
            Required = true,
        };
        var include = new Option<string?>("--include")
        {
            Description = "Semicolon-separated include filters.",
        };
        var exclude = new Option<string?>("--exclude")
        {
            Description = "Semicolon-separated exclude filters.",
        };

        var command = new Command(
            "stage",
            "Read the gathered drop, validate it, and write release-plan.json.")
        {
            config, plan, drop, output, include, exclude,
        };

        command.SetAction((parse, cancellationToken) => ExecuteAsync(
            console,
            File.ReadAllText(parse.GetValue(config)!.FullName),
            File.ReadAllText(parse.GetValue(plan)!.FullName),
            parse.GetValue(drop)!.FullName,
            parse.GetValue(output)!.FullName,
            new StageOptions
            {
                Include = PackageGlob.ParseList(parse.GetValue(include)),
                Exclude = PackageGlob.ParseList(parse.GetValue(exclude)),
            },
            DateTimeOffset.UtcNow,
            toolVersion,
            cancellationToken));

        return command;
    }

    public static async Task<int> ExecuteAsync(
        IReleaseConsole console,
        string policyJson,
        string resolvedPlanJson,
        string dropDirectory,
        string outputDirectory,
        StageOptions options,
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

        var packageFiles = FindShippingPackages(dropDirectory);
        if (packageFiles.Count == 0)
        {
            return ConsoleReporting.Fail(console, [new ReleaseError(
                ErrorCodes.PackageSetEmpty,
                $"No shipping nupkgs were found under '{dropDirectory}'.")]);
        }

        var reader = new NupkgIdentityReader();
        var packages = new List<DropPackage>(packageFiles.Count);
        var readErrors = new List<ReleaseError>();

        foreach (var file in packageFiles)
        {
            var read = await reader.ReadAsync(file, cancellationToken).ConfigureAwait(false);
            if (read.IsFailure)
            {
                readErrors.AddRange(read.Errors);
            }
            else
            {
                packages.Add(read.Value);
            }
        }

        if (readErrors.Count > 0)
        {
            return ConsoleReporting.Fail(console, readErrors);
        }

        var plan = StagePlanner.Create(
            resolved.Value,
            policy.Value,
            packages,
            options,
            now,
            toolVersion);
        if (plan.IsFailure)
        {
            return ConsoleReporting.Fail(console, plan.Errors);
        }

        var sourceFiles = IndexPackageFiles(packageFiles);
        if (sourceFiles.IsFailure)
        {
            return ConsoleReporting.Fail(console, sourceFiles.Errors);
        }

        Directory.CreateDirectory(outputDirectory);
        var planJson = ReleasePlanSerializer.Serialize(plan.Value);

        foreach (var set in plan.Value.Sets.OrderBy(set => set.Order))
        {
            var setDirectory = Path.Combine(outputDirectory, set.ArtifactName);
            Directory.CreateDirectory(setDirectory);

            foreach (var package in set.Packages)
            {
                File.Copy(
                    sourceFiles.Value[package.FileName],
                    Path.Combine(setDirectory, package.FileName),
                    overwrite: true);
            }

            await File.WriteAllTextAsync(
                Path.Combine(setDirectory, ReleaseArtifact.PlanFileName),
                planJson,
                cancellationToken).ConfigureAwait(false);
            await File.WriteAllTextAsync(
                Path.Combine(setDirectory, ReleaseSetMarker.FileName),
                ReleasePlanSerializer.Serialize(ReleaseSetMarker.For(set, resolved.Value)),
                cancellationToken).ConfigureAwait(false);
        }

        var planPath = Path.Combine(outputDirectory, ReleaseArtifact.PlanFileName);
        await File.WriteAllTextAsync(planPath, planJson, cancellationToken).ConfigureAwait(false);

        foreach (var set in plan.Value.Sets)
        {
            var integrity = StagedSetIntegrity.ValidateStaged(
                set,
                ReleaseArtifact.ReadPackageHashes(
                    Path.Combine(outputDirectory, set.ArtifactName)));
            if (integrity.IsFailure)
            {
                return ConsoleReporting.Fail(console, integrity.Errors);
            }
        }

        ConsoleReporting.WriteSummary(console, plan.Value);
        console.WriteLine(string.Empty);
        console.WriteLine($"Wrote {planPath}.");
        console.WriteLine($"Release plan SHA-256: {ReleasePlanSerializer.ComputeHash(planJson)}");

        return ExitCodes.Success;
    }

    internal static List<string> FindShippingPackages(string dropDirectory)
    {
        var shipping = Path.Combine(dropDirectory, "shipping", "packages");
        var root = Directory.Exists(shipping) ? shipping : dropDirectory;

        return Directory.Exists(root)
            ? [.. Directory
                .EnumerateFiles(root, "*.nupkg", SearchOption.AllDirectories)
                .Order(StringComparer.Ordinal)]
            : [];
    }

    private static Result<IReadOnlyDictionary<string, string>> IndexPackageFiles(
        IReadOnlyList<string> packageFiles)
    {
        var indexed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in packageFiles)
        {
            var name = Path.GetFileName(file);
            if (!indexed.TryAdd(name, file))
            {
                return Result<IReadOnlyDictionary<string, string>>.Failure(
                    ErrorCodes.PackageDuplicateFileName,
                    $"The gathered drop contains more than one '{name}': " +
                    $"'{indexed[name]}' and '{file}'.");
            }
        }

        return Result<IReadOnlyDictionary<string, string>>.Success(indexed);
    }
}
