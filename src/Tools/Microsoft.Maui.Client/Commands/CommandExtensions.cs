// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Parsing;

namespace Microsoft.Maui.Client.Commands;

/// <summary>
/// Extension methods for System.CommandLine option parsing.
/// Eliminates the repetitive cast-and-LINQ pattern for looking up command-local options by name.
/// </summary>
internal static class CommandExtensions
{
	/// <summary>
	/// Gets the value for a command-local option by name, avoiding the verbose
	/// <c>(Option&lt;T&gt;)parseResult.CommandResult.Command.Options.First(o => o.Name == "...")</c> pattern.
	/// </summary>
	public static T? GetOption<T>(this ParseResult parseResult, string name)
	{
		var option = parseResult.CommandResult.Command.Options.FirstOrDefault(o =>
			o.Name == name || o.Name == $"--{name}");
		if (option is Option<T> typed)
			return parseResult.GetValue(typed);
		return default;
	}
}
