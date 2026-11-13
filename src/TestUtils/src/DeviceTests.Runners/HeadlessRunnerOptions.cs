#nullable enable
namespace Microsoft.Maui.TestUtils.DeviceTests.Runners
{
	public class HeadlessRunnerOptions
	{
		public string TestResultsFilename { get; set; } = "testResults.trx";

		public bool RequiresUIContext { get; set; } = true;
	}
}
