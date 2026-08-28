using System.CommandLine;
using Microsoft.DotNet.ProductConstructionService.Client;

namespace DotNet.Release;

internal static class PlanCommand
{
    public const string FileName = "plan.json";

    public static Command Build(IReleaseConsole console, string toolVersion)
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
        var commit = new Option<string>("--commit")
        {
            Description = "Exact source commit registered in BAR.",
            Required = true,
        };
        var barId = new Option<int?>("--bar-id")
        {
            Description = "BAR build ID for builds without a GitHub URL.",
        };
        var output = new Option<DirectoryInfo>("--out")
        {
            Description = "Output directory.",
            Required = true,
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

        var command = new Command(
            "plan",
            "Resolve and verify the BAR build, then write plan.json.")
        {
            config, repo, commit, barId, output, barUri, token, managedIdentity,
        };

        command.SetAction((parse, cancellationToken) =>
        {
            var api = CreateApi(
                parse.GetValue(barUri),
                parse.GetValue(token),
                parse.GetValue(managedIdentity));

            return ExecuteAsync(
                console,
                MaestroBuildRegistry.Create(api),
                File.ReadAllText(parse.GetValue(config)!.FullName),
                parse.GetValue(repo)!,
                parse.GetValue(commit)!,
                parse.GetValue(barId),
                parse.GetValue(output)!.FullName,
                DateTimeOffset.UtcNow,
                toolVersion,
                cancellationToken);
        });

        return command;
    }

    public static async Task<int> ExecuteAsync(
        IReleaseConsole console,
        IBuildRegistry registry,
        string policyJson,
        string repository,
        string commit,
        int? barBuildId,
        string outputDirectory,
        DateTimeOffset now,
        string toolVersion,
        CancellationToken cancellationToken)
    {
        var policy = ReleasePolicy.Parse(policyJson);
        if (policy.IsFailure)
        {
            return ConsoleReporting.Fail(console, policy.Errors);
        }

        var repositoryId = RepositoryId.Parse(repository);
        if (repositoryId.IsFailure)
        {
            return ConsoleReporting.Fail(console, repositoryId.Errors);
        }

        var repositoryPolicy = policy.Value.GetRepository(repositoryId.Value);
        if (repositoryPolicy.IsFailure)
        {
            return ConsoleReporting.Fail(console, repositoryPolicy.Errors);
        }

        var request = new ReleaseRequest(repositoryId.Value, commit, barBuildId);
        var candidates = barBuildId is { } id
            ? await registry.GetBuildAsync(id, cancellationToken).ConfigureAwait(false)
            : await registry
                .GetBuildsAsync(repositoryId.Value, commit, cancellationToken)
                .ConfigureAwait(false);

        var resolved = BuildResolver.Resolve(
            request,
            repositoryPolicy.Value,
            candidates,
            now,
            toolVersion);
        if (resolved.IsFailure)
        {
            return ConsoleReporting.Fail(console, resolved.Errors);
        }

        Directory.CreateDirectory(outputDirectory);
        var planPath = Path.Combine(outputDirectory, FileName);
        await File
            .WriteAllTextAsync(
                planPath,
                ReleasePlanSerializer.Serialize(resolved.Value),
                cancellationToken)
            .ConfigureAwait(false);

        console.WriteLine(
            $"Resolved BAR build {resolved.Value.BarBuildId} for " +
            $"{resolved.Value.Repository} at {resolved.Value.Commit}.");
        console.WriteLine($"Identity established from: {resolved.Value.RepositoryOrigin}.");
        console.WriteLine($"Wrote {planPath}.");
        console.WriteLine(AzurePipelineCommand.SetBarId(resolved.Value.BarBuildId));
        console.WriteLine(AzurePipelineCommand.SetIsWorkload(resolved.Value.Workload));

        return ExitCodes.Success;
    }

    private static IProductConstructionServiceApi CreateApi(
        string? baseUri,
        string? token,
        string? managedIdentityId) =>
        baseUri is { Length: > 0 }
            ? PcsApiFactory.GetAuthenticated(
                baseUri,
                token!,
                managedIdentityId!,
                disableInteractiveAuth: true)
            : PcsApiFactory.GetAuthenticated(
                token!,
                managedIdentityId!,
                disableInteractiveAuth: true);
}
