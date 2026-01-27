using System.Diagnostics;
using Xunit.Abstractions;

namespace Microsoft.Maui.IntegrationTests.Android
{
	public class Emulator
	{
		readonly string AvdManagerTool = Path.Combine(TestEnvironment.GetAndroidCommandLineToolsPath(), TestEnvironment.IsWindows ? "avdmanager.bat" : "avdmanager");
		readonly string SdkManagerTool = Path.Combine(TestEnvironment.GetAndroidCommandLineToolsPath(), TestEnvironment.IsWindows ? "sdkmanager.bat" : "sdkmanager");
		readonly string EmulatorTool = Path.Combine(TestEnvironment.GetAndroidSdkPath(), "emulator", "emulator");

		public int ApiLevel { get; set; } = 30;
		public string Abi { get; set; } = "x86_64";
		public string ImageType { get; set; } = "default";
		public string DeviceType { get; set; } = "pixel_5";
		public int Port { get; set; } = 5570;

		public string Name => $"MauiTestDevice{ApiLevel}{Abi}";
		public string Id => $"emulator-{Port}";
		public string SystemImageId => $"system-images;android-{ApiLevel};{ImageType};{Abi}";

		public bool AcceptLicenses(out string acceptLicenseOutput, ITestOutputHelper? output = null)
		{
			acceptLicenseOutput = ToolRunner.Run(new ProcessStartInfo(SdkManagerTool, "--licenses"), out int exitCode, timeoutInSeconds: 30, inputAction: (p) =>
			{
				for (int i = 0; i < 10; i++)
				{
					p.StandardInput.WriteLine('y');
				}
			}, output: output);

			if (exitCode != 0)
				output?.WriteLine(acceptLicenseOutput);

			return exitCode == 0;
		}

		public bool InstallAvd(out string installOutput, ITestOutputHelper? output = null)
		{
			installOutput = ToolRunner.Run(SdkManagerTool, $"\"{SystemImageId}\"", out int exitCode, timeoutInSeconds: 180, output: output);
			if (exitCode != 0)
				output?.WriteLine(installOutput);

			return exitCode == 0;
		}

		public bool DeleteAvd(ITestOutputHelper? output = null)
		{
			var deleteOutput = ToolRunner.Run(AvdManagerTool, $"delete avd -n {Name}", out int exitCode, timeoutInSeconds: 15, output: output);
			return exitCode == 0 || deleteOutput.Contains($"There is no Android Virtual Device named '{Name}'", StringComparison.OrdinalIgnoreCase);
		}

		public bool CreateAvd(bool force = true, ITestOutputHelper? output = null)
		{
			var createArgs = $"create avd -n {Name} -k \"{SystemImageId}\" -d {DeviceType}";
			if (force)
				createArgs += " -f";

			var createOutput = ToolRunner.Run(AvdManagerTool, createArgs, out int exitCode, timeoutInSeconds: 15, output: output);
			if (exitCode != 0)
				output?.WriteLine(createOutput);

			return exitCode == 0;
		}

		public bool LaunchAndWaitForAvd(int timeToWaitInSeconds, string logFile, ITestOutputHelper? output = null)
		{
			if (Adb.WaitForEmulator(10, Id, output: output))
				return true;

			if (!DeleteAvd(output: output))
				return false;

			if (!CreateAvd(output: output))
				return false;

			var launchArgs = $"-verbose -detect-image-hang -port {Port} -avd {Name}";
			launchArgs += TestEnvironment.IsRunningOnCI ? " -no-window -no-boot-anim -no-audio -no-snapshot -cache-size 512" : string.Empty;

			// Emulator process does not stop once the emulator is running, end it after 15 seconds and then begin polling for boot success
			output?.WriteLine($"Launching AVD: {Name}...");
			var emulatorOutput = ToolRunner.Run(EmulatorTool, launchArgs, out _, timeoutInSeconds: 15, output: output);
			File.WriteAllText(logFile, emulatorOutput);
			return Adb.WaitForEmulator(timeToWaitInSeconds, Id, output: output);
		}

	}
}
