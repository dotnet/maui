using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27337 : _IssuesUITest
	{
		public override string Issue => "Flyout with FlyoutBehavior='Locked' unlocks (reverts to Flyout) after navigation";

		public Issue27337(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Shell)]
		public void EnsureFlyoutBehaviourAsLockedAfterNavigation()
		{
			App.WaitForElement("SecondPageButton");
			App.Tap("SecondPageButton");
			App.Back();
			VerifyNoFlyoutIconAfterBackNavigation();
		}

		void VerifyNoFlyoutIconAfterBackNavigation()
		{
#if ANDROID
			App.WaitForNoElement(AppiumQuery.ByXPath("//android.widget.ImageButton[@content-desc=\"Open navigation drawer\"]"));
#elif IOS || MACCATALYST
			App.WaitForNoElement("OK");
#elif WINDOWS
			App.WaitForNoElement(AppiumQuery.ByAccessibilityId("TogglePaneButton"));
#endif
		}
	}
}