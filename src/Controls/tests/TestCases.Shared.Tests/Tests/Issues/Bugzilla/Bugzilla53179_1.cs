using NUnit.Framework;
using NUnit.Framework.Legacy;
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
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}
		
		var startTest = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + StartTest + "']"));
		startTest.Click();

		var rootLabel = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + RootLabel + "']"));
		ClassicAssert.NotNull(rootLabel);
	}
}