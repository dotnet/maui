using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue1583_1 : IssuesUITest
	{
		public Issue1583_1(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "WebView fails to load from urlwebviewsource with non-ascii characters (works with Uri)";

		[Test]
		[Category(UITestCategories.WebView)]
		public async Task Issue1583_1_WebviewTest()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("label", "Could not find label", TimeSpan.FromSeconds(10), null, null);
			await Task.Delay(TimeSpan.FromSeconds(3));
			RunningApp.Screenshot("I didn't crash and i can see Skøyen");
			RunningApp.Tap("hashButton");
			await Task.Delay(TimeSpan.FromSeconds(3));
			RunningApp.Screenshot("I didn't crash and i can see the GitHub comment #issuecomment-389443737");
			RunningApp.Tap("queryButton");
			await Task.Delay(TimeSpan.FromSeconds(3));
			RunningApp.Screenshot("I didn't crash and i can see google search for http://microsoft.com");
		}
	}
}