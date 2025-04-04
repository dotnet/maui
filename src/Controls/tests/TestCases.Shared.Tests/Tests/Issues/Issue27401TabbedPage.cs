#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27401TabbedPage : _IssuesUITest
	{
		public Issue27401TabbedPage(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "TabbedPage.PopToRootOnTabReselect";

		[Test]
		[Category(UITestCategories.TabbedPage)]
		public void ClickingCurrentTabShouldNavigateToRoot()
		{
			App.WaitForElement("Deep0Button");
			App.Click("Deep0Button");
			App.WaitForElement("Deep1Button");
			App.Click("Deep1Button");
			App.Click("Tab 1");
			App.WaitForElement("Deep0Button");
		}
	}
}
#endif