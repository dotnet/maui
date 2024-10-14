#if WINDOWS // TODO: Fix on Apple devices and Windows.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla33612 : _IssuesUITest
{
	public Bugzilla33612(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "(A) Removing a page from the navigation stack causes an 'Object reference' exception in Android only";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void Issue33612RemovePagesWithoutRenderers()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		App.WaitForNoElement("Go To Page 2");
		var goToPage2Button = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "Go To Page 2" + "']"));
		goToPage2Button.Click();
		App.WaitForNoElement("This is Page 2");
		App.Screenshot("At Page 2");
		var goToPage3Button = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "Go To Page 3" + "']"));
		goToPage3Button.Click();

		App.WaitForNoElement("This is Page 3");
		App.WaitForNoElement("Return To Page 2",
			timeout: TimeSpan.FromSeconds(15));
		App.Screenshot("At Page 3");
		var returnPage2Button = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "Return To Page 2" + "']"));
		returnPage2Button.Click();

		App.WaitForNoElement("If you're seeing this, nothing crashed. Yay!");
		App.Screenshot("Success Page");
	}
}
#endif