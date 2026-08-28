using System.CommandLine;
using System.Reflection;

namespace DotNet.Release;

internal static class Program
{
    private static string ToolVersion =>
        typeof(Program).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
        ?? "0.0.0";

    public static Task<int> Main(string[] args) =>
        BuildRootCommand().Parse(args).InvokeAsync();

    internal static RootCommand BuildRootCommand()
    {
        var console = new SystemConsole();
        var root = new RootCommand(
            "Prepares and verifies a .NET package release from a BAR build. " +
            "1ES.PublishNuget performs every package upload.");

        root.Subcommands.Add(PlanCommand.Build(console, ToolVersion));
        root.Subcommands.Add(StageCommand.Build(console, ToolVersion));
        root.Subcommands.Add(PrunePublishedCommand.Build(console));
        root.Subcommands.Add(VerifyCommand.Build(console));

        return root;
    }
}
