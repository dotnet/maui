using System.CommandLine;

namespace DotNet.Release;

internal static class PrunePublishedCommand
{
    public const string ReportFileName = "release-prune.json";

    public static Command Build(TextWriter outputWriter)
    {
        var plan = new Option<FileInfo>("--plan")
        {
            Description = "release-plan.json.",
            Required = true,
        };
        var stage = new Option<DirectoryInfo?>("--stage")
        {
            Description = "Release artifact directory. Defaults to the plan directory.",
        };
        var recoveryFilters = new Option<string?>("--recovery-filters")
        {
            Description = "Semicolon-separated filters for already submitted packages.",
        };
        var expectedHash = new Option<string>("--expected-plan-hash")
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
            plan, stage, recoveryFilters, expectedHash, feed, set,
        };

        command.SetAction((parse, cancellationToken) =>
        {
            var planFile = parse.GetValue(plan)!;
            using var lookup = new NuGetPackageLookup(parse.GetValue(feed));

            return ExecuteAsync(outputWriter, lookup, File.ReadAllText(planFile.FullName), parse.GetValue(stage)?.FullName ?? planFile.DirectoryName!,
                PackageGlob.ParseList(parse.GetValue(recoveryFilters)), parse.GetValue(expectedHash)!, parse.GetValue(set), cancellationToken);
        });

        return command;
    }

    public static async Task ExecuteAsync(TextWriter outputWriter, INuGetPackageLookup lookup, string planJson, string stageDirectory,
        IReadOnlyList<string> recoveryPatterns, string expectedPlanHash, string? setName, CancellationToken cancellationToken)
    {
        var plan = ReleasePlanSerializer.VerifyAndDeserialize(planJson, expectedPlanHash);
        var sets = ReleaseArtifact.SelectSets(plan, setName);

        ReleaseOutput.WriteSelectedRelease(outputWriter, plan, sets, expectedPlanHash);
        var pending = 0;

        foreach (var set in sets)
        {
            var setDirectory = ReleaseArtifact.GetSetDirectory(stageDirectory, set);

            var availability = await lookup.GetAvailabilityAsync(set.Packages, cancellationToken).ConfigureAwait(false);
            var report = PrunePublishedPlanner.Plan(set, plan.AllPackages, recoveryPatterns, availability);
            var invalidFileName = report.FilesToRemove
                .FirstOrDefault(fileName => !ReleaseArtifact.IsSinglePathComponent(fileName));
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

            await File.WriteAllTextAsync(Path.Combine(setDirectory, ReportFileName), ReleasePlanSerializer.Serialize(report),
                cancellationToken).ConfigureAwait(false);

            ReleaseOutput.WritePruneReport(outputWriter, set, report);
            pending += report.PendingCount;
        }

        outputWriter.WriteLine(AzurePipelineCommand.SetPackagesToPublish(pending > 0));
    }
}
