using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
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
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows });

			await Task.Delay(1000); // Wait WebView to load.

			App.WaitForElement("WaitForStubControl");
			App.ScrollDown("WaitForStubControl", ScrollStrategy.Gesture, 0.75);
			App.Screenshot("Scrolling has been done correctly.");
		}
	}
}
