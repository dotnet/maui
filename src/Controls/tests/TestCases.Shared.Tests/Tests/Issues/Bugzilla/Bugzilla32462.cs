using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla32462 : _IssuesUITest
{
	public Bugzilla32462(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Crash after a page disappeared if a ScrollView is in the HeaderTemplate property of a ListView";

	[Test]
	[Category(UITestCategories.ListView)]
	[FailsOnIOS]
	public void Bugzilla36729Test()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		App.WaitForNoElement("Click!");

		var button = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "Click!" + "']"));
		button.Click();

		App.WaitForElement("listview");
		App.WaitForNoElement("some text 35");
		App.Back();
	}
}
