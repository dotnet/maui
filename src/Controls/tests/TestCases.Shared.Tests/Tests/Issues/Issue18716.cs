using NUnit.Framework; // Test Case passed but pdf not loading on Android More Information:https://github.com/dotnet/maui/issues/14184
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

		 
  		//There's an issue getting the mouse interactions to work with Appium.
		[Test]
		[Category(UITestCategories.WebView)]
		public async Task CanScrollWebView()
		{
			await Task.Delay(1300); // Wait WebView to load.

			App.WaitForElement("WaitForStubControl");
			App.ScrollDown("WaitForStubControl", ScrollStrategy.Gesture, 0.75);
			App.Screenshot("Scrolling has been done correctly.");
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
