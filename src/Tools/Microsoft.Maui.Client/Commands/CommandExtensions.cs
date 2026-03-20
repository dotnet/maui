// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Invocation;

namespace Microsoft.Maui.Client.Commands;

/// <summary>
/// Extension methods for System.CommandLine option parsing.
/// Eliminates the repetitive cast-and-LINQ pattern for looking up command-local options by name.
/// </summary>
internal static class CommandExtensions
{
	/// <summary>
	/// Gets the value for a command-local option by name, avoiding the verbose
	/// <c>(Option&lt;T&gt;)context.ParseResult.CommandResult.Command.Options.First(o => o.Name == "...")</c> pattern.
	/// </summary>
	public static T? GetOption<T>(this InvocationContext context, string name)
	{
		var option = context.ParseResult.CommandResult.Command.Options.FirstOrDefault(o => o.Name == name);
		if (option is Option<T> typed)
			return context.ParseResult.GetValueForOption(typed);
		return default;
	}
}
