#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24741 : _IssuesUITest
	{
		public Issue24741(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Unable to select tab after backing out of page and returning";

		[Test]
		[Category(UITestCategories.TabbedPage)]
		public void SelectTabAfterNavigation()
		{
			if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
			{
				throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
			}

			const string Page2Title = "Page 2";

			// 1. Navigate to the TabbedPage.
			App.WaitForElement("NavigateButton");
			App.Tap("NavigateButton");

			// 2. Click the second Tab.
			var tab2First = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + Page2Title + "']"));
			tab2First.Click();

			// 3. Navigate back.
			App.Back();

			// 4. Repeat the process. Navigate to the TabbedPage.
			App.WaitForElement("NavigateButton");
			App.Tap("NavigateButton");

			// 2. Click the second Tab.
			var tab2Second = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + Page2Title + "']"));
			tab2Second.Click();

			// 6. Screenshot to validate the result.
			App.Screenshot("If can select the second Tab, the test has passed.");
			App.Back();
		}
	}
}
#endif