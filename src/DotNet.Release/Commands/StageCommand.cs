using System.CommandLine;
using Microsoft.DotNet.ProductConstructionService.Client;

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
        var barId = new Option<int>("--bar-id")
        {
            Description = "Candidate BAR build ID resolved by Darc.",
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
        var barUri = new Option<string?>("--bar-uri")
        {
            Description = "Product Construction Service URI. Defaults to production.",
        };
        var token = new Option<string?>("--bar-token")
        {
            Description = "Access token. Omit to use the ambient Azure identity.",
        };
        var managedIdentity = new Option<string?>("--managed-identity")
        {
            Description = "Managed identity client ID.",
        };

        var command = new Command("stage", "Read the gathered drop, validate it, and write release-manifest.json.")
        {
            config, repo, barId, drop, output, include, exclude, barUri, token, managedIdentity,
        };

        command.SetAction((parse, cancellationToken) =>
        {
            var api = CreateApi(parse.GetValue(barUri), parse.GetValue(token), parse.GetValue(managedIdentity));

            return ExecuteAsync(
                outputWriter,
                MaestroBuildRegistry.Create(api),
                File.ReadAllText(parse.GetValue(config)!.FullName),
                parse.GetValue(repo)!,
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
                cancellationToken);
        });

        return command;
    }

    public static async Task ExecuteAsync(
        TextWriter outputWriter,
        IBuildRegistry registry,
        string policyJson,
        string repository,
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

        if (barBuildId <= 0)
        {
            throw new DotNetReleaseException($"BAR build ID '{barBuildId}' must be positive.");
        }

        var request = new ReleaseRequest(repositoryId, barBuildId);
        var candidates = await registry.GetBuildAsync(barBuildId, cancellationToken).ConfigureAwait(false);
        var resolved = BuildResolver.Resolve(request, repositoryPolicy, candidates);
        ReleaseOutput.WriteResolvedBuild(outputWriter, resolved);
        outputWriter.WriteLine();

        var source = new ReleaseSource
        {
            Repository = resolved.Repository,
            RepositoryUrl = resolved.RepositoryUrl,
            Commit = resolved.Commit,
            BarBuildId = resolved.BarBuildId,
            Workload = resolved.Workload,
            Channel = resolved.Channel,
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

        foreach (var set in manifest.Sets)
        {
            StagedSetIntegrity.ValidateStaged(set, ReleaseArtifact.ReadPackageHashes(Path.Combine(outputDirectory, set.ArtifactName)));
        }

        var manifestPath = Path.Combine(outputDirectory, ReleaseArtifact.ManifestFileName);
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

    private static IProductConstructionServiceApi CreateApi(string? baseUri, string? token, string? managedIdentityId) =>
        baseUri is { Length: > 0 }
            ? PcsApiFactory.GetAuthenticated(baseUri, token!, managedIdentityId!, disableInteractiveAuth: true)
            : PcsApiFactory.GetAuthenticated(token!, managedIdentityId!, disableInteractiveAuth: true);
}
