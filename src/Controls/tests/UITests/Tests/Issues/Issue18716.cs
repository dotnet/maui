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
		public void CanScrollWebView()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows });

			App.WaitForElement("WaitForStubControl");
			App.ScrollDown("WaitForStubControl", ScrollStrategy.Gesture);
			App.Screenshot("Scrolling has been done correctly.");
		}
	}
}
