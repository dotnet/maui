using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.DotNet.ProductConstructionService.Client;

namespace DotNet.Release;

internal static class ResolveCommand
{
    private static readonly JsonSerializerOptions OutputJsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

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
        var build = new Option<string>("--build")
        {
            Description = "Positive BAR build ID or full 40-character commit SHA.",
            Required = true,
        };
        var output = new Option<FileInfo>("--output")
        {
            Description = "Path for the verified resolved-build JSON.",
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

        var command = new Command("resolve", "Resolve and verify a BAR build.")
        {
            config, repo, build, output, barUri, token, managedIdentity,
        };

        command.SetAction(async (parse, cancellationToken) =>
        {
            var api = CreateApi(parse.GetValue(barUri), parse.GetValue(token), parse.GetValue(managedIdentity));
            var resolved = await ExecuteAsync(
                outputWriter,
                MaestroBuildRegistry.Create(api),
                File.ReadAllText(parse.GetValue(config)!.FullName),
                parse.GetValue(repo)!,
                parse.GetValue(build)!,
                cancellationToken).ConfigureAwait(false);

            var outputPath = parse.GetValue(output)!.FullName;
            await File.WriteAllTextAsync(outputPath, SerializeOutput(resolved), cancellationToken).ConfigureAwait(false);
            outputWriter.WriteLine();
            outputWriter.WriteLine($"Wrote {outputPath}.");
        });

        return command;
    }

    public static async Task<ResolvedBuild> ExecuteAsync(
        TextWriter outputWriter,
        IBuildRegistry registry,
        string policyJson,
        string repository,
        string buildIdentifier,
        CancellationToken cancellationToken)
    {
        var policy = ReleasePolicy.Parse(policyJson);
        var repositoryId = RepositoryId.Parse(repository);
        var repositoryPolicy = policy.GetRepository(repositoryId);
        var request = ParseRequest(repositoryId, buildIdentifier);
        var candidates = request.BarBuildId is { } barBuildId
            ? await registry.GetBuildAsync(barBuildId, cancellationToken).ConfigureAwait(false)
            : await registry.GetBuildsAsync(request.Commit!, cancellationToken).ConfigureAwait(false);

        var resolved = BuildResolver.Resolve(request, repositoryPolicy, candidates);
        ReleaseOutput.WriteResolvedBuild(outputWriter, resolved);
        return resolved;
    }

    internal static string SerializeOutput(ResolvedBuild resolved) =>
        JsonSerializer.Serialize(resolved, OutputJsonOptions);

    private static ReleaseRequest ParseRequest(RepositoryId repository, string buildIdentifier)
    {
        var value = buildIdentifier?.Trim();
        if (value is { Length: 40 } && value.All(Uri.IsHexDigit))
        {
            return new ReleaseRequest(repository, value, BarBuildId: null);
        }

        if (value is { Length: > 0 } && value.All(char.IsAsciiDigit) && int.TryParse(value, out var barBuildId) && barBuildId > 0)
        {
            return new ReleaseRequest(repository, Commit: null, BarBuildId: barBuildId);
        }

        throw new DotNetReleaseException(
            $"Build identifier '{buildIdentifier}' must be a positive BAR build ID or full 40-character commit SHA.");
    }

    private static IProductConstructionServiceApi CreateApi(string? baseUri, string? token, string? managedIdentityId) =>
        baseUri is { Length: > 0 }
            ? PcsApiFactory.GetAuthenticated(baseUri, token!, managedIdentityId!, disableInteractiveAuth: true)
            : PcsApiFactory.GetAuthenticated(token!, managedIdentityId!, disableInteractiveAuth: true);
}
