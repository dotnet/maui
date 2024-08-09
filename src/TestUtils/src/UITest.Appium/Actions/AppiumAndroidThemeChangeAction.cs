using System.Diagnostics;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumAndroidThemeChangeAction : ICommandExecutionGroup
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
				ExecuteAdbCommand($"adb shell cmd uimode night no");
				return CommandResponse.SuccessEmptyResponse;
			}
			else if (commandName == SetDarkTheme)
			{
				ExecuteAdbCommand($"adb shell cmd uimode night yes");
				return CommandResponse.SuccessEmptyResponse;
			}

			return CommandResponse.FailedEmptyResponse;
		}

		public bool IsCommandSupported(string commandName)
		{
			return _commands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
		}

		private static void ExecuteAdbCommand(string command)
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
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				return "cmd.exe";
			else
				return "/bin/bash";
		}

		private static string GetShellArgument(string shell, string command)
		{
			if (shell == "cmd.exe")
				return $"/C {command}";
			else
				return $"-c \"{command}\"";
		}
	}
}
