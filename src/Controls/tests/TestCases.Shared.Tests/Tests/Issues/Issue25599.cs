using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25599 : _IssuesUITest
	{
		public Issue25599(TestDevice testDevice) : base(testDevice) { }

		public override string Issue => "OnNavigating wrong target when tapping the same tab";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void NavigatingEventFired()
		{
			App.WaitForElement("HomePageButton");
			App.Tap("HomePageButton");
			App.WaitForElement("DetailsPageLabel");
			App.TapTab("Home"); // Tapping already selected tab triggers OnNavigating, which cancels pop-to-root
			App.WaitForElement("DetailsPageLabel");
			App.WaitForNoElement("HomePageLabel");
		}
	}
}