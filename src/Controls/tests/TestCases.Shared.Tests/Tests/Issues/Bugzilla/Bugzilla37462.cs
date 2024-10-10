#if !IOS && !MACCATALYST // TODO: Fix on Apple devices.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla37462 : _IssuesUITest
{
	public Bugzilla37462(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Using App Compat/App Compat theme breaks Navigation.RemovePage on Android ";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void CanRemoveIntermediatePagesAndPopToFirstPage()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		// Start at page 1
		App.WaitForNoElement("Go To 2");
		App.WaitForNoElement("This is a label on page 1");
		var goTo2Button = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "Go To 2" + "']"));
		goTo2Button.Click();

		App.WaitForNoElement("Go To 3");
		var goTo3Button = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "Go To 3" + "']"));
		goTo3Button.Click();

		App.WaitForNoElement("Go To 4");
		var goTo4Button = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "Go To 4" + "']"));
		goTo4Button.Click();

		App.WaitForNoElement("Back to 1");
		var backTo1Button = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "Back to 1" + "']"));
		backTo1Button.Click();

		// Clicking "Back to 1" should remove pages 2 and 3 from the stack
		// Then call PopAsync, which should return to page 1
		App.WaitForNoElement("Go To 2");
		App.WaitForNoElement("This is a label on page 1");
	}
}
#endif