// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Client.Commands;
using Microsoft.Maui.Client.Output;
using Microsoft.Maui.Client.Providers.Android;
using Microsoft.Maui.Client.Services;
using Microsoft.Maui.Client.Utils;

namespace Microsoft.Maui.Client;

public class Program
{
	// Thread-safe lazy initialization of the service provider
	static readonly object _lock = new();
	static IServiceProvider? _serviceProvider;

	/// <summary>
	/// Gets or sets the service provider. Thread-safe initialization with lock.
	/// Can be overridden for testing (set before first access).
	/// </summary>
	public static IServiceProvider Services
	{
		get
		{
			lock (_lock)
			{
				return _serviceProvider ??= ServiceConfiguration.CreateServiceProvider();
			}
		}
		set { lock (_lock) { _serviceProvider = value; } }
	}

	// Convenience accessors for services
	internal static IAndroidProvider AndroidProvider => Services.GetRequiredService<IAndroidProvider>();
	internal static IDoctorService DoctorService => Services.GetRequiredService<IDoctorService>();
	internal static IDeviceManager DeviceManager => Services.GetRequiredService<IDeviceManager>();
	internal static IJdkManager JdkManager => Services.GetRequiredService<IJdkManager>();

	public static async Task<int> Main(string[] args)
	{
		var rootCommand = BuildRootCommand();

		// Handle help rendering with Spectre.Console before the parser runs
		if (TryShowSpectreHelp(args, rootCommand))
			return 0;

		var parser = new CommandLineBuilder(rootCommand)
			.UseHelp()
			.UseEnvironmentVariableDirective()
			.UseParseDirective()
			.UseSuggestDirective()
			.RegisterWithDotnetSuggest()
			.UseTypoCorrections()
			.UseParseErrorReporting()
			.UseVersionOption()
			.UseExceptionHandler((exception, context) =>
			{
				var formatter = GetFormatter(context);
				var isCi = context.ParseResult.GetValueForOption(GlobalOptions.CiOption);

				formatter.WriteError(exception);
				context.ExitCode = 1;

				// In CI mode, fail fast
				if (isCi)
				{
					Environment.Exit(1);
				}
			})
			.Build();

		return await parser.InvokeAsync(args);
	}

	static RootCommand BuildRootCommand()
	{
		var rootCommand = new RootCommand("MAUI Development Tools - Device management and environment setup")
		{
			Name = "maui"
		};

		// Global options
		rootCommand.AddGlobalOption(GlobalOptions.JsonOption);
		rootCommand.AddGlobalOption(GlobalOptions.VerboseOption);
		rootCommand.AddGlobalOption(GlobalOptions.DryRunOption);
		rootCommand.AddGlobalOption(GlobalOptions.CiOption);

		// Top-level commands (per spec)
		rootCommand.AddCommand(DoctorCommand.Create());
		rootCommand.AddCommand(DeviceCommand.Create());
		rootCommand.AddCommand(VersionCommand.Create());

		// Platform-specific command groups
		rootCommand.AddCommand(AndroidCommands.Create());

		return rootCommand;
	}

	internal static IOutputFormatter GetFormatter(InvocationContext context)
	{
		var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
		var verbose = context.ParseResult.GetValueForOption(GlobalOptions.VerboseOption);

		return useJson
			? new JsonOutputFormatter(Console.Out)
			: new SpectreOutputFormatter(verbose: verbose);
	}

	internal static bool IsVerbose(InvocationContext context) =>
		context.ParseResult.GetValueForOption(GlobalOptions.VerboseOption);

	internal static bool IsDryRun(InvocationContext context) =>
		context.ParseResult.GetValueForOption(GlobalOptions.DryRunOption);

	internal static bool IsCiMode(InvocationContext context) =>
		context.ParseResult.GetValueForOption(GlobalOptions.CiOption);

	static readonly string[] s_helpAliases = ["--help", "-h", "-?", "/h", "/?"];

	/// <summary>
	/// Checks if help was requested and renders colorized help using Spectre.Console.
	/// Returns true if help was handled, false otherwise.
	/// </summary>
	static bool TryShowSpectreHelp(string[] args, RootCommand rootCommand)
	{
		if (!args.Any(a => s_helpAliases.Contains(a, StringComparer.OrdinalIgnoreCase)))
			return false;

		// Resolve which command the user is asking help for
		var argsWithoutHelp = args.Where(a => !s_helpAliases.Contains(a, StringComparer.OrdinalIgnoreCase)).ToArray();
		var command = ResolveCommand(argsWithoutHelp, rootCommand);
		SpectreHelpBuilder.WriteHelp(command);
		return true;
	}

	/// <summary>
	/// Walks the command tree to find the deepest matching command for the given args.
	/// </summary>
	static Command ResolveCommand(string[] args, RootCommand rootCommand)
	{
		Command current = rootCommand;
		foreach (var arg in args)
		{
			var sub = current.Subcommands.FirstOrDefault(c =>
				string.Equals(c.Name, arg, StringComparison.OrdinalIgnoreCase));
			if (sub == null)
				break;
			current = sub;
		}
		return current;
	}

	/// <summary>
	/// Resets the service provider. Used for testing.
	/// </summary>
	internal static void ResetServices()
	{
		_serviceProvider = null;
	}
}

/// <summary>
/// Global options available for all commands.
/// </summary>
public static class GlobalOptions
{
	public static readonly Option<bool> JsonOption = new(
		aliases: new[] { "--json" },
		description: "Output in JSON format for machine consumption")
	{
		Arity = ArgumentArity.ZeroOrOne
	};

	public static readonly Option<bool> VerboseOption = new(
		aliases: new[] { "-v", "--verbose" },
		description: "Enable verbose output")
	{
		Arity = ArgumentArity.ZeroOrOne
	};

	public static readonly Option<bool> DryRunOption = new(
		aliases: new[] { "--dry-run" },
		description: "Show what would be done without making changes")
	{
		Arity = ArgumentArity.ZeroOrOne
	};

	public static readonly Option<bool> CiOption = new(
		aliases: new[] { "--ci" },
		description: "CI mode - non-interactive, fail on first error")
	{
		Arity = ArgumentArity.ZeroOrOne
	};
}
