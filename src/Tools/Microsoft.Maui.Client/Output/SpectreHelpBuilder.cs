// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using Spectre.Console;

namespace Microsoft.Maui.Client.Output;

/// <summary>
/// Custom help rendering using Spectre.Console for colorized output.
/// Produces styled help matching the tool's visual identity.
/// </summary>
static class SpectreHelpBuilder
{
	/// <summary>
	/// Writes colorized help for a command to the console.
	/// </summary>
	internal static void WriteHelp(HelpContext context) => WriteHelp(context.Command);

	/// <summary>
	/// Writes colorized help for a command to the console.
	/// </summary>
	internal static void WriteHelp(Command command)
	{
		var console = AnsiConsole.Console;

		// Description
		if (!string.IsNullOrEmpty(command.Description))
		{
			console.MarkupLine("[yellow]Description:[/]");
			console.MarkupLine($"  {Markup.Escape(command.Description)}");
			console.WriteLine();
		}

		// Usage
		console.MarkupLine("[yellow]Usage:[/]");
		console.MarkupLine($"  {Markup.Escape(BuildUsageLine(command))}");
		console.WriteLine();

		// Options
		var options = GetVisibleOptions(command).ToList();
		if (options.Count > 0)
		{
			console.MarkupLine("[yellow]Options:[/]");

			// Add standard options that are injected by the parser framework
			var helpEntry = ("-?, -h, --help", "Show help and usage information");
			var versionEntry = ("--version", "Show version information");
			var allAliasLengths = options.Select(o => FormatAliases(o).Length)
				.Append(helpEntry.Item1.Length)
				.Append(versionEntry.Item1.Length);
			var maxAliasLen = allAliasLengths.Max();

			foreach (var option in options)
			{
				var aliases = FormatAliases(option);
				var desc = option.Description ?? string.Empty;
				var padding = new string(' ', Math.Max(1, maxAliasLen - aliases.Length + 2));
				console.MarkupLine($"  [green]{Markup.Escape(aliases)}[/]{padding}{Markup.Escape(desc)}");
			}

			// Help option
			var helpPad = new string(' ', Math.Max(1, maxAliasLen - helpEntry.Item1.Length + 2));
			console.MarkupLine($"  [green]{Markup.Escape(helpEntry.Item1)}[/]{helpPad}{Markup.Escape(helpEntry.Item2)}");

			// Version option (root command only)
			if (command is RootCommand)
			{
				var verPad = new string(' ', Math.Max(1, maxAliasLen - versionEntry.Item1.Length + 2));
				console.MarkupLine($"  [green]{Markup.Escape(versionEntry.Item1)}[/]{verPad}{Markup.Escape(versionEntry.Item2)}");
			}

			console.WriteLine();
		}

		// Subcommands
		var subcommands = command.Subcommands.Where(c => !c.IsHidden).OrderBy(c => c.Name).ToList();
		if (subcommands.Count > 0)
		{
			console.MarkupLine("[yellow]Commands:[/]");
			var maxNameLen = subcommands.Max(c => c.Name.Length);
			foreach (var sub in subcommands)
			{
				var name = sub.Name;
				var desc = sub.Description ?? string.Empty;
				var padding = new string(' ', Math.Max(1, maxNameLen - name.Length + 2));

				// Add dim annotation for platform-specific commands
				if (desc.Contains("macOS only", StringComparison.OrdinalIgnoreCase))
				{
					var mainDesc = desc.Replace("(macOS only)", "", StringComparison.Ordinal).Replace("(macOS)", "", StringComparison.Ordinal).TrimEnd();
					console.MarkupLine($"  [green]{Markup.Escape(name)}[/]{padding}{Markup.Escape(mainDesc)} [dim](macOS)[/]");
				}
				else
				{
					console.MarkupLine($"  [green]{Markup.Escape(name)}[/]{padding}{Markup.Escape(desc)}");
				}
			}
			console.WriteLine();
		}

		// Arguments
		var arguments = command.Arguments.Where(a => !a.IsHidden).ToList();
		if (arguments.Count > 0)
		{
			console.MarkupLine("[yellow]Arguments:[/]");
			var maxArgLen = arguments.Max(a => $"<{a.Name}>".Length);
			foreach (var arg in arguments)
			{
				var name = $"<{arg.Name}>";
				var desc = arg.Description ?? string.Empty;
				var padding = new string(' ', Math.Max(1, maxArgLen - name.Length + 2));
				console.MarkupLine($"  [green]{Markup.Escape(name)}[/]{padding}{Markup.Escape(desc)}");
			}
			console.WriteLine();
		}
	}

	static string BuildUsageLine(Command command)
	{
		var parts = new List<string>();

		// Walk up to build the full command path
		var current = command;
		var path = new Stack<string>();
		while (current != null)
		{
			if (!string.IsNullOrEmpty(current.Name))
				path.Push(current.Name);
			current = current.Parents.OfType<Command>().FirstOrDefault();
		}
		parts.Add(string.Join(" ", path));

		if (command.Subcommands.Any(c => !c.IsHidden))
			parts.Add("[command]");

		if (GetVisibleOptions(command).Any())
			parts.Add("[options]");

		foreach (var arg in command.Arguments.Where(a => !a.IsHidden))
			parts.Add($"<{arg.Name}>");

		return string.Join(" ", parts);
	}

	static IEnumerable<Option> GetVisibleOptions(Command command)
	{
		// Include the command's own options and any global options from parents
		var options = command.Options.Where(o => !o.IsHidden).ToList();

		// For root command, global options are already included
		// For subcommands, add inherited global options
		var parent = command.Parents.OfType<Command>().FirstOrDefault();
		if (parent != null)
		{
			foreach (var globalOpt in parent.Options.Where(o => !o.IsHidden))
			{
				if (!options.Any(o => o.Name == globalOpt.Name))
					options.Add(globalOpt);
			}
		}

		return options;
	}

	static string FormatAliases(Option option)
	{
		var aliases = option.Aliases.OrderBy(a => a.Length).ToList();
		return string.Join(", ", aliases);
	}
}
