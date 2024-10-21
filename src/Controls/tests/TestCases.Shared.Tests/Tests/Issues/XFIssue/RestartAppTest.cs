using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class RestartAppTest : _IssuesUITest
{
	public RestartAppTest(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Config changes which restart the app should not crash";

	//[Test]
	//[Category(UITestCategories.LifeCycle)]
	//[FailsOnIOS]
	//public void ForcingRestartDoesNotCauseCrash()
	//{
	//	RunningApp.WaitForElement(RestartButton);
	//	RunningApp.Tap(RestartButton);

	//	// If the app hasn't crashed, this test has passed
	//	RunningApp.WaitForElement(Success);
	//}
}