#if ANDROID
using NUnit.Framework;
using OpenQA.Selenium.Appium;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla25662 : _IssuesUITest
{
    public Bugzilla25662(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "Setting IsEnabled does not disable SwitchCell";

	[Test]
	[Category(UITestCategories.Cells)]
	[FailsOnIOS]
	[FailsOnWindows]
	public void Bugzilla25662Test()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		App.WaitForNoElement("One");

		var item = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "One" + "']"));
		item.Click();

		App.WaitForNoElement("FAIL");
	}
}
#endif
