using Microsoft.Maui.Appium;
using OpenQA.Selenium.Support.UI;

namespace Microsoft.Maui.AppiumTests
{
	public abstract class _IssuesUITest : UITestBase
	{
		public _IssuesUITest(TestDevice device) : base(device) { }

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			NavigateToIssue(Issue);
		}

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			App.NavigateBack();
			App.Tap("GoBackToGalleriesButton");
		}

		public abstract string Issue { get; }

		private static void NavigateToIssue(string issue)
		{
			App.NavigateToIssues();

			App.EnterText("SearchBarGo", issue);

			App.WaitForElement("SearchButton");
			App.Tap("SearchButton");
		}
	}
}