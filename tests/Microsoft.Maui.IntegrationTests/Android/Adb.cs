
using System.Diagnostics;

namespace Microsoft.Maui.IntegrationTests.Android
{
	public static class Adb
	{
		static readonly string AdbTool = Path.Combine(TestEnvironment.GetAndroidSdkPath(), "platform-tools", "adb");

		public static bool WaitForEmulator(int timeout, string deviceId = "")
		{
			var maxWaitTime = DateTime.UtcNow.AddSeconds(timeout);
			int currentWaitTime = 0;
			bool bootCompleted = false;
			while (DateTime.UtcNow < maxWaitTime && !bootCompleted)
			{
				TestContext.WriteLine($"Waiting {currentWaitTime}/{timeout} seconds for the emulator to boot up...");
				var adbOutput = Run(GetArgs("shell getprop sys.boot_completed", deviceId), out _);
				int.TryParse(adbOutput, out int bootCompletedPropValue);
				bootCompleted = bootCompletedPropValue == 1;
				Thread.Sleep(3000);
				currentWaitTime += 3;
			}
			return bootCompleted;
		}

		public static bool KillEmulator (string deviceId = "")
		{
			var killOutput = Run(GetArgs("emu kill", deviceId), out int exitCode);
			if (exitCode != 0)
				TestContext.WriteLine(killOutput);

			return exitCode == 0;
		}

		public static string Run(string args, out int exitCode, int timeout = 20, string deviceId = "")
		{
			return ToolRunner.Run(AdbTool, GetArgs(args, deviceId), out exitCode, timeoutInSeconds: timeout);
		}

		static string GetArgs(string args, string deviceId)
		{
			return string.IsNullOrEmpty(deviceId) ? args : $"-s {deviceId} " + args;
		}
	}
}
