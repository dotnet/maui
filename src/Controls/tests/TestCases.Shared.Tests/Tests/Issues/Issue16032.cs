#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue16032 : _IssuesUITest
	{
		public Issue16032(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Improve the customization of WebView on Android";

		[Test]
		[Category(UITestCategories.WebView)]
		public void EnsureSupportForCustomWebViewClients()
		{
			App.WaitForElement("Success");
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
#endif