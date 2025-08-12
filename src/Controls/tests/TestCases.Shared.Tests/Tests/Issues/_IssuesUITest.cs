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

		protected override void FixtureSetup()
		{
			int retries = 0;
			while (true)
			{
				try
				{
					base.FixtureSetup();
#if ANDROID || MACCATALYST
					App.ToggleSystemAnimations(false);
#endif
#if !MACCATALYST
					// For non-Catalyst platforms, navigate via UI
					NavigateToIssue(Issue);
#endif
					break;
				}
				catch (Exception e)
				{
					TestContext.Error.WriteLine($">>>>> {DateTime.Now} The FixtureSetup threw an exception. Attempt {retries}/{SetupMaxRetries}.{Environment.NewLine}Exception details: {e}");
					if (retries++ < SetupMaxRetries)
					{
						App.Back();
#if ANDROID || MACCATALYST
						App.ToggleSystemAnimations(true);
#endif
						Reset();
					}
					else
					{
						throw;
					}
				}
			}
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