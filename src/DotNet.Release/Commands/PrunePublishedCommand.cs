using System.CommandLine;

namespace DotNet.Release;

/// <summary>
/// Implements <c>release prune-published</c>, which removes already-published or recovered
/// packages from a local staged set without changing the release manifest.
/// </summary>
internal static class PrunePublishedCommand
{
    public static Command Build(TextWriter outputWriter)
    {
        var manifest = new Option<FileInfo>("--manifest")
        {
            Description = "release-manifest.json.",
            Required = true,
        };
        var stage = new Option<DirectoryInfo?>("--stage")
        {
            Description = "Release artifact directory. Defaults to the manifest directory.",
        };
        var recoveryFilters = new Option<string?>("--recovery-filters")
        {
            Description = "Semicolon-separated filters for already submitted packages.",
        };
        var expectedHash = new Option<string>("--expected-manifest-hash")
        {
            Description = "The SHA-256 emitted by the prepare stage.",
            Required = true,
        };
        var feed = new Option<string?>("--feed")
        {
            Description = "Feed index to query. Defaults to NuGet.org.",
        };
        var set = new Option<string>("--set")
        {
            Description = "Package-set directory name.",
            Required = true,
        };
        var command = new Command("prune-published", "Remove package versions already published on the target feed.")
        {
            manifest, stage, recoveryFilters, expectedHash, feed, set,
        };

        command.SetAction((parse, cancellationToken) =>
        {
            var manifestFile = parse.GetValue(manifest)!;
            using var lookup = new NuGetPackageLookup(parse.GetValue(feed));

            return ExecuteAsync(
                outputWriter,
                lookup,
                File.ReadAllText(manifestFile.FullName),
                parse.GetValue(stage)?.FullName ?? manifestFile.DirectoryName!,
                PackageGlob.ParseList(parse.GetValue(recoveryFilters)),
                parse.GetValue(expectedHash)!,
                parse.GetValue(set),
                cancellationToken);
        });

        return command;
    }

    public static async Task ExecuteAsync(
        TextWriter outputWriter,
        INuGetPackageLookup lookup,
        string manifestJson,
        string stageDirectory,
        IReadOnlyList<string> recoveryPatterns,
        string expectedManifestHash,
        string? setName,
        CancellationToken cancellationToken)
    {
        var manifest = ReleaseManifestSerializer.VerifyAndDeserialize(manifestJson, expectedManifestHash);
        var sets = ReleaseArtifact.SelectSets(manifest, setName);

        ReleaseOutput.WriteSelectedRelease(outputWriter, manifest, sets, expectedManifestHash);

        foreach (var set in sets)
        {
            var setDirectory = ReleaseArtifact.GetSetDirectory(stageDirectory, set);

            var availability = await lookup.GetAvailabilityAsync(set.Packages, cancellationToken).ConfigureAwait(false);
            var report = PrunePublishedPlanner.Plan(set, manifest.AllPackages, recoveryPatterns, availability);
            var invalidFileName = report.FilesToRemove.FirstOrDefault(fileName => !ReleaseArtifact.IsSinglePathComponent(fileName));
            if (invalidFileName is not null)
            {
                throw new DotNetReleaseException($"Package file name '{invalidFileName}' must not contain a directory.");
            }

            foreach (var fileName in report.FilesToRemove)
            {
                var path = Path.Combine(setDirectory, fileName);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                outputWriter.WriteLine($"Withheld {fileName}.");
            }

            StagedSetIntegrity.ValidateFiltered(set, ReleaseArtifact.ReadPackageHashes(setDirectory), report);

            ReleaseOutput.WritePruneReport(outputWriter, set, report);
        }
    }
}
