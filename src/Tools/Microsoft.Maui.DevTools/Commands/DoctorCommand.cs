// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Maui.DevTools.Output;
using Microsoft.Maui.DevTools.Services;

namespace Microsoft.Maui.DevTools.Commands;

/// <summary>
/// Implementation of 'dotnet maui doctor' command.
/// </summary>
public static class DoctorCommand
{
	public static Command Create()
	{
		var command = new Command("doctor", "Check system for MAUI development readiness")
		{
			// Options
			new Option<bool>("--fix", "Attempt to automatically fix issues"),
			new Option<string>("--platform", "Check only specific platform (dotnet, android, apple, windows)")
		};

		command.SetHandler(async (InvocationContext context) =>
		{
			var doctorService = Program.DoctorService;
			
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);
			var fix = context.ParseResult.GetValueForOption(
				(Option<bool>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "fix"));
			var platform = context.ParseResult.GetValueForOption(
				(Option<string>)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "platform"));

			var formatter = useJson 
				? (IOutputFormatter)new JsonOutputFormatter(Console.Out) 
				: new ConsoleOutputFormatter(Console.Out);

			try
			{
				var report = string.IsNullOrEmpty(platform)
					? await doctorService.RunAllChecksAsync(context.GetCancellationToken())
					: await doctorService.RunCategoryChecksAsync(platform, context.GetCancellationToken());

				// Output the report
				formatter.Write(report);

				// Attempt fixes if requested
				if (fix)
				{
					var fixableIssues = report.Checks
						.Where(c => c.Fix?.AutoFixable == true)
						.ToList();

					if (fixableIssues.Any())
					{
						Console.WriteLine();
						Console.WriteLine("Attempting automatic fixes...");
						foreach (var issue in fixableIssues)
						{
							Console.WriteLine($"  Fixing: {issue.Name}");
							await doctorService.TryFixAsync(issue.Fix!, context.GetCancellationToken());
						}
					}
				}

				// Set exit code based on results
				var hasErrors = report.Checks.Any(c => c.Status == Models.CheckStatus.Error);
				context.ExitCode = hasErrors ? 1 : 0;
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
