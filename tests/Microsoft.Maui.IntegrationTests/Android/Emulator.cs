
namespace Microsoft.Maui.IntegrationTests.Android
{
	public class Emulator
	{
		readonly string AvdManagerTool = Path.Combine(TestEnvironment.GetAndroidCommandLineToolsPath(), TestEnvironment.IsWindows ? "avdmanager.bat" : "avdmanager");
		readonly string EmulatorTool = Path.Combine(TestEnvironment.GetAndroidSdkPath(), "emulator", "emulator");

		public int ApiLevel { get; set; } = 30;
		public string Abi { get; set; } = "x86_64";
		public string ImageType { get; set; } = "google_apis_playstore";
		public string DeviceType { get; set; } = "pixel_5";
		public int Port { get; set; } = 5570;

		public string Name => $"MauiTestDevice{ApiLevel}{Abi}";
		public string Id => $"emulator-{Port}";

		public bool DeleteAvd()
		{
			TestContext.WriteLine($"Deleting AVD: {Name}...");
			var deleteOutput = ToolRunner.Run(AvdManagerTool, $"delete avd -n {Name}", out int code, timeoutInSeconds: 15);
			return code == 0 || deleteOutput.Contains($"There is no Android Virtual Device named '{Name}'", StringComparison.OrdinalIgnoreCase);
		}

		public bool CreateAvd(bool force = true)
		{
			var createArgs = $"create avd -n {Name} -k \"system-images;android-{ApiLevel};{ImageType};{Abi}\" -d {DeviceType}";
			if (force)
				createArgs += " -f";

			var createOutput = ToolRunner.Run(AvdManagerTool, createArgs, out int code, timeoutInSeconds: 15);
			TestContext.WriteLine($"Creating AVD: {Name}...");
			TestContext.WriteLine(createOutput);
			return code == 0;
		}

		public bool LaunchAndWaitForAvd(int timeToWaitInSeconds)
		{
			if (Adb.WaitForEmulator(5, Id))
				return true;

			if (!DeleteAvd())
				return false;

			if (!CreateAvd())
				return false;

			var launchArgs = $"-verbose -detect-image-hang -port {Port} -avd {Name}";
			launchArgs += TestEnvironment.IsRunningOnCI ? " -no-window -no-boot-anim" : string.Empty;

			// Emulator process does not stop once the emulator is running, end it after 15 seconds and then begin polling for boot success
			TestContext.WriteLine($"Launching AVD: {Name}...");
			ToolRunner.Run(EmulatorTool, launchArgs, out _, timeoutInSeconds: 15);
			return Adb.WaitForEmulator(timeToWaitInSeconds, Id);
		}

	}
}
