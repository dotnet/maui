using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNet.Release;

/// <summary>
/// Implements <c>release stage</c>, which validates gathered packages and creates the
/// immutable release manifest and package-set directories.
/// </summary>
internal static class StageCommand
{
    private static readonly JsonSerializerOptions ReleaseSourceJsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static Command Build(TextWriter outputWriter, string toolVersion)
    {
        var config = new Option<FileInfo>("--config")
        {
            Description = "Release policy JSON.",
            Required = true,
        };
        var resolvedBuild = new Option<FileInfo>("--resolved-build")
        {
            Description = "Verified JSON written by release resolve.",
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
            config, resolvedBuild, drop, output, include, exclude,
        };

        command.SetAction((parse, cancellationToken) => ExecuteAsync(
            outputWriter,
            File.ReadAllText(parse.GetValue(config)!.FullName),
            File.ReadAllText(parse.GetValue(resolvedBuild)!.FullName),
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
        string resolvedBuildJson,
        string dropDirectory,
        string outputDirectory,
        StageOptions options,
        DateTimeOffset now,
        string toolVersion,
        CancellationToken cancellationToken)
    {
        var policy = ReleasePolicy.Parse(policyJson);
        var source = DeserializeReleaseSource(resolvedBuildJson);
        var repositoryId = RepositoryId.Parse(source.Repository);
        var repositoryPolicy = policy.GetRepository(repositoryId);

        if (!string.Equals(source.RepositoryUrl, repositoryId.GitHubUrl, StringComparison.Ordinal) ||
            source.Commit is not { Length: 40 } ||
            !source.Commit.All(Uri.IsHexDigit) ||
            source.BarBuildId <= 0 ||
            source.Workload != repositoryPolicy.Workload ||
            source.Channel != repositoryPolicy.Channel)
        {
            throw new DotNetReleaseException("The resolved build does not match current repository policy or has incomplete identity.");
        }

        ReleaseOutput.WriteResolvedBuild(outputWriter, source);
        outputWriter.WriteLine();

        var allPackageFiles = FindShippingPackages(dropDirectory);
        if (allPackageFiles.Count == 0)
        {
            throw new DotNetReleaseException($"No shipping nupkgs were found under '{dropDirectory}'.");
        }

        var packageFiles = allPackageFiles
            .Where(file => ReleaseManifestBuilder.IsSelected(Path.GetFileName(file), source.Workload, options))
            .ToList();
        if (packageFiles.Count == 0)
        {
            throw new DotNetReleaseException("Package filtering selected no packages.");
        }

        var packages = new List<ReleasePackage>(packageFiles.Count);
        var readErrors = new List<string>();

        foreach (var file in packageFiles)
        {
            try
            {
                packages.Add(await NuGetClient.ReadPackageAsync(file, cancellationToken).ConfigureAwait(false));
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
        var manifestJson = manifest.Serialize();

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
            set.ValidateDirectory(Path.Combine(outputDirectory, set.ArtifactName));
        }

        var manifestPath = Path.Combine(outputDirectory, ReleaseManifest.FileName);
        await File.WriteAllTextAsync(manifestPath, manifestJson, cancellationToken).ConfigureAwait(false);

        ReleaseOutput.WriteReleaseManifest(outputWriter, manifest);
        outputWriter.WriteLine();
        outputWriter.WriteLine($"Wrote {manifestPath}.");
        outputWriter.WriteLine($"Release manifest SHA-256: {manifest.ComputeHash()}");
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

    internal static ReleaseSource DeserializeReleaseSource(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<ReleaseSource>(json, ReleaseSourceJsonOptions) ??
                throw new DotNetReleaseException("The resolved build output is empty.");
        }
        catch (JsonException ex)
        {
            throw new DotNetReleaseException($"The resolved build output is not valid: {ex.Message}");
        }
    }
}
