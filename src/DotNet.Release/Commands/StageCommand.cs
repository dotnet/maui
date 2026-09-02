using System.CommandLine;

namespace DotNet.Release;

internal static class StageCommand
{
    public static Command Build(TextWriter outputWriter)
    {
        var config = new Option<FileInfo>("--config")
        {
            Description = "Release policy JSON.",
            Required = true,
        };
        var manifest = new Option<FileInfo>("--manifest")
        {
            Description = "Manifest initialized by release resolve.",
            Required = true,
        };
        var drop = new Option<DirectoryInfo>("--drop")
        {
            Description = "Directory produced by darc gather-drop.",
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

        var command = new Command("stage", "Read the gathered drop, validate it, and write release-manifest.json.")
        {
            config, manifest, drop, include, exclude,
        };

        command.SetAction((parse, cancellationToken) =>
        {
            var manifestFile = parse.GetValue(manifest)!;

            return ExecuteAsync(
                outputWriter,
                File.ReadAllText(parse.GetValue(config)!.FullName),
                File.ReadAllText(manifestFile.FullName),
                manifestFile.FullName,
                parse.GetValue(drop)!.FullName,
                new StageOptions
                {
                    Include = PackageGlob.ParseList(parse.GetValue(include)),
                    Exclude = PackageGlob.ParseList(parse.GetValue(exclude)),
                },
                cancellationToken);
        });

        return command;
    }

    public static async Task ExecuteAsync(
        TextWriter outputWriter,
        string policyJson,
        string resolvedManifestJson,
        string manifestPath,
        string dropDirectory,
        StageOptions options,
        CancellationToken cancellationToken)
    {
        var policy = ReleasePolicy.Parse(policyJson);
        var resolvedManifest = ReleaseManifestSerializer.DeserializeManifest(resolvedManifestJson);
        var source = resolvedManifest.Source;
        var repositoryId = RepositoryId.Parse(source.Repository);
        var repositoryPolicy = policy.GetRepository(repositoryId);

        if (resolvedManifest.Sets.Count != 0 || resolvedManifest.WorkloadSet is not null)
        {
            throw new DotNetReleaseException("The manifest supplied to release stage has already been staged.");
        }

        if (string.IsNullOrWhiteSpace(resolvedManifest.ToolVersion) || resolvedManifest.CreatedUtc == default)
        {
            throw new DotNetReleaseException("The resolved release manifest is missing tool identity or creation time.");
        }

        if (!string.Equals(source.RepositoryUrl, repositoryId.GitHubUrl, StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(source.Commit) ||
            source.BarBuildId <= 0 ||
            source.Workload != repositoryPolicy.Workload ||
            source.Channel != repositoryPolicy.Channel)
        {
            throw new DotNetReleaseException("The resolved release manifest does not match current repository policy or has incomplete source identity.");
        }

        var packageFiles = FindShippingPackages(dropDirectory);
        if (packageFiles.Count == 0)
        {
            throw new DotNetReleaseException($"No shipping nupkgs were found under '{dropDirectory}'.");
        }

        var reader = new NupkgIdentityReader();
        var packages = new List<DropPackage>(packageFiles.Count);
        var readErrors = new List<string>();

        foreach (var file in packageFiles)
        {
            try
            {
                packages.Add(await reader.ReadAsync(file, cancellationToken).ConfigureAwait(false));
            }
            catch (DotNetReleaseException ex)
            {
                readErrors.Add(ex.Message);
            }
        }

        if (readErrors.Count > 0)
        {
            throw new DotNetReleaseException(readErrors);
        }

        var manifest = ReleaseManifestBuilder.Build(
            source, policy, packages, options, resolvedManifest.CreatedUtc, resolvedManifest.ToolVersion);
        var sourceFiles = IndexPackageFiles(packageFiles);
        var outputDirectory = Path.GetDirectoryName(manifestPath)!;
        Directory.CreateDirectory(outputDirectory);
        var manifestJson = ReleaseManifestSerializer.Serialize(manifest);

        foreach (var set in manifest.Sets.OrderBy(set => set.Order))
        {
            var setDirectory = Path.Combine(outputDirectory, set.ArtifactName);
            Directory.CreateDirectory(setDirectory);

            foreach (var package in set.Packages)
            {
                File.Copy(sourceFiles[package.FileName], Path.Combine(setDirectory, package.FileName), overwrite: true);
            }
        }

        foreach (var set in manifest.Sets)
        {
            StagedSetIntegrity.ValidateStaged(set, ReleaseArtifact.ReadPackageHashes(Path.Combine(outputDirectory, set.ArtifactName)));
        }

        await File.WriteAllTextAsync(manifestPath, manifestJson, cancellationToken).ConfigureAwait(false);

        ReleaseOutput.WriteReleaseManifest(outputWriter, manifest);
        outputWriter.WriteLine();
        outputWriter.WriteLine($"Wrote {manifestPath}.");
        outputWriter.WriteLine($"Release manifest SHA-256: {ReleaseManifestSerializer.ComputeHash(manifestJson)}");
    }

    internal static List<string> FindShippingPackages(string dropDirectory)
    {
        var shipping = Path.Combine(dropDirectory, "shipping", "packages");

        return Directory.Exists(shipping) ? [.. Directory.EnumerateFiles(shipping, "*.nupkg", SearchOption.AllDirectories).Order(StringComparer.Ordinal)] : [];
    }

    private static IReadOnlyDictionary<string, string> IndexPackageFiles(IReadOnlyList<string> packageFiles)
    {
        var indexed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in packageFiles)
        {
            var name = Path.GetFileName(file);
            if (!indexed.TryAdd(name, file))
            {
                throw new DotNetReleaseException(
                    $"The gathered drop contains more than one '{name}': '{indexed[name]}' and '{file}'.");
            }
        }

        return indexed;
    }

}
