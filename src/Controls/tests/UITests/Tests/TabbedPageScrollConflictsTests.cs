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
			if (Device == TestDevice.Android)
			{
				App.WaitForElement("Tab1Element");

				App.SwipeRightToLeft(e => e.Marked("Tab1Element").Child("android.webkit.WebView").Child("android.widget.TextView"));

				var element = App.Query("Tab1Label").FirstOrDefault();
				Assert.True(element != null && element.Text.Equals("Passed", StringComparison.OrdinalIgnoreCase));
			}
			else
			{
				Assert.Ignore("This test only runs on Android");
			}
		}
	}
}
