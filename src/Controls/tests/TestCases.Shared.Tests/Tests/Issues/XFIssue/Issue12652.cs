using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12652 : _IssuesUITest
{
#if ANDROID
	const string Top3 = "TOP 3";
#else
    const string Top3 = "Top 3";
#endif
	public Issue12652(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] NullReferenceException in the Shell on UWP when navigating back to Shell Section with multiple content items";

	[Test]
	[Category(UITestCategories.Shell)]
	public void NavigatingBackToAlreadySelectedTopTabDoesntCrash()
	{
		// On the Windows platform, there is a dropdown menu that has to be accessed to view the "TopTab" choices.
		// The "navViewItem" tap emulates user actions to open the dropdown and display the TopTab for navigation.
#if WINDOWS
        App.Tap("navViewItem");
#endif
		App.Tap(Top3);
		App.WaitForElement("TopTabPage3");
		App.Tap("Main 2");
		App.WaitForElement("TopTabPage2");
		App.Tap("Main 1");
		App.WaitForElement("TopTabPage3");

		// Once the actions are completed, the dropdown is closed to restore the original view.
		// Tapping at coordinates (50, 50) simulates a click outside the dropdown, triggering it to close.
#if WINDOWS
		App.TapCoordinates(50, 50);
#endif
		App.Tap("Main 2");
		App.WaitForElement("TopTabPage2");
		App.Tap("Main 1");
		App.WaitForElement("TopTabPage3");
	}
}