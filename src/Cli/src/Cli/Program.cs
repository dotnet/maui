using System.CommandLine;
using System.CommandLine.Parsing;
using Microsoft.Maui.Cli.Commands.Screenshot;

namespace Microsoft.Maui.Cli;

public class Program
{
    public static int Main(string[] args)
    {
        var rootCommand = CreateRootCommand();
        ParseResult parseResult = rootCommand.Parse(args);
        return parseResult.Invoke();
    }

    internal static RootCommand CreateRootCommand()
    {
        var rootCommand = new RootCommand("dotnet-maui CLI - Tools for capturing screenshots, viewing logs, and inspecting .NET MAUI applications");

        // Add global options (recursive to all subcommands)
        rootCommand.Options.Add(CommonOptions.FrameworkOption);
        rootCommand.Options.Add(CommonOptions.DeviceOption);
        rootCommand.Options.Add(CommonOptions.ProjectOption);
        CommonOptions.FrameworkOption.Recursive = true;
        CommonOptions.DeviceOption.Recursive = true;
        CommonOptions.ProjectOption.Recursive = true;

        // Add commands
        rootCommand.Subcommands.Add(ScreenshotCommand.GetCommand());

        return rootCommand;
    }
}
