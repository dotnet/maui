using System.CommandLine;

namespace DotNet.Release;

internal static class StageCommand
{
    public static Command Build(TextWriter outputWriter, string toolVersion)
    {
        var config = new Option<FileInfo>("--config")
        {
            Description = "Release policy JSON.",
            Required = true,
        };
        var repo = new Option<string>("--repo")
        {
            Description = "Repository as 'owner/name'.",
            Required = true,
        };
        var commit = new Option<string>("--commit")
        {
            Description = "Exact source commit verified by release resolve.",
            Required = true,
        };
        var barId = new Option<int>("--bar-id")
        {
            Description = "Positive BAR build ID verified by release resolve.",
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

        var command = new Command("stage", "Read the gathered drop, validate it, and write release-manifest.json.")
        {
            config, repo, commit, barId, drop, output, include, exclude,
        };

        command.SetAction((parse, cancellationToken) => ExecuteAsync(
            outputWriter,
            File.ReadAllText(parse.GetValue(config)!.FullName),
            parse.GetValue(repo)!,
            parse.GetValue(commit)!,
            parse.GetValue(barId),
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

    public static async Task ExecuteAsync(
        TextWriter outputWriter,
        string policyJson,
        string repository,
        string commit,
        int barBuildId,
        string dropDirectory,
        string outputDirectory,
        StageOptions options,
        DateTimeOffset now,
        string toolVersion,
        CancellationToken cancellationToken)
    {
        var policy = ReleasePolicy.Parse(policyJson);
        var repositoryId = RepositoryId.Parse(repository);
        var repositoryPolicy = policy.GetRepository(repositoryId);

        if (string.IsNullOrWhiteSpace(commit))
        {
            throw new DotNetReleaseException("A commit must be supplied; a release is always pinned to an exact commit.");
        }

        if (barBuildId <= 0)
        {
            throw new DotNetReleaseException($"BAR build ID '{barBuildId}' must be positive.");
        }

        var source = new ReleaseSource
        {
            Repository = repositoryId.FullName,
            RepositoryUrl = repositoryId.GitHubUrl,
            Commit = commit.Trim(),
            BarBuildId = barBuildId,
            Workload = repositoryPolicy.Workload,
            Channel = repositoryPolicy.Channel,
        };

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

        var manifest = ReleaseManifestBuilder.Build(source, policy, packages, options, now, toolVersion);
        var sourceFiles = IndexPackageFiles(packageFiles);
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

        var manifestPath = Path.Combine(outputDirectory, ReleaseArtifact.ManifestFileName);
        await File.WriteAllTextAsync(manifestPath, manifestJson, cancellationToken).ConfigureAwait(false);

        foreach (var set in manifest.Sets)
        {
            StagedSetIntegrity.ValidateStaged(set, ReleaseArtifact.ReadPackageHashes(Path.Combine(outputDirectory, set.ArtifactName)));
        }

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
