using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.Layout)]
	public class ScrollViewScrollToUITests : LayoutUITests
	{
		public ScrollViewScrollToUITests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		[Description("Scroll to the end using ScrollToAsync method")]
		public async Task ScrollViewScrollTo()
		{
			App.Click("ScrollViewScrollTo");
			App.WaitForElement("TestScrollView");

			// 1. Tap the Button to scroll to the end.
			App.Click("ScrollToEndButton");

			// Wait for the ScrollView to complete the scroll operation.
			await Task.Delay(1000);

			// 2.With a snapshot we verify that the ScrollView scrolled to the end.
			VerifyScreenshot();
		}
	}
}