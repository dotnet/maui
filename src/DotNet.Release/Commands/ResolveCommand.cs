using System.CommandLine;
using Microsoft.DotNet.ProductConstructionService.Client;

namespace DotNet.Release;

internal static class ResolveCommand
{
    public static Command Build(TextWriter outputWriter, string toolVersion)
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
        var manifest = new Option<FileInfo>("--manifest")
        {
            Description = "Path for release-manifest.json.",
            Required = true,
        };

        var command = new Command("resolve", "Resolve and verify the BAR build.")
        {
            config, repo, commit, barId, barUri, token, managedIdentity, manifest,
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
                parse.GetValue(manifest)!.FullName,
                DateTimeOffset.UtcNow,
                toolVersion,
                cancellationToken);
        });

        return command;
    }

    public static async Task ExecuteAsync(TextWriter outputWriter, IBuildRegistry registry, string policyJson, string repository,
        string commit, int? barBuildId, string manifestPath, DateTimeOffset now, string toolVersion, CancellationToken cancellationToken)
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

        var manifest = new ReleaseManifest
        {
            ToolVersion = toolVersion,
            CreatedUtc = now.ToUniversalTime(),
            Source = new ReleaseSource
            {
                Repository = resolved.Repository,
                RepositoryUrl = resolved.RepositoryUrl,
                Commit = resolved.Commit,
                BarBuildId = resolved.BarBuildId,
                Workload = resolved.Workload,
                Channel = resolved.Channel,
            },
            WorkloadSet = null,
            Sets = [],
        };

        Directory.CreateDirectory(Path.GetDirectoryName(manifestPath)!);
        await File.WriteAllTextAsync(manifestPath, ReleaseManifestSerializer.Serialize(manifest), cancellationToken).ConfigureAwait(false);
        outputWriter.WriteLine();
        outputWriter.WriteLine($"Initialized {manifestPath}.");
    }

    private static IProductConstructionServiceApi CreateApi(string? baseUri, string? token, string? managedIdentityId) =>
        baseUri is { Length: > 0 }
            ? PcsApiFactory.GetAuthenticated(baseUri, token!, managedIdentityId!, disableInteractiveAuth: true)
            : PcsApiFactory.GetAuthenticated(token!, managedIdentityId!, disableInteractiveAuth: true);
}
