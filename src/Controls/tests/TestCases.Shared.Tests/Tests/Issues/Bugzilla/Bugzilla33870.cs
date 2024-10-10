#if !MACCATALYST && !WINDOWS // TODO: Fix on Mac and Windows. 
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla33870 : _IssuesUITest
{
	public Bugzilla33870(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[W] Crash when the ListView Selection is set to null";

	[Test]
	[Category(UITestCategories.ListView)]
	[FailsOnIOS]
	public void Bugzilla33870Test()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		App.WaitForElement("PageContentAutomatedId");
		App.WaitForElement("ListViewAutomatedId");

		var clearSelectionButton = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "CLEAR SELECTION" + "']"));
		clearSelectionButton.Click();

		App.WaitForNoElement("Cleared");
	}
}
#endif
