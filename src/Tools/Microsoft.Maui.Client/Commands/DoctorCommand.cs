// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Parsing;
using Microsoft.Maui.Client.Models;
using Microsoft.Maui.Client.Output;
using Microsoft.Maui.Client.Services;

namespace Microsoft.Maui.Client.Commands;

/// <summary>
/// Implementation of 'maui doctor' command.
/// </summary>
public static class DoctorCommand
{
	public static Command Create()
	{
		var command = new Command("doctor", "Check system for MAUI development readiness")
		{
			// Options
			new Option<bool>("--fix") { Description = "Attempt to automatically fix issues" },
			new Option<string>("--platform") { Description = "Check only specific platform (dotnet, android, apple, windows)" }
		};

		command.SetAction(async (ParseResult parseResult, CancellationToken cancellationToken) =>
		{
			var doctorService = Program.DoctorService;

			var useJson = parseResult.GetValue(GlobalOptions.JsonOption);
			var fix = parseResult.GetOption<bool>("fix");
			var platform = parseResult.GetOption<string>("platform");
			var formatter = Program.GetFormatter(parseResult);

			try
			{
				DoctorReport report;

				if (!useJson && formatter is SpectreOutputFormatter spectre)
				{
					report = await spectre.StatusAsync("Running health checks...", async () =>
						string.IsNullOrEmpty(platform)
							? await doctorService.RunAllChecksAsync(cancellationToken)
							: await doctorService.RunCategoryChecksAsync(platform, cancellationToken));
				}
				else
				{
					report = string.IsNullOrEmpty(platform)
						? await doctorService.RunAllChecksAsync(cancellationToken)
						: await doctorService.RunCategoryChecksAsync(platform, cancellationToken);
				}

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
						if (!useJson && formatter is SpectreOutputFormatter spectreFix)
						{
							await spectreFix.LiveProgressAsync(async (ctx) =>
							{
								for (int i = 0; i < fixableIssues.Count; i++)
								{
									var issue = fixableIssues[i];
									var task = ctx.AddTask($"Fixing: {issue.Name}");
									var success = await doctorService.TryFixAsync(issue.Fix!, cancellationToken);
									task.Complete(success ? $"Fixed: {issue.Name}" : $"Failed: {issue.Name}");
								}
							});
						}
						else
						{
							formatter.WriteInfo("Attempting automatic fixes...");
							foreach (var issue in fixableIssues)
							{
								formatter.WriteProgress($"Fixing: {issue.Name}");
								await doctorService.TryFixAsync(issue.Fix!, cancellationToken);
							}
						}
					}
				}

				// Set exit code based on results
				var hasErrors = report.Checks.Any(c => c.Status == Models.CheckStatus.Error);
				return hasErrors ? 1 : 0;
			}
			catch (Exception ex)
			{
				formatter.WriteError(ex);
				return 1;
			}
		});

		return command;
	}
}
