using Xunit;
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

		[Fact]
		[Trait("Category", UITestCategories.WebView)]
		[Trait("Category", UITestCategories.Compatibility)]

		public void CookiesCorrectlyLoadWithMultipleWebViews()
		{
			VerifyInternetConnectivity();
			for (int i = 0; i < 10; i++)
			{
				App.WaitForElement("Success", $"Failied on: {i}");
				App.Tap("LoadNewWebView");
			}
		}
	}
}