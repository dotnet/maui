using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue4484 : IssuesUITest
	{
		public Issue4484(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] ImageButton inside NavigationView.TitleView throw exception during device rotation";
		
		[Test]
		[Category(UITestCategories.ImageButton)]
		public void RotatingDeviceDoesntCrashTitleView()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("Instructions");
			RunningApp.SetOrientationLandscape();
			RunningApp.WaitForElement("Instructions");
			RunningApp.SetOrientationPortrait();
			RunningApp.WaitForElement("Instructions");
		}
	}
}