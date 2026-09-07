// iOS/MacCatalyst only: The fix updates UINavigationItem.Title for back-stack pages,
// which iOS uses to display the back button text. This is an Apple platform-specific behavior.
#if IOS || MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue35471 : _IssuesUITest
	{
		public override string Issue => "iOS Shell back button history menu does not update after runtime culture change";

		public Issue35471(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Shell)]
		public void ShellBackButtonHistoryUpdatesAfterTitleChange()
		{
			// iOS 26+ no longer exposes the back button title text via accessibility,
			// so there is no reliable way to assert the updated title in a UI test.
			if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
			{
				Assert.Ignore("iOS 26+ does not expose back button title text via accessibility");
			}

			// Navigate to Detail page
			App.WaitForElement("NavigateToDetail");
			App.Tap("NavigateToDetail");

			// Change the previous page's title (simulates runtime culture change)
			App.WaitForElement("ChangePreviousPageTitle");
			App.Tap("ChangePreviousPageTitle");

			// Verify back button now shows updated title "Accueil"
			// Without the fix, UINavigationItem.Title is stale and still shows "Home"
			App.WaitForElement("Accueil");
			App.Tap("Accueil");
			App.WaitForElement("RootPageLabel");
		}
	}
}
#endif
