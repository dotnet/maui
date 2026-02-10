// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Maui.DevTools.Output;
using Microsoft.Maui.DevTools.Providers.Android;
using Microsoft.Maui.DevTools.Utils;

namespace Microsoft.Maui.DevTools.Commands;

/// <summary>
/// Implementation of 'dotnet maui android' command group.
/// </summary>
public static class AndroidCommands
{
	public static Command Create()
	{
		var command = new Command("android", "Android SDK and device management");

		command.AddCommand(CreateInstallCommand());
		command.AddCommand(CreateJdkCommand());
		command.AddCommand(CreateSdkCommand());
		command.AddCommand(CreateEmulatorCommand());

		return command;
	}

	private static Command CreateInstallCommand()
	{
		var packagesOption = new Option<string[]>("--packages", "Additional SDK packages to install (comma-separated or multiple --packages flags)")
		{
			AllowMultipleArgumentsPerToken = true
		};

		var command = new Command("install", "Set up Android development environment")
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

			// Create progress reporter
			var progress = new Progress<string>(message =>
			{
				if (useJson)
				{
					formatter.WriteProgress(message);
				}
				else
				{
					Console.WriteLine(message);
				}
			});

			try
			{
				if (dryRun)
				{
					Console.WriteLine("[dry-run] Would install Android environment:");
					Console.WriteLine($"  JDK version: {jdkVersion}");
					Console.WriteLine($"  JDK path: {jdkPath ?? "(default)"}");
					Console.WriteLine($"  SDK path: {sdkPath ?? "(default)"}");
					if (packages?.Any() == true)
						Console.WriteLine($"  Extra packages: {string.Join(", ", packages)}");
					return;
				}

				await androidProvider.InstallAsync(
					sdkPath: sdkPath,
					jdkPath: jdkPath,
					jdkVersion: jdkVersion,
					additionalPackages: packages,
					progress: progress,
					cancellationToken: context.GetCancellationToken());

				if (useJson)
				{
					formatter.Write(new { success = true, message = "Android environment installed successfully" });
				}
				else
				{
					Console.WriteLine("✓ Android environment installed successfully");
				}
			}
			catch (UnauthorizedAccessException) when (PlatformDetector.IsWindows)
			{
				if (!useJson)
					Console.WriteLine("Administrator access required. Requesting elevation...");

				if (ProcessRunner.RelaunchElevated())
				{
					if (useJson)
					{
						formatter.Write(new { success = true, message = "Android environment installed successfully (elevated)" });
					}
					else
					{
						Console.WriteLine("✓ Android environment installed successfully (elevated)");
					}
				}
				else
				{
					var error = new Exception("Elevation was cancelled or failed. Run this command from an administrator terminal.");
					formatter.WriteError(error);
					context.ExitCode = 1;
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
		var listCommand = new Command("list", "List SDK packages")
		{
			new Option<bool>("--available", "List packages available for installation"),
			new Option<bool>("--all", "List both installed and available packages")
		};
		listCommand.SetHandler(async (InvocationContext context) =>
		{
			var androidProvider = Program.AndroidProvider;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var showAvailable = context.ParseResult.GetValueForOption(
				(Option<bool>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "available"));
			var showAll = context.ParseResult.GetValueForOption(
				(Option<bool>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "all"));
			
			var formatter = useJson 
				? (IOutputFormatter)new JsonOutputFormatter(Console.Out) 
				: new ConsoleOutputFormatter(Console.Out);

			try
			{
				var packages = new List<SdkPackage>();
				
				if (showAll)
				{
					// Get both installed and available
					var installed = await androidProvider.GetInstalledPackagesAsync(context.GetCancellationToken());
					var available = await androidProvider.GetAvailablePackagesAsync(context.GetCancellationToken());
					packages.AddRange(installed);
					packages.AddRange(available);
				}
				else if (showAvailable)
				{
					packages = await androidProvider.GetAvailablePackagesAsync(context.GetCancellationToken());
				}
				else
				{
					packages = await androidProvider.GetInstalledPackagesAsync(context.GetCancellationToken());
				}

				if (useJson)
				{
					formatter.Write(packages);
				}
				else
				{
					if (!packages.Any())
					{
						Console.WriteLine(showAvailable ? "No packages available." : "No packages installed.");
						return;
					}

					if (showAll)
					{
						Console.WriteLine("Installed packages:");
						foreach (var pkg in packages.Where(p => p.IsInstalled))
						{
							Console.WriteLine($"  {pkg.Path,-50} {pkg.Version}");
						}
						Console.WriteLine();
						Console.WriteLine("Available packages:");
						foreach (var pkg in packages.Where(p => !p.IsInstalled))
						{
							Console.WriteLine($"  {pkg.Path,-50} {pkg.Version}");
						}
					}
					else
					{
						Console.WriteLine(showAvailable ? "Available packages:" : "Installed packages:");
						foreach (var pkg in packages)
						{
							Console.WriteLine($"  {pkg.Path,-50} {pkg.Version}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});

		// sdk accept-licenses
		var acceptLicensesCommand = new Command("accept-licenses", "Accept Android SDK licenses interactively");
		acceptLicensesCommand.SetHandler(async (InvocationContext context) =>
		{
			var androidProvider = Program.AndroidProvider;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var formatter = useJson 
				? (IOutputFormatter)new JsonOutputFormatter(Console.Out) 
				: new ConsoleOutputFormatter(Console.Out);

			try
			{
				// Check if licenses already accepted
				var accepted = await androidProvider.AreLicensesAcceptedAsync(context.GetCancellationToken());
				if (accepted)
				{
					if (useJson)
					{
						formatter.Write(new { 
							success = true, 
							status = "already_accepted",
							message = "SDK licenses are already accepted" 
						});
					}
					else
					{
						Console.WriteLine("✓ SDK licenses are already accepted");
					}
					return;
				}

				// Get the command for interactive license acceptance
				var licenseCommand = androidProvider.GetLicenseAcceptanceCommand();
				if (licenseCommand == null)
				{
					if (useJson)
					{
						formatter.Write(new { 
							success = false, 
							status = "sdk_not_found",
							message = "Android SDK not found. Run 'dotnet maui android install' first." 
						});
					}
					else
					{
						Console.WriteLine("✗ Android SDK not found. Run 'dotnet maui android install' first.");
					}
					context.ExitCode = 1;
					return;
				}

				if (useJson)
				{
					// For IDE integration: return the command to run in a terminal
					formatter.Write(new { 
						success = true, 
						status = "requires_interaction",
						message = "Run the following command in a terminal to accept licenses interactively",
						command = licenseCommand.Value.Command,
						arguments = licenseCommand.Value.Arguments,
						full_command = $"{licenseCommand.Value.Command} {licenseCommand.Value.Arguments}"
					});
				}
				else
				{
					// For CLI: run interactively
					Console.WriteLine("Starting interactive license acceptance...");
					Console.WriteLine("Review each license and type 'y' to accept.\n");
					
					// Run sdkmanager --licenses interactively (inherits stdin/stdout)
					var processInfo = new System.Diagnostics.ProcessStartInfo
					{
						FileName = licenseCommand.Value.Command,
						Arguments = licenseCommand.Value.Arguments,
						UseShellExecute = false,
						RedirectStandardInput = false,
						RedirectStandardOutput = false,
						RedirectStandardError = false
					};
					
					// Set environment variables for JDK
					var jdkPath = androidProvider.JdkPath;
					if (!string.IsNullOrEmpty(jdkPath))
					{
						processInfo.Environment["JAVA_HOME"] = jdkPath;
						var currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";
						processInfo.Environment["PATH"] = $"{Path.Combine(jdkPath, "bin")}{Path.PathSeparator}{currentPath}";
					}
					
					using var process = System.Diagnostics.Process.Start(processInfo);
					if (process != null)
					{
						await process.WaitForExitAsync(context.GetCancellationToken());
						
						if (process.ExitCode == 0)
						{
							Console.WriteLine("\n✓ License acceptance completed");
						}
						else
						{
							Console.WriteLine($"\n⚠ License acceptance exited with code {process.ExitCode}");
						}
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
		command.AddCommand(acceptLicensesCommand);

		return command;
	}

	private static Command CreateEmulatorCommand()
	{
		var command = new Command("emulator", "Manage Android emulators");

		// emulator list
		var listCommand = new Command("list", "List available emulators");
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
						Console.WriteLine("No emulators found.");
						return;
					}

					Console.WriteLine("Available emulators:");
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

		// emulator create
		var createCommand = new Command("create", "Create a new emulator")
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

		// emulator start
		var startCommand = new Command("start", "Start an emulator")
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

		// emulator stop
		var stopCommand = new Command("stop", "Stop a running emulator")
		{
			new Argument<string>("serial", "Device serial (e.g., emulator-5554)")
		};
		stopCommand.SetHandler(async (InvocationContext context) =>
		{
			var androidProvider = Program.AndroidProvider;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var dryRun = context.ParseResult.GetValueForOption(GlobalOptions.DryRunOption);
			var serial = context.ParseResult.GetValueForArgument(
				(Argument<string>)context.ParseResult.CommandResult.Command.Arguments.First());

			var formatter = useJson 
				? (IOutputFormatter)new JsonOutputFormatter(Console.Out) 
				: new ConsoleOutputFormatter(Console.Out);

			try
			{
				if (dryRun)
				{
					Console.WriteLine($"[dry-run] Would stop emulator: {serial}");
					return;
				}

				await androidProvider.StopEmulatorAsync(serial, context.GetCancellationToken());

				if (useJson)
				{
					formatter.Write(new { success = true, serial = serial, status = "stopped" });
				}
				else
				{
					Console.WriteLine($"✓ Stopped emulator: {serial}");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});
		command.AddCommand(stopCommand);

		// emulator delete
		var deleteCommand = new Command("delete", "Delete an emulator")
		{
			new Argument<string>("name", "AVD name to delete")
		};
		deleteCommand.SetHandler(async (InvocationContext context) =>
		{
			var androidProvider = Program.AndroidProvider;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var dryRun = context.ParseResult.GetValueForOption(GlobalOptions.DryRunOption);
			var name = context.ParseResult.GetValueForArgument(
				(Argument<string>)context.ParseResult.CommandResult.Command.Arguments.First());

			var formatter = useJson 
				? (IOutputFormatter)new JsonOutputFormatter(Console.Out) 
				: new ConsoleOutputFormatter(Console.Out);

			try
			{
				if (dryRun)
				{
					Console.WriteLine($"[dry-run] Would delete AVD: {name}");
					return;
				}

				await androidProvider.DeleteAvdAsync(name, context.GetCancellationToken());

				if (useJson)
				{
					formatter.Write(new { success = true, name = name, status = "deleted" });
				}
				else
				{
					Console.WriteLine($"✓ Deleted AVD: {name}");
				}
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});
		command.AddCommand(deleteCommand);

		return command;
	}
}
