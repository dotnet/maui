using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public abstract class _IssuesUITest : UITest
	{
#if ANDROID
		protected const string FlyoutIconAutomationId = "Open navigation drawer";
#else
		protected const string FlyoutIconAutomationId = "OK";
#endif
#if __IOS__ || WINDOWS
		protected const string BackButtonAutomationId = "Back";
#else
		protected const string BackButtonAutomationId = "Navigate up";
#endif

		public _IssuesUITest(TestDevice device) : base(device) { }

		protected override void TryToResetTestState()
		{
			NavigateToIssue(Issue);
		}

		public override IConfig GetTestConfig()
		{
			var config = base.GetTestConfig();

#if MACCATALYST
			// For Catalyst, pass the test name as a startup argument
			// If the UITestContext is not null we can directly pass the Issue via LaunchAppWithTest
			if (UITestContext is null)
			{
				config.SetTestConfigurationArg("test", Issue);
			}
#endif

			return config;
		}

		public override void LaunchAppWithTest()
		{
			App.LaunchApp(Issue, ResetAfterEachTest);
		}

		public abstract string Issue { get; }

		private void NavigateToIssue(string issue)
		{
			App.WaitForElement("GoToTestButton", issue);
			App.EnterText("SearchBar", issue);
			App.WaitForElement("GoToTestButton");
			App.Tap("GoToTestButton");
		}
	}
}