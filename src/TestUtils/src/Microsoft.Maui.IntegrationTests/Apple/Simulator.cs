using System.Diagnostics;

namespace Microsoft.Maui.IntegrationTests.Apple
{
	public class Simulator
	{
		readonly string XCRunTool = "xcrun";

		public string XHarnessID { get; set; } = System.Environment.GetEnvironmentVariable("IOS_TEST_DEVICE") ?? "ios-simulator-64";

		string _udid = "";
		public string GetUDID()
		{
			if (!string.IsNullOrEmpty(_udid))
				return _udid;

			return _udid = XHarness.GetSimulatorUDID(XHarnessID).Trim();
		}

		public bool Launch()
		{
			ToolRunner.Run(XCRunTool, $"simctl boot {GetUDID()}", out int exitCode, timeoutInSeconds: 30);
			return exitCode == 0;
		}

		public bool Shutdown()
		{
			ToolRunner.Run(XCRunTool, $"simctl shutdown {GetUDID()}", out int exitCode, timeoutInSeconds: 60);
			return exitCode == 0;
		}

		public bool ShowWindow()
		{
			ToolRunner.Run("open", $"-a Simulator", out int exitCode, timeoutInSeconds: 30);
			return exitCode == 0;
		}

		public void Log(string logDirectory)
		{
			var homeDirectory = Environment.GetEnvironmentVariable("HOME");
			var simUDID = GetUDID();

			var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
			StartProcess("zip", $"-9r \"{logDirectory}/DiagnosticReports_{simUDID}_{timestamp}.zip\" \"{homeDirectory}/Library/Logs/DiagnosticReports/\"");
			StartProcess("zip", $"-9r \"{logDirectory}/CoreSimulator_{simUDID}_{timestamp}.zip\" \"{homeDirectory}/Library/Logs/CoreSimulator/{simUDID}\"");
			StartProcess("xcrun", $"simctl spawn {simUDID} log collect --output {homeDirectory}/{simUDID}_log_{timestamp}.logarchive");
			StartProcess("zip", $"-9r \"{logDirectory}/{simUDID}_log_{timestamp}.logarchive.zip\" \"{homeDirectory}/{simUDID}_log_{timestamp}.logarchive\"");
		}

		static void StartProcess(string fileName, string arguments)
		{
			var processStartInfo = new ProcessStartInfo
			{
				FileName = fileName,
				Arguments = arguments,
				RedirectStandardOutput = false,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			using (var process = Process.Start(processStartInfo))
			{
				process?.WaitForExit();
			}
		}
	}
}

