using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12652 : _IssuesUITest
{

    const string Top3 = "Top 3";

    public Issue12652(TestDevice testDevice) : base(testDevice)
    {
    }
    
    public override string Issue => "[Bug] NullReferenceException in the Shell on UWP when navigating back to Shell Section with multiple content items";
    
    [Test]
    [Category(UITestCategories.Shell)]
    public void NavigatingBackToAlreadySelectedTopTabDoesntCrash()
    {
        App.TapTab(Top3);
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