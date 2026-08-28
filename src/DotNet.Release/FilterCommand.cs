namespace DotNet.Release;

internal static class FilterCommand
{
    public const string ReportFileName = "release-filter.json";

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
