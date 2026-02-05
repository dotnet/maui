// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Maui.DevTools.Output;
using Microsoft.Maui.DevTools.Providers.Android;

namespace Microsoft.Maui.DevTools.Commands;

/// <summary>
/// Implementation of 'dotnet maui android' command group.
/// </summary>
public static class AndroidCommands
{
	public static Command Create()
	{
		var command = new Command("android", "Android SDK and device management");

		command.AddCommand(CreateBootstrapCommand());
		command.AddCommand(CreateJdkCommand());
		command.AddCommand(CreateSdkCommand());
		command.AddCommand(CreateAvdCommand());

		return command;
	}

	private static Command CreateBootstrapCommand()
	{
		var packagesOption = new Option<string[]>("--packages", "Additional SDK packages to install (comma-separated or multiple --packages flags)")
		{
			AllowMultipleArgumentsPerToken = true
		};
		// Support comma-separated values like: --packages "pkg1,pkg2,pkg3"
		packagesOption.AddValidator(result =>
		{
			// No validation needed, just allow the input
		});

		var command = new Command("bootstrap", "Set up Android development environment")
		{
			new Option<string>("--sdk-path", "Custom SDK installation path"),
			new Option<string>("--jdk-path", "Custom JDK installation path"),
			new Option<int>("--jdk-version", () => 17, "JDK version to install (17 or 21)"),
			packagesOption
		};

		command.SetHandler(async (InvocationContext context) =>
		{
			var androidProvider = Program.AndroidProvider;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var dryRun = context.ParseResult.GetValueForOption(GlobalOptions.DryRunOption);
			var sdkPath = context.ParseResult.GetValueForOption(
				(Option<string>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "sdk-path"));
			var jdkPath = context.ParseResult.GetValueForOption(
				(Option<string>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "jdk-path"));
			var jdkVersion = context.ParseResult.GetValueForOption(
				(Option<int>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "jdk-version"));
			var rawPackages = context.ParseResult.GetValueForOption(
				(Option<string[]>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "packages"));
			
			// Support comma-separated packages: "pkg1,pkg2,pkg3" becomes ["pkg1", "pkg2", "pkg3"]
			var packages = rawPackages?
				.SelectMany(p => p.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
				.ToArray();

			var formatter = useJson 
				? (IOutputFormatter)new JsonOutputFormatter(Console.Out) 
				: new ConsoleOutputFormatter(Console.Out);

			try
			{
				if (dryRun)
				{
					Console.WriteLine("[dry-run] Would bootstrap Android environment:");
					Console.WriteLine($"  JDK version: {jdkVersion}");
					Console.WriteLine($"  JDK path: {jdkPath ?? "(default)"}");
					Console.WriteLine($"  SDK path: {sdkPath ?? "(default)"}");
					if (packages?.Any() == true)
						Console.WriteLine($"  Extra packages: {string.Join(", ", packages)}");
					return;
				}

				await androidProvider.BootstrapAsync(
					sdkPath: sdkPath,
					jdkPath: jdkPath,
					jdkVersion: jdkVersion,
					additionalPackages: packages,
					cancellationToken: context.GetCancellationToken());

				if (useJson)
				{
					formatter.Write(new { success = true, message = "Android environment bootstrapped successfully" });
				}
				else
				{
					Console.WriteLine("✓ Android environment bootstrapped successfully");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});

		return command;
	}

	private static Command CreateJdkCommand()
	{
		var command = new Command("jdk", "Manage JDK installation");

		// jdk check
		var checkCommand = new Command("check", "Check JDK installation status");
		checkCommand.SetHandler(async (InvocationContext context) =>
		{
			var jdkManager = Program.JdkManager;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var formatter = useJson 
				? (IOutputFormatter)new JsonOutputFormatter(Console.Out) 
				: new ConsoleOutputFormatter(Console.Out);

			var healthCheck = await jdkManager.CheckHealthAsync(context.GetCancellationToken());

			if (useJson)
			{
				formatter.Write(healthCheck);
			}
			else
			{
				var statusIcon = healthCheck.Status == Models.CheckStatus.Ok ? "✓" : 
								 healthCheck.Status == Models.CheckStatus.Warning ? "⚠" : "✗";
				Console.WriteLine($"{statusIcon} {healthCheck.Message}");
				
				if (healthCheck.Details?.TryGetValue("path", out var path) == true)
					Console.WriteLine($"  Path: {path}");
			}
		});

		// jdk install
		var installCommand = new Command("install", "Install OpenJDK")
		{
			new Option<int>("--version", () => 17, "JDK version (17 or 21)"),
			new Option<string>("--path", "Installation path")
		};
		installCommand.SetHandler(async (InvocationContext context) =>
		{
			var jdkManager = Program.JdkManager;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var dryRun = context.ParseResult.GetValueForOption(GlobalOptions.DryRunOption);
			var version = context.ParseResult.GetValueForOption(
				(Option<int>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "version"));
			var path = context.ParseResult.GetValueForOption(
				(Option<string>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "path"));

			var formatter = useJson 
				? (IOutputFormatter)new JsonOutputFormatter(Console.Out) 
				: new ConsoleOutputFormatter(Console.Out);

			try
			{
				if (dryRun)
				{
					Console.WriteLine($"[dry-run] Would install OpenJDK {version}");
					return;
				}

				Console.WriteLine($"Installing OpenJDK {version}...");
				await jdkManager.InstallAsync(version, path, context.GetCancellationToken());

				if (useJson)
				{
					formatter.Write(new { success = true, version = version, path = jdkManager.DetectedJdkPath });
				}
				else
				{
					Console.WriteLine($"✓ OpenJDK {version} installed to {jdkManager.DetectedJdkPath}");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});

		// jdk list
		var listCommand = new Command("list", "List available JDK versions");
		listCommand.SetHandler((InvocationContext context) =>
		{
			var jdkManager = Program.JdkManager;
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);

			var versions = jdkManager.GetAvailableVersions().ToList();

			if (useJson)
			{
				var formatter = new JsonOutputFormatter(Console.Out);
				formatter.Write(new { versions = versions });
			}
			else
			{
				Console.WriteLine("Available JDK versions:");
				foreach (var v in versions)
				{
					var current = jdkManager.DetectedJdkVersion == v ? " (current)" : "";
					Console.WriteLine($"  {v}{current}");
				}
			}
		});

		command.AddCommand(checkCommand);
		command.AddCommand(installCommand);
		command.AddCommand(listCommand);

		return command;
	}

	private static Command CreateSdkCommand()
	{
		var command = new Command("sdk", "Manage Android SDK");

		// sdk check
		var checkCommand = new Command("check", "Check Android SDK installation status");
		checkCommand.SetHandler(async (InvocationContext context) =>
		{
			var androidProvider = Program.AndroidProvider;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var formatter = useJson 
				? (IOutputFormatter)new JsonOutputFormatter(Console.Out) 
				: new ConsoleOutputFormatter(Console.Out);

			var checks = await androidProvider.CheckHealthAsync(context.GetCancellationToken());
			var sdkCheck = checks.FirstOrDefault(c => c.Name == "Android SDK");

			if (sdkCheck != null)
			{
				if (useJson)
				{
					formatter.Write(sdkCheck);
				}
				else
				{
					var statusIcon = sdkCheck.Status == Models.CheckStatus.Ok ? "✓" : 
									 sdkCheck.Status == Models.CheckStatus.Warning ? "⚠" : "✗";
					Console.WriteLine($"{statusIcon} {sdkCheck.Message ?? "Android SDK"}");
					
					if (sdkCheck.Details?.TryGetValue("path", out var path) == true)
						Console.WriteLine($"  Path: {path}");
				}
			}
		});

		// sdk install
		var installCommand = new Command("install", "Install SDK packages")
		{
			new Argument<string[]>("packages", "SDK packages to install")
		};
		installCommand.SetHandler(async (InvocationContext context) =>
		{
			var androidProvider = Program.AndroidProvider;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var dryRun = context.ParseResult.GetValueForOption(GlobalOptions.DryRunOption);
			var packages = context.ParseResult.GetValueForArgument(
				(Argument<string[]>)context.ParseResult.CommandResult.Command.Arguments.First());

			var formatter = useJson 
				? (IOutputFormatter)new JsonOutputFormatter(Console.Out) 
				: new ConsoleOutputFormatter(Console.Out);

			try
			{
				if (dryRun)
				{
					Console.WriteLine($"[dry-run] Would install: {string.Join(", ", packages)}");
					return;
				}

				await androidProvider.InstallPackagesAsync(packages, true, context.GetCancellationToken());

				if (useJson)
				{
					formatter.Write(new { success = true, installed = packages });
				}
				else
				{
					Console.WriteLine($"✓ Installed: {string.Join(", ", packages)}");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});

		// sdk list
		var listCommand = new Command("list", "List installed SDK packages");
		listCommand.SetHandler(async (InvocationContext context) =>
		{
			var androidProvider = Program.AndroidProvider;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var formatter = useJson 
				? (IOutputFormatter)new JsonOutputFormatter(Console.Out) 
				: new ConsoleOutputFormatter(Console.Out);

			try
			{
				var packages = await androidProvider.GetInstalledPackagesAsync(context.GetCancellationToken());

				if (useJson)
				{
					formatter.Write(packages);
				}
				else
				{
					if (!packages.Any())
					{
						Console.WriteLine("No packages installed.");
						return;
					}

					Console.WriteLine("Installed packages:");
					foreach (var pkg in packages)
					{
						Console.WriteLine($"  {pkg.Path}");
					}
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});

		command.AddCommand(checkCommand);
		command.AddCommand(installCommand);
		command.AddCommand(listCommand);

		return command;
	}

	private static Command CreateAvdCommand()
	{
		var command = new Command("avd", "Manage Android Virtual Devices");

		// avd list
		var listCommand = new Command("list", "List available AVDs");
		listCommand.SetHandler(async (InvocationContext context) =>
		{
			var androidProvider = Program.AndroidProvider;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var formatter = useJson 
				? (IOutputFormatter)new JsonOutputFormatter(Console.Out) 
				: new ConsoleOutputFormatter(Console.Out);

			try
			{
				var avds = await androidProvider.GetAvdsAsync(context.GetCancellationToken());

				if (useJson)
				{
					formatter.Write(avds);
				}
				else
				{
					if (!avds.Any())
					{
						Console.WriteLine("No AVDs found.");
						return;
					}

					Console.WriteLine("Available AVDs:");
					foreach (var avd in avds)
					{
						Console.WriteLine($"  {avd.Name} ({avd.Target ?? avd.SystemImage ?? "unknown"})");
					}
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});

		// avd create
		var createCommand = new Command("create", "Create a new AVD")
		{
			new Argument<string>("name", "AVD name"),
			new Option<string>("--package", "System image package (auto-detects most recent if not specified)"),
			new Option<string>("--device", "Device definition")
		};
		createCommand.SetHandler(async (InvocationContext context) =>
		{
			var androidProvider = Program.AndroidProvider;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var dryRun = context.ParseResult.GetValueForOption(GlobalOptions.DryRunOption);
			var name = context.ParseResult.GetValueForArgument(
				(Argument<string>)context.ParseResult.CommandResult.Command.Arguments.First());
			var package = context.ParseResult.GetValueForOption(
				(Option<string>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "package"));
			var device = context.ParseResult.GetValueForOption(
				(Option<string>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "device"));

			var formatter = useJson 
				? (IOutputFormatter)new JsonOutputFormatter(Console.Out) 
				: new ConsoleOutputFormatter(Console.Out);

			try
			{
				// Auto-detect system image if not provided
				if (string.IsNullOrEmpty(package))
				{
					package = await androidProvider.GetMostRecentSystemImageAsync(context.GetCancellationToken());
					if (string.IsNullOrEmpty(package))
					{
						throw new InvalidOperationException(
							"No system images installed. Install one first with: dotnet maui android sdk install \"system-images;android-35;google_apis;arm64-v8a\"");
					}
					
					if (!useJson)
					{
						Console.WriteLine($"Auto-detected system image: {package}");
					}
				}

				if (dryRun)
				{
					Console.WriteLine($"[dry-run] Would create AVD: {name}");
					Console.WriteLine($"  Package: {package}");
					Console.WriteLine($"  Device: {device ?? "(default: pixel_6)"}");
					return;
				}

				await androidProvider.CreateAvdAsync(name, device ?? "pixel_6", package, false, context.GetCancellationToken());

				if (useJson)
				{
					formatter.Write(new { success = true, name = name, package = package });
				}
				else
				{
					Console.WriteLine($"✓ Created AVD: {name}");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});

		// avd start
		var startCommand = new Command("start", "Start an AVD")
		{
			new Argument<string>("name", "AVD name to start"),
			new Option<bool>("--cold-boot", "Perform a cold boot"),
			new Option<bool>("--wait", "Wait for boot completion")
		};
		startCommand.SetHandler(async (InvocationContext context) =>
		{
			var androidProvider = Program.AndroidProvider;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var dryRun = context.ParseResult.GetValueForOption(GlobalOptions.DryRunOption);
			var name = context.ParseResult.GetValueForArgument(
				(Argument<string>)context.ParseResult.CommandResult.Command.Arguments.First());
			var coldBoot = context.ParseResult.GetValueForOption(
				(Option<bool>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "cold-boot"));
			var wait = context.ParseResult.GetValueForOption(
				(Option<bool>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "wait"));

			var formatter = useJson 
				? (IOutputFormatter)new JsonOutputFormatter(Console.Out) 
				: new ConsoleOutputFormatter(Console.Out);

			try
			{
				if (dryRun)
				{
					Console.WriteLine($"[dry-run] Would start AVD: {name}");
					return;
				}

				Console.WriteLine($"Starting {name}...");
				await androidProvider.StartAvdAsync(name, coldBoot, wait, context.GetCancellationToken());

				if (useJson)
				{
					formatter.Write(new { success = true, name = name, status = "started" });
				}
				else
				{
					Console.WriteLine($"✓ Started AVD: {name}");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});

		command.AddCommand(listCommand);
		command.AddCommand(createCommand);
		command.AddCommand(startCommand);

		return command;
	}
}
