#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class HamburgerIconTest : _IssuesUITest
	{
		public override string Issue => "FlyoutTest";

		public HamburgerIconTest(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.ActivityIndicator), Order(1)]
		public void VerifyHamburgerIcon()
		{
			App.WaitForElement("Tab1Page");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.ActivityIndicator), Order(2)]
		public void VerifyFlyoutBackgroundColor()
		{
			App.WaitForElement("Tab1Page");
			App.Tap("ChangeFlyoutBackground");
#if ANDROID
			App.Tap(AppiumQuery.ByXPath("//android.widget.ImageButton[@content-desc='Open navigation drawer']"));
#else
			App.Tap(FlyoutIconAutomationId);
#endif
			VerifyScreenshot();
		}
	}
}
#endif