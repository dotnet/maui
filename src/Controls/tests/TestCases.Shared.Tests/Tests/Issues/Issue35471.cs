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
			App.WaitForElement("NavigateToDetail");
			App.Tap("NavigateToDetail");

			// Change the previous page's title (simulates runtime culture change)
			App.WaitForElement("ChangePreviousPageTitle");
			App.Tap("ChangePreviousPageTitle");

			// The back button should now show "Accueil" (updated title).
			// Without the fix, it still shows "Home" (stale title).
			if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
			{
				App.TapBackArrow(); // iOS 26+: no text on back button
			}
			else
			{
				App.TapBackArrow("Accueil");
			}

			// Verify we navigated back to root
			App.WaitForElement("RootPageLabel");
		}
	}
}
#endif
