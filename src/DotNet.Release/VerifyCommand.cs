namespace DotNet.Release;

internal static class VerifyCommand
{
    public static async Task<int> ExecuteAsync(
        IReleaseConsole console,
        IPackageAvailabilityProbe probe,
        string planJson,
        TimeSpan maxDuration,
        TimeSpan pollInterval,
        Func<DateTimeOffset> clock,
        Func<TimeSpan, CancellationToken, Task> delay,
        string? setName,
        string stageDirectory,
        string expectedPlanHash,
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
        }

        var packages = sets.Value.SelectMany(set => set.Packages).ToList();
        var deadline = clock() + maxDuration;
        IReadOnlyList<PlannedPackage> missing = packages;

        while (true)
        {
            try
            {
                var availability = await probe
                    .GetAvailabilityAsync(packages, cancellationToken)
                    .ConfigureAwait(false);
                missing = VerificationEvaluator.GetMissing(packages, availability);

                if (missing.Count == 0)
                {
                    console.WriteLine(
                        $"Verified all {packages.Count} packages on NuGet.org.");
                    return ExitCodes.Success;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                console.WriteError(
                    $"warning: NuGet.org query failed, will retry: {ex.Message}");
            }

            if (clock() + pollInterval >= deadline)
            {
                return ConsoleReporting.Fail(console, [new ReleaseError(
                    ErrorCodes.PackageFileMissing,
                    VerificationEvaluator.DescribeMissing(missing))]);
            }

            console.WriteLine(
                $"Waiting for {missing.Count} package(s) to become available on NuGet.org.");
            await delay(pollInterval, cancellationToken).ConfigureAwait(false);
        }
    }
}
