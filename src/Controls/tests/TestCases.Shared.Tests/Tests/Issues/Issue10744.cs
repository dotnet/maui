
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue10744 : _IssuesUITest
	{
		public Issue10744(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] WebView.Eval crashes on Android with long string";

		[Test]
		[Category(UITestCategories.WebView)]
		[Category(UITestCategories.Compatibility)]
		public void WebViewEvalCrashesOnAndroidWithLongString()
		{
			App.WaitForElement("navigatedLabel");
		}
		public override void TestSetup()
		{
			base.TestSetup();

			try
			{
				App.WaitForElement("NoInternetAccessLabel", timeout: TimeSpan.FromSeconds(1));
				Assert.Inconclusive("This device doesn't have internet access");
			}
			catch (TimeoutException)
			{
				// Element not found within timeout, assume internet is available
				// Continue with the test
			}
		}
	}
}
 