// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Parsing;
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
		checkCommand.SetAction(async (ParseResult parseResult, CancellationToken cancellationToken) =>
		{
			var jdkManager = Program.JdkManager;

			var useJson = parseResult.GetValue(GlobalOptions.JsonOption);
			var formatter = Program.GetFormatter(parseResult);

			var healthCheck = await jdkManager.CheckHealthAsync(cancellationToken);

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
			new Option<int>("--version") { Description = "JDK version (17 or 21)", DefaultValueFactory = _ => 17 },
			new Option<string>("--path") { Description = "Installation path" }
		};
		installCommand.SetAction(async (ParseResult parseResult, CancellationToken cancellationToken) =>
		{
			var jdkManager = Program.JdkManager;

			var useJson = parseResult.GetValue(GlobalOptions.JsonOption);
			var dryRun = parseResult.GetValue(GlobalOptions.DryRunOption);
			var version = parseResult.GetOption<int>("version");
			var path = parseResult.GetOption<string>("path");
			var formatter = Program.GetFormatter(parseResult);

			try
			{
				if (dryRun)
				{
					formatter.WriteInfo($"[dry-run] Would install OpenJDK {version}");
					return 0;
				}

				formatter.WriteProgress($"Installing OpenJDK {version}...");
				await jdkManager.InstallAsync(version, path, cancellationToken);

				if (useJson)
				{
					formatter.Write(new { success = true, version = version, path = jdkManager.DetectedJdkPath });
				}
				else
				{
					formatter.WriteSuccess($"OpenJDK {version} installed to {jdkManager.DetectedJdkPath}");
				}
				return 0;
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				return 1;
			}
		});

		// jdk list
		var listCommand = new Command("list", "List available JDK versions");
		listCommand.SetAction((ParseResult parseResult) =>
		{
			var jdkManager = Program.JdkManager;
			var useJson = parseResult.GetValue(GlobalOptions.JsonOption);

			var versions = jdkManager.GetAvailableVersions().ToList();

			if (useJson)
			{
				var formatter = Program.GetFormatter(parseResult);
				formatter.Write(new { versions = versions });
			}
			else
			{
				var formatter = Program.GetFormatter(parseResult);
				formatter.WriteTable(
					versions,
					("Version", v => v.ToString()),
					("Status", v => jdkManager.DetectedJdkVersion == v ? "✓ current" : ""));
			}
		});

		command.Add(checkCommand);
		command.Add(installCommand);
		command.Add(listCommand);

		return command;
	}
}
