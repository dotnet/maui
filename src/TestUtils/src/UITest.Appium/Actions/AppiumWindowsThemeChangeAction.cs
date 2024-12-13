using System.Diagnostics;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumWindowsThemeChangeAction : ICommandExecutionGroup
	{
		const string SetLightTheme = "setLightTheme";
		const string SetDarkTheme = "setDarkTheme";

		readonly List<string> _commands = new()
		{
			SetLightTheme,
			SetDarkTheme
		};

		public CommandResponse Execute(string commandName, IDictionary<string, object> parameters)
		{
			if (commandName == SetLightTheme)
			{
				ExecuteCommand($"start C://Windows/Resources/Themes/aero.theme");
				return CommandResponse.SuccessEmptyResponse;
			}
			else if (commandName == SetDarkTheme)
			{
				ExecuteCommand($"start C://Windows/Resources/Themes/dark.theme");
				return CommandResponse.SuccessEmptyResponse;
			}

			return CommandResponse.FailedEmptyResponse;
		}

		public bool IsCommandSupported(string commandName)
		{
			return _commands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
		}

		private static void ExecuteCommand(string command)
		{
			var shell = GetShell();
			var shellArgument = GetShellArgument(shell, command);

			var processInfo = new ProcessStartInfo(shell, shellArgument)
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			var process = new Process { StartInfo = processInfo };

			process.Start();
			process.WaitForExit();
		}

		private static string GetShell()
		{
			return "cmd.exe";
		}

		private static string GetShellArgument(string shell, string command)
		{
			return $"/C {command}";
		}
	}
}
