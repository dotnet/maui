using NUnit.Framework;
using UITest.Appium;

namespace UITests.Tests.Issues
{
	public class Issue2354 : IssuesUITest
	{
		public Issue2354(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ListView, ImageCell and disabled source cache and same image url";

		[Test]
		[Category(UITestCategories.ListView)]
		public void TestDoesntCrashWithCachingDisable()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("ImageLoaded");
			App.ScrollDown("TestListView", ScrollStrategy.Programmatically);
			App.ScrollDown("TestListView", ScrollStrategy.Programmatically);
		}
	}
}