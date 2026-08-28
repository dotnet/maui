using System.CommandLine;

namespace DotNet.Release;

internal static class VerifyCommand
{
    public static Command Build(IReleaseConsole console)
    {
        var plan = new Option<FileInfo>("--plan")
        {
            Description = "release-plan.json.",
            Required = true,
        };
        var maxDuration = new Option<int>("--max-duration-minutes")
        {
            Description = "Verification deadline.",
            DefaultValueFactory = _ => 30,
        };
        var interval = new Option<int>("--poll-seconds")
        {
            Description = "Delay between polls.",
            DefaultValueFactory = _ => 20,
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
        var expectedHash = new Option<string>("--expected-plan-hash")
        {
            Description = "The SHA-256 emitted by the prepare stage.",
            Required = true,
        };
        var stage = new Option<DirectoryInfo?>("--stage")
        {
            Description = "Release artifact directory. Defaults to the plan directory.",
        };

        var command = new Command(
            "verify",
            "Poll until every package in the set is indexed on NuGet.org.")
        {
            plan, maxDuration, interval, feed, set, stage, expectedHash,
        };

        command.SetAction((parse, cancellationToken) =>
        {
            var planFile = parse.GetValue(plan)!;
            using var checker = new FlatContainerExistenceChecker(parse.GetValue(feed));

            return ExecuteAsync(
                console,
                new PackageAvailabilityProbe(checker),
                File.ReadAllText(planFile.FullName),
                TimeSpan.FromMinutes(parse.GetValue(maxDuration)),
                TimeSpan.FromSeconds(parse.GetValue(interval)),
                () => DateTimeOffset.UtcNow,
                Task.Delay,
                parse.GetValue(set),
                parse.GetValue(stage)?.FullName ?? planFile.DirectoryName!,
                parse.GetValue(expectedHash)!,
                cancellationToken);
        });

        return command;
    }

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
