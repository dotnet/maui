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
			if (!UseDirectLaunch)
			{
				App.WaitForGoToTestButtonWithRecovery(Issue);
				App.EnterText("SearchBar", Issue);
				App.WaitForElement("GoToTestButton");
				App.Tap("GoToTestButton");
			}
		}

		public override IConfig GetTestConfig()
		{
			var config = base.GetTestConfig();

			if (UseDirectLaunch)
				config.SetTestConfigurationArg("test", Issue);

			return config;
		}

		public override void LaunchAppWithTest()
		{
			if (UseDirectLaunch)
				App.LaunchApp(Issue, ResetAfterEachTest);
			else
				App.LaunchApp();
		}

		public abstract string Issue { get; }
	}
}