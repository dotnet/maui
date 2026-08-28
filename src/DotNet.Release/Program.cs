using System.CommandLine;
using System.Reflection;
using Microsoft.DotNet.ProductConstructionService.Client;

namespace DotNet.Release;

/// <summary>
/// The <c>release</c> command line.
/// </summary>
/// <remarks>
/// Four verbs, each with one responsibility: <c>plan</c> and <c>stage</c> bracket the
/// <c>darc gather-drop</c> pipeline step; <c>filter</c> and <c>verify</c> bracket the
/// <c>1ES.PublishNuget@1</c> step.
/// <para>
/// There is no <c>push</c> verb and no <c>--push</c> flag. 1ES performs every upload, so
/// this tool never holds a NuGet.org credential. See docs/design.md section 3.
/// </para>
/// </remarks>
internal static class Program
{
    private static string ToolVersion =>
        typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "0.0.0";

    public static Task<int> Main(string[] args) => BuildRootCommand().Parse(args).InvokeAsync();

    internal static RootCommand BuildRootCommand()
    {
        var console = new SystemConsole();

        var root = new RootCommand(
            "Prepares and verifies a .NET package release from a BAR build. " +
            "Never pushes packages: 1ES.PublishNuget@1 performs every upload.");

        root.Subcommands.Add(BuildPlanCommand(console));
        root.Subcommands.Add(BuildStageCommand(console));
        root.Subcommands.Add(BuildValidateCommand(console));
        root.Subcommands.Add(BuildFilterCommand(console));
        root.Subcommands.Add(BuildVerifyCommand(console));

        return root;
    }

    private static Command BuildPlanCommand(IReleaseConsole console)
    {
        var config = new Option<FileInfo>("--config") { Description = "Release policy JSON.", Required = true };
        var repo = new Option<string>("--repo") { Description = "Repository as 'owner/name'.", Required = true };
        var commit = new Option<string>("--commit") { Description = "Exact source commit registered in BAR.", Required = true };
        var barId = new Option<int?>("--bar-id") { Description = "BAR build ID, for builds BAR records without a GitHub URL." };
        var output = new Option<DirectoryInfo>("--out") { Description = "Output directory.", Required = true };
        var barUri = new Option<string?>("--bar-uri") { Description = "Product Construction Service URI. Defaults to production." };
        var token = new Option<string?>("--bar-token") { Description = "Access token. Omit to use the ambient Azure identity." };
        var managedIdentity = new Option<string?>("--managed-identity") { Description = "Managed identity client ID." };

        var command = new Command("plan", "Resolve and verify the BAR build, then write plan.json.")
        {
            config, repo, commit, barId, output, barUri, token, managedIdentity,
        };

        command.SetAction((parse, cancellationToken) =>
        {
            var api = CreateApi(
                parse.GetValue(barUri),
                parse.GetValue(token),
                parse.GetValue(managedIdentity));

            return Verbs.PlanAsync(
                console,
                MaestroBuildRegistry.Create(api),
                File.ReadAllText(parse.GetValue(config)!.FullName),
                parse.GetValue(repo)!,
                parse.GetValue(commit)!,
                parse.GetValue(barId),
                parse.GetValue(output)!.FullName,
                DateTimeOffset.UtcNow,
                ToolVersion,
                cancellationToken);
        });

        return command;
    }

    private static Command BuildStageCommand(IReleaseConsole console)
    {
        var config = new Option<FileInfo>("--config") { Description = "Release policy JSON.", Required = true };
        var plan = new Option<FileInfo>("--plan") { Description = "plan.json written by `release plan`.", Required = true };
        var drop = new Option<DirectoryInfo>("--drop") { Description = "Directory produced by `darc gather-drop`.", Required = true };
        var output = new Option<DirectoryInfo>("--out") { Description = "Output directory.", Required = true };
        var include = new Option<string?>("--include") { Description = "Semicolon-separated include filters." };
        var exclude = new Option<string?>("--exclude") { Description = "Semicolon-separated exclude filters." };

        var command = new Command("stage", "Read the gathered drop, validate it, and write release-plan.json.")
        {
            config, plan, drop, output, include, exclude,
        };

        command.SetAction((parse, cancellationToken) => Verbs.StageAsync(
            console,
            File.ReadAllText(parse.GetValue(config)!.FullName),
            File.ReadAllText(parse.GetValue(plan)!.FullName),
            parse.GetValue(drop)!.FullName,
            parse.GetValue(output)!.FullName,
            new StageOptions
            {
                Include = PackageGlob.ParseList(parse.GetValue(include)),
                Exclude = PackageGlob.ParseList(parse.GetValue(exclude)),
            },
            DateTimeOffset.UtcNow,
            ToolVersion,
            cancellationToken));

        return command;
    }

