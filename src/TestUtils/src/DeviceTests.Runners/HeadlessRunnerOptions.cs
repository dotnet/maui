#nullable enable
namespace Microsoft.Maui.TestUtils.DeviceTests.Runners
{
	public class HeadlessRunnerOptions
	{
		public string TestResultsFilename { get; set; } = "testResults.xml";

		public bool RequiresUIContext { get; set; } = true;
	}
}
