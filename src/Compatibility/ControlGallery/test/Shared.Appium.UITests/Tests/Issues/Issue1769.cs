using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue1769 : IssuesUITest
	{
		const string GoToPageTwoButtonText = "Go To Page 2";
		const string SwitchAutomatedId = nameof(SwitchAutomatedId);
		const string SwitchIsNowLabelTextFormat = "Switch is now {0}";

		public Issue1769(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "PushAsync with Switch produces NRE";
		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.Switch)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void Issue1769Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement(GoToPageTwoButtonText);
			RunningApp.Tap(GoToPageTwoButtonText);

			RunningApp.WaitForElement(SwitchAutomatedId);
			RunningApp.WaitForElement(string.Format(SwitchIsNowLabelTextFormat, false));
			RunningApp.Tap(SwitchAutomatedId);
			RunningApp.WaitForElement(string.Format(SwitchIsNowLabelTextFormat, true));
		}
	}
}