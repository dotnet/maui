using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	public class LayoutUITests : UITest
	{
		const string LayoutGallery = "Layout Gallery";

		public LayoutUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(LayoutGallery);
		}

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			this.Back();
		}

		[Test]
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