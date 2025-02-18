#if TEST_FAILS_ON_ANDROID // Pdf Not Loading on Android Issue: https://github.com/dotnet/maui/issues/14184
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18716 : _IssuesUITest
	{
		public Issue18716(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Touch events are not working on WebView when a PDF is displayed";

		 
  		 
		[Test]
		[Category(UITestCategories.WebView)]
		public async Task CanScrollWebView()
		{
			VerifyInternetConnectivity();
			await Task.Delay(1000); // Wait WebView to load.

			App.WaitForElement("WaitForStubControl");
			App.ScrollDown("WaitForStubControl", ScrollStrategy.Gesture, 0.75);
			Thread.Sleep(1000);
			VerifyScreenshot();
		}
	}
}
#endif
 