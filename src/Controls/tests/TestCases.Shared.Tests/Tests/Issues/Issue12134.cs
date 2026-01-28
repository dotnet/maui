using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue12134 : _IssuesUITest
	{
		public Issue12134(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] WkWebView does not handle cookies consistently";

		[Test]
		[Category(UITestCategories.WebView)]
		public void CookiesCorrectlyLoadWithMultipleWebViews()
		{
			VerifyInternetConnectivity();
			for (int i = 0; i < 10; i++)
			{
				// WebView loading and cookie evaluation can take time, especially on slow networks
				App.WaitForElement("Success", $"Failed on iteration: {i}", timeout: TimeSpan.FromSeconds(30));
				App.Tap("LoadNewWebView");
			}
		}
	}
}