    private static Command BuildValidateCommand(IReleaseConsole console)
    {
        var plan = new Option<FileInfo>("--plan") { Description = "release-plan.json.", Required = true };
        var stage = new Option<DirectoryInfo>("--stage") { Description = "Release artifact directory.", Required = true };
        var set = new Option<string>("--set") { Description = "Package-set directory name.", Required = true };
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

        command.SetAction(parse => Verbs.Validate(
            console,
            File.ReadAllText(parse.GetValue(plan)!.FullName),
            parse.GetValue(stage)!.FullName,
            parse.GetValue(expectedHash)!,
            parse.GetValue(set)!));

        return command;
    }

    private static Command BuildFilterCommand(IReleaseConsole console)
    {
        var plan = new Option<FileInfo>("--plan") { Description = "release-plan.json.", Required = true };
        var stage = new Option<DirectoryInfo?>("--stage") { Description = "Staging directory. Defaults to the plan's directory." };
        var recoveryFilters = new Option<string?>("--recovery-filters")
        {
            Description = "Semicolon-separated filters for packages already submitted by this release.",
        };
        var expectedHash = new Option<string>("--expected-plan-hash")
        {
            Description = "The SHA-256 the preparing stage pinned. The plan must match it.",
            Required = true,
        };
        var feed = new Option<string?>("--feed") { Description = "Feed index to query. Defaults to NuGet.org." };
        var set = new Option<string>("--set")
        {
            Description = "Directory name of the package set this stage publishes.",
            Required = true,
        };

        var command = new Command("filter", "Remove already-published packages from the staging directory.")
        {
            plan, stage, recoveryFilters, expectedHash, feed, set,
        };

        command.SetAction((parse, cancellationToken) =>
        {
            var planFile = parse.GetValue(plan)!;
            using var checker = new FlatContainerExistenceChecker(parse.GetValue(feed));

            return Verbs.FilterAsync(
                console,
                new PackageAvailabilityProbe(checker),
                File.ReadAllText(planFile.FullName),
                parse.GetValue(stage)?.FullName ?? planFile.DirectoryName!,
                PackageGlob.ParseList(parse.GetValue(recoveryFilters)),
                parse.GetValue(expectedHash),
                parse.GetValue(set),
                cancellationToken);
        });

        return command;
    }

    private static Command BuildVerifyCommand(IReleaseConsole console)
    {
        var plan = new Option<FileInfo>("--plan") { Description = "release-plan.json.", Required = true };
        var maxDuration = new Option<int>("--max-duration-minutes") { Description = "Verification deadline.", DefaultValueFactory = _ => 30 };
        var interval = new Option<int>("--poll-seconds") { Description = "Delay between polls.", DefaultValueFactory = _ => 20 };
        var feed = new Option<string?>("--feed") { Description = "Feed index to query. Defaults to NuGet.org." };
        var set = new Option<string>("--set")
        {
            Description = "Directory name of the package set this stage published.",
            Required = true,
        };
        var expectedHash = new Option<string>("--expected-plan-hash")
        {
            Description = "The SHA-256 the preparing stage pinned. The plan must match it.",
            Required = true,
        };
        var stage = new Option<DirectoryInfo?>("--stage") { Description = "Directory containing the staged set directories. Defaults to the plan's directory." };

        var command = new Command("verify", "Poll until every package in scope is indexed on NuGet.org.")
        {
            plan, maxDuration, interval, feed, set, stage, expectedHash,
        };

        command.SetAction((parse, cancellationToken) =>
        {
            var planFile = parse.GetValue(plan)!;
            using var checker = new FlatContainerExistenceChecker(parse.GetValue(feed));

            return Verbs.VerifyAsync(
                console,
                new PackageAvailabilityProbe(checker),
                File.ReadAllText(planFile.FullName),
                TimeSpan.FromMinutes(parse.GetValue(maxDuration)),
                TimeSpan.FromSeconds(parse.GetValue(interval)),
                () => DateTimeOffset.UtcNow,
                Task.Delay,
                parse.GetValue(set),
                parse.GetValue(stage)?.FullName ?? planFile.DirectoryName,
                parse.GetValue(expectedHash),
                cancellationToken);
        });

        return command;
    }

    private static IProductConstructionServiceApi CreateApi(string? baseUri, string? token, string? managedIdentityId) =>
        baseUri is { Length: > 0 }
            ? PcsApiFactory.GetAuthenticated(baseUri, token!, managedIdentityId!, disableInteractiveAuth: true)
            : PcsApiFactory.GetAuthenticated(token!, managedIdentityId!, disableInteractiveAuth: true);
}
