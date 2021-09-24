#nullable enable
namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	public interface ITestListener
	{
		void RecordResult(TestResultViewModel result);
	}
}