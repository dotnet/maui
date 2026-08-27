using DotNet.Release.Core;

namespace DotNet.Release.Cli;

/// <summary>Process exit codes. Stable, so pipeline steps can distinguish outcomes.</summary>
public static class ExitCodes
{
    public const int Success = 0;

    /// <summary>A release rule failed closed. The log carries the error codes.</summary>
    public const int ReleaseError = 1;

    /// <summary>Bad invocation: missing file, unusable argument.</summary>
    public const int UsageError = 2;
}

/// <summary>Console output, kept behind an interface so verbs are testable.</summary>
public interface IReleaseConsole
{
    void WriteLine(string message);

    void WriteError(string message);
}

internal sealed class SystemConsole : IReleaseConsole
{
    public void WriteLine(string message) => Console.Out.WriteLine(message);

    public void WriteError(string message) => Console.Error.WriteLine(message);
}

/// <summary>Shared reporting so every verb fails the same way.</summary>
internal static class ConsoleReporting
{
    /// <summary>
    /// Prints every error with its stable code.
    /// </summary>
    /// <remarks>
    /// All of them, not just the first: validation collects everything it found in one pass,
    /// and a release operator fixing packages one round-trip at a time is exactly the
    /// experience the current pipeline already has.
    /// </remarks>
    public static int Fail(IReleaseConsole console, IReadOnlyList<ReleaseError> errors)
    {
        foreach (var error in errors)
        {
            console.WriteError($"error {error.Code}: {error.Message}");
        }

        return ExitCodes.ReleaseError;
    }

    public static void WriteSummary(IReleaseConsole console, ReleasePlan plan)
    {
        console.WriteLine($"Repository      : {plan.Source.Repository}");
        console.WriteLine($"Commit          : {plan.Source.Commit}");
        console.WriteLine($"BAR build       : {plan.Source.BarBuildId}");
        console.WriteLine($"Identity source : {plan.Source.RepositoryOrigin}");

        if (plan.Source.Channel is { } channel)
        {
            console.WriteLine($"Channel         : {channel.Name} ({channel.Id})");
        }

        if (plan.WorkloadSet is { } workloadSet)
        {
            console.WriteLine($"Workload set    : {workloadSet.Channel} -> {workloadSet.Feed} (.NET {workloadSet.Band})");
        }

        foreach (var set in plan.Sets.OrderBy(s => s.Order))
        {
            console.WriteLine(string.Empty);
            console.WriteLine($"{set.Name} ({set.Packages.Count}) -> artifact '{set.ArtifactName}'");

            foreach (var package in set.Packages)
            {
                console.WriteLine($"  {package.Id} {package.Version}");
            }
        }
    }
}
