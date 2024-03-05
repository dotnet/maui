using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue12134 : IssuesUITest
	{
		public Issue12134(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] WkWebView does not handle cookies consistently";

		[Test]
		[Category(UITestCategories.WebView)]
		public void CookiesCorrectlyLoadWithMultipleWebViews()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			for (int i = 0; i < 10; i++)
			{
				RunningRunningApp.WaitForNoElement("Success", $"Failied on: {i}");
				RunningApp.Tap("LoadNewWebView");
			}
		}
	}
}