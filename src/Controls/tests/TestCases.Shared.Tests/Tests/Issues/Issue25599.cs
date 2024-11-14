#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // To fix the issue in ios https://github.com/dotnet/maui/pull/25749#pullrequestreview-2554186362
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
			App.Tap("Home"); // Tapping already selected tab using Title
			App.WaitForElement("DetailsPageLabel");
			App.WaitForNoElement("HomePageLabel"); // Navigation does not occur when clicking on an already selected tab
		}
	}
}
#endif