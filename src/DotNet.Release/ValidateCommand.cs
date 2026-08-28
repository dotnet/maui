using System.CommandLine;

namespace DotNet.Release;

internal static class ValidateCommand
{
    public static Command Build(IReleaseConsole console)
    {
        var plan = new Option<FileInfo>("--plan")
        {
            Description = "release-plan.json.",
            Required = true,
        };
        var stage = new Option<DirectoryInfo>("--stage")
        {
            Description = "Release artifact directory.",
            Required = true,
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

        var command = new Command(
            "validate",
            "Validate a downloaded package-set artifact without contacting NuGet.org.")
        {
            plan, stage, set, expectedHash,
        };

        command.SetAction(parse => Execute(
            console,
            File.ReadAllText(parse.GetValue(plan)!.FullName),
            parse.GetValue(stage)!.FullName,
            parse.GetValue(expectedHash)!,
            parse.GetValue(set)!));

        return command;
    }

    public static int Execute(
        IReleaseConsole console,
        string planJson,
        string stageDirectory,
        string expectedPlanHash,
        string setName)
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

            var integrity = StagedSetIntegrity.ValidateStaged(
                set,
                ReleaseArtifact.ReadPackageHashes(setDirectory.Value));
            if (integrity.IsFailure)
            {
                return ConsoleReporting.Fail(console, integrity.Errors);
            }

            console.WriteLine(
                $"Validated {set.Name}: {set.Packages.Count} package(s), no NuGet.org query.");
        }

        return ExitCodes.Success;
    }
}
