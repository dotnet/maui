#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla32148 : _IssuesUITest
{
	public Bugzilla32148(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => " Pull to refresh hides the first item on a list view";

	[Test]
	[Category(UITestCategories.ListView)]
	[FailsOnIOSWhenRunningOnXamarinUITest]
	public void Bugzilla32148Test()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		App.WaitForNoElement("Contact0 LastName");
		var searchButton = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "Search" + "']"));
		searchButton.Click();

		App.WaitForNoElement("Contact0 LastName");
		App.Screenshot("For manual review, verify that the first cell is visible");
	}
}
#endif