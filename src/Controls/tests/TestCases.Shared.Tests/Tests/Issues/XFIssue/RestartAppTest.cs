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
	//[FailsOnIOSWhenRunningOnXamarinUITest]
	//public void ForcingRestartDoesNotCauseCrash()
	//{
	//	App.WaitForElement(RestartButton);
	//	App.Tap(RestartButton);

	//	// If the app hasn't crashed, this test has passed
	//	App.WaitForElement(Success);
	//}
}