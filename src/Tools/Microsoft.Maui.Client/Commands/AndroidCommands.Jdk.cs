// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Maui.Client.Models;
using Microsoft.Maui.Client.Output;
using Microsoft.Maui.Client.Providers.Android;
using Microsoft.Maui.Client.Utils;
using Spectre.Console;

namespace Microsoft.Maui.Client.Commands;

public static partial class AndroidCommands
{
	static Command CreateJdkCommand()
	{
		var command = new Command("jdk", "Manage JDK installation");

		// jdk check
		var checkCommand = new Command("check", "Check JDK installation status");
		checkCommand.SetHandler(async (InvocationContext context) =>
		{
			var jdkManager = Program.JdkManager;

			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var formatter = Program.GetFormatter(context);

			var healthCheck = await jdkManager.CheckHealthAsync(context.GetCancellationToken());

			if (useJson)
			{
				formatter.Write(healthCheck);
			}
			else
			{
				var statusIcon = healthCheck.Status == Models.CheckStatus.Ok ? "✓" :
								 healthCheck.Status == Models.CheckStatus.Warning ? "⚠" : "✗";
				formatter.WriteInfo($"{statusIcon} {healthCheck.Message}");

				if (healthCheck.Details?.TryGetValue("path", out var path) == true)
					formatter.WriteProgress($"Path: {path}");
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
			var version = context.GetOption<int>("version");
			var path = context.GetOption<string>("path");
			var formatter = Program.GetFormatter(context);

			try
			{
				if (dryRun)
				{
					formatter.WriteInfo($"[dry-run] Would install OpenJDK {version}");
					return;
				}

				formatter.WriteProgress($"Installing OpenJDK {version}...");
				await jdkManager.InstallAsync(version, path, context.GetCancellationToken());

				if (useJson)
				{
					formatter.Write(new { success = true, version = version, path = jdkManager.DetectedJdkPath });
				}
				else
				{
					formatter.WriteSuccess($"OpenJDK {version} installed to {jdkManager.DetectedJdkPath}");
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
				var formatter = Program.GetFormatter(context);
				formatter.Write(new { versions = versions });
			}
			else
			{
				var formatter = Program.GetFormatter(context);
				formatter.WriteTable(
					versions,
					("Version", v => v.ToString()),
					("Status", v => jdkManager.DetectedJdkVersion == v ? "✓ current" : ""));
			}
		});

		command.AddCommand(checkCommand);
		command.AddCommand(installCommand);
		command.AddCommand(listCommand);

		return command;
	}
}
