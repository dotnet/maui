namespace DotNet.Release;

internal static class PlanCommand
{
    public const string FileName = "plan.json";

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
}
