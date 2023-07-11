using Microsoft.Maui.Appium;

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

		private void NavigateToIssue(string issue)
		{
			App.NavigateToIssues();
			App.WaitForElement(q => q.Raw("* marked:'TestCasesIssueList'"));

			App.EnterText(q => q.Raw("* marked:'SearchBarGo'"), $"{issue}");

			App.WaitForElement(q => q.Raw("* marked:'SearchButton'"));
			App.Tap(q => q.Raw("* marked:'SearchButton'"));
		}
	}

}