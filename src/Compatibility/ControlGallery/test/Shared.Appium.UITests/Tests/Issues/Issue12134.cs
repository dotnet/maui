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
		[Category(UITestCategories.RequiresInternetConnection)]
		[FailsOnAndroid]
		public void CookiesCorrectlyLoadWithMultipleWebViews()
		{
			for (int i = 0; i < 10; i++)
			{
				RunningApp.WaitForNoElement("Success", $"Failied on: {i}");
				RunningApp.Tap("LoadNewWebView");
			}
		}
	}
}