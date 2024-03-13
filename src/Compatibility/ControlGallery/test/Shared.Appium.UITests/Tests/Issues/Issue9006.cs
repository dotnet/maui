using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue9006 : IssuesUITest
	{
		public Issue9006(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Unable to open a new Page for the second time in Xamarin.Forms Shell Tabbar";

		[Test]
		[Category(UITestCategories.Shell)]
		public void ClickingOnTabToPopToRootDoesntBreakNavigation()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.Tap("Click Me");
			RunningApp.WaitForElement("FinalLabel");
			RunningApp.Tap("Tab1AutomationId");
			RunningApp.Tap("Click Me");
			RunningApp.WaitForNoElement("Success");
		}
	}
}