using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;

namespace DotNet.Release;

internal static class Program
{
    private static string ToolVersion => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "0.0.0";

    public static async Task<int> Main(string[] args)
    {
        try
        {
            var invocationConfiguration = new InvocationConfiguration
            {
                EnableDefaultExceptionHandler = false,
            };

            return await BuildRootCommand().Parse(args).InvokeAsync(invocationConfiguration).ConfigureAwait(false);
        }
        catch (DotNetReleaseException ex)
        {
            await Console.Error.WriteLineAsync($"error: {ex.Message}").ConfigureAwait(false);
            return 1;
        }
    }

    internal static RootCommand BuildRootCommand()
    {
        var root = new RootCommand("Prepares and verifies a .NET package release from a BAR build. " + "1ES.PublishNuget performs every package upload.");

        root.Subcommands.Add(ResolveCommand.Build(Console.Out));
        root.Subcommands.Add(StageCommand.Build(Console.Out, ToolVersion));
        root.Subcommands.Add(PrunePublishedCommand.Build(Console.Out));
        root.Subcommands.Add(VerifyCommand.Build(Console.Out));

        return root;
    }
}
