using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
    public class Issue2681 : IssuesUITest
	{
		const string NavigateToPage = "Click Me.";

		public Issue2681(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[UWP] Label inside Listview gets stuck inside infinite loop";
		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.ListView)]
		public void ListViewDoesntFreezeApp()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Mac]);

			RunningApp.Tap(NavigateToPage);
			RunningApp.WaitForNoElement("3");
		}
	}
}