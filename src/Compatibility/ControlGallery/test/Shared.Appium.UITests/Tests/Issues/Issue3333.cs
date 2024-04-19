using NUnit.Framework;
using UITest.Appium;

namespace UITests
{ 
    public class Issue3333 : IssuesUITest
	{
		const string KSuccess = "If you're reading this the test has passed";

		public Issue3333(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[UWP] with ListView on page, Navigation.PopAsync() throws exception";
		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Navigation)]
		[FailsOnIOS]
		public void SettingBindingContextToNullBeforingPoppingPageCrashes()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForNoElement(KSuccess);
		}
	}
}