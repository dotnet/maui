// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Maui.DevTools.Output;

namespace Microsoft.Maui.DevTools.Commands;

/// <summary>
/// Implementation of 'maui deploy' command.
/// </summary>
public static class DeployCommand
{
	public static Command Create()
	{
		var command = new Command("deploy", "Deploy app to device")
		{
			new Argument<string>("project", () => ".", "Project or solution path"),
			new Option<string>("--device", "Target device ID"),
			new Option<string>("--configuration", () => "Debug", "Build configuration"),
			new Option<string>("--framework", "Target framework"),
			new Option<bool>("--no-build", "Skip build step")
		};

		command.SetHandler(async (InvocationContext context) =>
		{
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var dryRun = context.ParseResult.GetValueForOption(GlobalOptions.DryRunOption);
			
			var project = context.ParseResult.GetValueForArgument(
				(Argument<string>)context.ParseResult.CommandResult.Command.Arguments.First());
			var device = context.ParseResult.GetValueForOption(
				(Option<string>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "device"));
			var configuration = context.ParseResult.GetValueForOption(
				(Option<string>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "configuration"));
			var framework = context.ParseResult.GetValueForOption(
				(Option<string>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "framework"));
			var noBuild = context.ParseResult.GetValueForOption(
				(Option<bool>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "no-build"));

			var formatter = useJson 
				? (IOutputFormatter)new JsonOutputFormatter(Console.Out) 
				: new SpectreOutputFormatter();

			try
			{
				if (dryRun)
				{
					formatter.WriteInfo("[dry-run] Would deploy:");
					formatter.WriteProgress($"Project: {project}");
					formatter.WriteProgress($"Configuration: {configuration}");
					formatter.WriteProgress($"Device: {device ?? "(auto-select)"}");
					formatter.WriteProgress($"Framework: {framework ?? "(auto-detect)"}");
					formatter.WriteProgress($"Skip build: {noBuild}");
					return;
				}

				// TODO: Implement actual deployment
				formatter.WriteWarning("Deploy command not yet fully implemented.");
				formatter.WriteInfo($"Would deploy {project} to {device ?? "default device"}");
				
				context.ExitCode = 0;
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				context.ExitCode = 1;
			}
		});

		return command;
	}
}
