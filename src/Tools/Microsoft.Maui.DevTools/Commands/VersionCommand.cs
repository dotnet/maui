// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;
using Microsoft.Maui.DevTools.Output;

namespace Microsoft.Maui.DevTools.Commands;

/// <summary>
/// Implementation of 'maui version' command.
/// </summary>
public static class VersionCommand
{
	public static Command Create()
	{
		var command = new Command("version", "Display version information");

		command.SetHandler((InvocationContext context) =>
		{
			var useJson = context.ParseResult.GetValueForOption(GlobalOptions.JsonOption);

			var assembly = Assembly.GetExecutingAssembly();
			var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion 
				?? assembly.GetName().Version?.ToString() 
				?? "0.0.0";

			if (useJson)
			{
				var formatter = new JsonOutputFormatter(Console.Out);
				formatter.Write(new
				{
					version = version,
					runtime = Environment.Version.ToString(),
					os = Environment.OSVersion.ToString()
				});
			}
			else
			{
				Console.WriteLine($"MAUI DevTools v{version}");
				Console.WriteLine($"Runtime: .NET {Environment.Version}");
				Console.WriteLine($"OS: {Environment.OSVersion}");
			}
		});

		return command;
	}
}
