using System.Diagnostics;
using Xunit.Abstractions;

namespace Microsoft.Maui.IntegrationTests.Android
{
	public static class Adb
	{
		static readonly string AdbTool = Path.Combine(TestEnvironment.GetAndroidSdkPath(), "platform-tools", "adb");
		const int DEFAULT_TIMEOUT = 20;

		public static bool WaitForEmulator(int timeout, string deviceId = "", ITestOutputHelper? output = null)
		{
			var maxWaitTime = DateTime.UtcNow.AddSeconds(timeout);
			int currentWaitTime = 0;
			bool bootCompleted = false;
			while (DateTime.UtcNow < maxWaitTime && !bootCompleted)
			{
				output?.WriteLine($"Waiting {currentWaitTime}/{timeout} seconds for the emulator to boot up...");
				var adbOutput = RunForOutput(GetArgs("shell getprop sys.boot_completed", deviceId), out _, output: output);
				output?.WriteLine($"sys.boot_completed: {adbOutput}");
				int.TryParse(adbOutput, out int bootCompletedPropValue);
				bootCompleted = bootCompletedPropValue == 1;
				Thread.Sleep(10000);
				currentWaitTime += 10;
			}
			return bootCompleted;
		}

		public static bool KillEmulator(string deviceId = "", ITestOutputHelper? output = null)
		{
			return Run(GetArgs("emu kill", deviceId), output: output);
		}

		public static bool UninstallPackage(string package, string deviceId = "", ITestOutputHelper? output = null)
		{
			return Run(GetArgs($"uninstall {package}", deviceId), output: output);
		}

		static string GetArgs(string args, string deviceId)
		{
			return string.IsNullOrEmpty(deviceId) ? args : $"-s {deviceId} " + args;
		}

		public static bool Run(string args, int timeout = DEFAULT_TIMEOUT, string deviceId = "", ITestOutputHelper? output = null)
		{
			RunForOutput(args, out int exitCode, timeout, deviceId, output: output);
			if (exitCode != 0)
				output?.WriteLine(exitCode.ToString());

			return exitCode == 0;
		}

		public static string RunForOutput(string args, out int exitCode, int timeout = DEFAULT_TIMEOUT, string deviceId = "", ITestOutputHelper? output = null)
		{
			return ToolRunner.Run(AdbTool, GetArgs(args, deviceId), out exitCode, timeoutInSeconds: timeout, output: output);
		}

	}
}
