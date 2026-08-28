using System.CommandLine;

namespace DotNet.Release;

internal static class FilterCommand
{
    public const string ReportFileName = "release-filter.json";

    public static Command Build(IReleaseConsole console)
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

        var command = new Command(
            "filter",
            "Remove already-published packages from the staged set.")
        {
            plan, stage, recoveryFilters, expectedHash, feed, set,
        };

        command.SetAction((parse, cancellationToken) =>
        {
            var planFile = parse.GetValue(plan)!;
            using var checker = new FlatContainerExistenceChecker(parse.GetValue(feed));

            return ExecuteAsync(
                console,
                new PackageAvailabilityProbe(checker),
                File.ReadAllText(planFile.FullName),
                parse.GetValue(stage)?.FullName ?? planFile.DirectoryName!,
                PackageGlob.ParseList(parse.GetValue(recoveryFilters)),
                parse.GetValue(expectedHash)!,
                parse.GetValue(set),
                cancellationToken);
        });

        return command;
    }

    public static async Task<int> ExecuteAsync(
        IReleaseConsole console,
        IPackageAvailabilityProbe probe,
        string planJson,
        string stageDirectory,
        IReadOnlyList<string> recoveryPatterns,
        string expectedPlanHash,
        string? setName,
        CancellationToken cancellationToken)
    {
        var plan = ReleasePlanSerializer.VerifyAndDeserialize(planJson, expectedPlanHash);
        if (plan.IsFailure)
        {
            return ConsoleReporting.Fail(console, plan.Errors);
        }

        var sets = ReleaseArtifact.SelectSets(plan.Value, setName);
        if (sets.IsFailure)
        {
            return ConsoleReporting.Fail(console, sets.Errors);
        }

        var pending = 0;

        foreach (var set in sets.Value)
        {
            var setDirectory = ReleaseArtifact.GetSetDirectory(stageDirectory, set);
            if (setDirectory.IsFailure)
            {
                return ConsoleReporting.Fail(console, setDirectory.Errors);
            }

            var marker = ReleaseArtifact.ValidateSetMarker(
                setDirectory.Value,
                set,
                plan.Value.Source,
                setName);
            if (marker.IsFailure)
            {
                return ConsoleReporting.Fail(console, marker.Errors);
            }

            var availability = await probe
                .GetAvailabilityAsync(set.Packages, cancellationToken)
                .ConfigureAwait(false);
            var report = FilterPlanner.Plan(set, recoveryPatterns, availability);
            if (report.IsFailure)
            {
                return ConsoleReporting.Fail(console, report.Errors);
            }

            var invalidFileName = report.Value.FilesToRemove
                .FirstOrDefault(fileName => !ReleaseArtifact.IsSinglePathComponent(fileName));
            if (invalidFileName is not null)
            {
                return ConsoleReporting.Fail(console, [new ReleaseError(
                    ErrorCodes.PlanSchemaInvalid,
                    $"Package file name '{invalidFileName}' must not contain a directory.")]);
            }

            foreach (var fileName in report.Value.FilesToRemove)
            {
                var path = Path.Combine(setDirectory.Value, fileName);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                console.WriteLine($"Withheld {fileName}.");
            }

            var integrity = StagedSetIntegrity.ValidateFiltered(
                set,
                ReleaseArtifact.ReadPackageHashes(setDirectory.Value),
                report.Value);
            if (integrity.IsFailure)
            {
                return ConsoleReporting.Fail(console, integrity.Errors);
            }

            await File.WriteAllTextAsync(
                Path.Combine(setDirectory.Value, ReportFileName),
                ReleasePlanSerializer.Serialize(report.Value),
                cancellationToken).ConfigureAwait(false);

            console.WriteLine(
                $"{set.Name}: {report.Value.PendingCount} of " +
                $"{set.Packages.Count} remain to publish.");
            pending += report.Value.PendingCount;
        }

        console.WriteLine(AzurePipelineCommand.SetPackagesToPublish(pending > 0));
        return ExitCodes.Success;
    }
}
