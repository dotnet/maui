// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DevTools.Commands;
using Microsoft.Maui.DevTools.Output;
using Microsoft.Maui.DevTools.Providers.Android;
using Microsoft.Maui.DevTools.Providers.Apple;
using Microsoft.Maui.DevTools.Services;
using Microsoft.Maui.DevTools.Utils;

namespace Microsoft.Maui.DevTools;

public class Program
{
	// Service provider for dependency injection
	private static IServiceProvider? _serviceProvider;

	/// <summary>
	/// Gets or sets the service provider. Can be overridden for testing.
	/// </summary>
	public static IServiceProvider Services
	{
		get => _serviceProvider ??= ServiceConfiguration.CreateServiceProvider();
		set => _serviceProvider = value;
	}

	// Convenience accessors for services
	internal static IAndroidProvider AndroidProvider => Services.GetRequiredService<IAndroidProvider>();
	internal static IAppleProvider AppleProvider => Services.GetRequiredService<IAppleProvider>();
	internal static IDoctorService DoctorService => Services.GetRequiredService<IDoctorService>();
	internal static IDeviceManager DeviceManager => Services.GetRequiredService<IDeviceManager>();
	internal static IJdkManager JdkManager => Services.GetRequiredService<IJdkManager>();

	public static async Task<int> Main(string[] args)
	{
		var rootCommand = BuildRootCommand();

		var parser = new CommandLineBuilder(rootCommand)
			.UseDefaults()
			.UseExceptionHandler((exception, context) =>
			{
				var useJson = context.ParseResult.FindResultFor(GlobalOptions.JsonOption) is not null &&
							  context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
				var isCi = context.ParseResult.GetValueForOption(GlobalOptions.CiOption);
				
				var formatter = useJson 
					? (IOutputFormatter)new JsonOutputFormatter(Console.Out) 
					: new ConsoleOutputFormatter(Console.Out);
				
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

	private static RootCommand BuildRootCommand()
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
		rootCommand.AddCommand(ScreenshotCommand.Create());  // Top-level per spec
		rootCommand.AddCommand(LogsCommand.Create());        // Top-level per spec
		rootCommand.AddCommand(DeviceCommand.Create());
		rootCommand.AddCommand(VersionCommand.Create());

		// Platform-specific command groups
		rootCommand.AddCommand(AndroidCommands.Create());
		
		// Only add Apple commands on macOS
		if (PlatformDetector.IsMacOS)
		{
			rootCommand.AddCommand(AppleCommands.Create(AppleProvider, GetFormatter));
		}

		return rootCommand;
	}

	internal static IOutputFormatter GetFormatter(InvocationContext context)
	{
		var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
		var verbose = context.ParseResult.GetValueForOption(GlobalOptions.VerboseOption);
		
		return useJson 
			? new JsonOutputFormatter(Console.Out) 
			: new ConsoleOutputFormatter(Console.Out, verbose);
	}

	internal static bool IsVerbose(InvocationContext context) =>
		context.ParseResult.GetValueForOption(GlobalOptions.VerboseOption);

	internal static bool IsDryRun(InvocationContext context) =>
		context.ParseResult.GetValueForOption(GlobalOptions.DryRunOption);

	internal static bool IsCiMode(InvocationContext context) =>
		context.ParseResult.GetValueForOption(GlobalOptions.CiOption);

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
