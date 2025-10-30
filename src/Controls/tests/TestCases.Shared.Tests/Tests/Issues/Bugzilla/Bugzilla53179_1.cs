using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla53179_1 : _IssuesUITest
{
	const string StartTest = "Start Test";
	const string RootLabel = "Root";

	public Bugzilla53179_1(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "1PopAsync crashing after RemovePage when support packages are updated to 25.1.1";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void PopAsyncAfterRemovePageDoesNotCrash()
	{
		App.WaitForElement(StartTest);
		App.Tap(StartTest);
		App.WaitForElementTillPageNavigationSettled(RootLabel);
	}
}