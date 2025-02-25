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
			const string Page2Title = "Page 2";

			// 1. Navigate to the TabbedPage.
			App.WaitForElement("NavigateButton");
			App.Tap("NavigateButton");

			// 2. Click the second Tab.
			App.TapTab(Page2Title);

			// 3. Navigate back.
			App.WaitForElement("Page2Button");
			App.Tap("Page2Button");

			// 4. Repeat the process. Navigate to the TabbedPage.
			App.WaitForElement("NavigateButton");
			App.Tap("NavigateButton");

			// 2. Click the second Tab.
			App.TapTab(Page2Title);

			App.WaitForElement("Page2Button");
		}
	}
}