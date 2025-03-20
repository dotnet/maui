using System.Diagnostics;

namespace UITest.Appium
{
	public static class ShellHelper
	{
		public static void ExecuteShellCommand(string command)
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

		internal static string GetShell()
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				return "cmd.exe";
			else
				return "/bin/bash";
		}

		internal static string GetShellArgument(string shell, string command)
		{
			if (shell == "cmd.exe")
				return $"/C {command}";
			else
				return $"-c \"{command}\"";
		}
	}
}