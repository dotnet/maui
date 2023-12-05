using Maui.Controls.Sample;
using NUnit.Framework;
using UITest.Core;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.MultiTouch;
using UITest.Appium;

namespace Microsoft.Maui.AppiumTests
{
	public class NavBarTranslucentTest : UITest
	{
		const string KeyboardScrollingGallery = "NavBarTranslucent Gallery";
		public NavBarTranslucentTest(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(KeyboardScrollingGallery);
		}

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			this.Back();
		}

		[Test]
		public void NavigationBarIsTranslucent()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows }, "This test is only for iOS");

            var boxView = App.WaitForElement("TopBoxView");
            Assert.NotNull(boxView);
			var rect = boxView.GetRect();
            Assert.AreEqual(rect.Y, 0);
		}
	}
}
