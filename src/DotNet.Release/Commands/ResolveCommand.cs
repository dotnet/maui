using System.CommandLine;
using System.Text.Json;
using Microsoft.DotNet.ProductConstructionService.Client;

namespace DotNet.Release;

internal static class ResolveCommand
{
    private static readonly JsonSerializerOptions ResultJsonOptions = new() { WriteIndented = true };

    public static Command Build(TextWriter outputWriter)
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
        var result = new Option<FileInfo>("--result")
        {
            Description = "Path for the transient resolved-build JSON result.",
            Required = true,
        };

        var command = new Command("resolve", "Resolve and verify the BAR build.")
        {
            config, repo, commit, barId, barUri, token, managedIdentity, result,
        };

        command.SetAction((parse, cancellationToken) =>
        {
            var api = CreateApi(parse.GetValue(barUri), parse.GetValue(token), parse.GetValue(managedIdentity));

            return ExecuteAsync(
                outputWriter,
                MaestroBuildRegistry.Create(api),
                File.ReadAllText(parse.GetValue(config)!.FullName),
                parse.GetValue(repo)!,
                parse.GetValue(commit)!,
                parse.GetValue(barId),
                parse.GetValue(result)!.FullName,
                cancellationToken);
        });

        return command;
    }

    public static async Task ExecuteAsync(TextWriter outputWriter, IBuildRegistry registry, string policyJson, string repository,
        string commit, int? barBuildId, string resultPath, CancellationToken cancellationToken)
    {
        var policy = ReleasePolicy.Parse(policyJson);
        var repositoryId = RepositoryId.Parse(repository);
        var repositoryPolicy = policy.GetRepository(repositoryId);

        var request = new ReleaseRequest(repositoryId, commit, barBuildId);
        var candidates = barBuildId is { } id
            ? await registry.GetBuildAsync(id, cancellationToken).ConfigureAwait(false)
            : await registry.GetBuildsAsync(repositoryId, commit, cancellationToken).ConfigureAwait(false);

        var resolved = BuildResolver.Resolve(request, repositoryPolicy, candidates);

        ReleaseOutput.WriteResolvedBuild(outputWriter, resolved);

        var resultJson = JsonSerializer.Serialize(new ResolveResult(resolved.BarBuildId, resolved.Workload), ResultJsonOptions);
        await File.WriteAllTextAsync(resultPath, resultJson, cancellationToken).ConfigureAwait(false);
    }

    private static IProductConstructionServiceApi CreateApi(string? baseUri, string? token, string? managedIdentityId) =>
        baseUri is { Length: > 0 }
            ? PcsApiFactory.GetAuthenticated(baseUri, token!, managedIdentityId!, disableInteractiveAuth: true)
            : PcsApiFactory.GetAuthenticated(token!, managedIdentityId!, disableInteractiveAuth: true);
}
