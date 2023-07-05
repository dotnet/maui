using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests
{
	public class TabbedPageScrollConflictsTests : UITestBase
	{
		const string TabbedPageScrollConflictsGallery = "* marked:'TabbedPage ScrollConflicts Gallery'";

		public TabbedPageScrollConflictsTests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(TabbedPageScrollConflictsGallery);
		}

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			App.NavigateBack();
		}

		[Test]
		public void NoScrollConflicts()
		{
			if (UITestContext.TestConfig.TestDevice == TestDevice.Android)
			{
				App.WaitForElement("WebViewElement");
				App.SwipeRightToLeft("WebViewElement");
			}
			else
			{
				Assert.Ignore("This test only runs on Android");
			}
		}
	}
}
