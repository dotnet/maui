using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue6368 : IssuesUITest
	{
		public Issue6368(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[CustomRenderer]Crash when navigating back from page with custom renderer control";
		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.Navigation)]
		[FailsOnIOS]
		public void Issue6368Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("btnGo");
			RunningApp.Tap("btnGo");
			RunningApp.WaitForElement("GoToNextPage");
			RunningApp.Back();
		}
	}
}