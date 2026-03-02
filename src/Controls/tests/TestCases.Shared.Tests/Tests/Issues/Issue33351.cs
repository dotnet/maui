using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33351 : _IssuesUITest
{
    public Issue33351(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "Changing Shell Tab Visibility when navigating back multiple pages ignores Shell Tab Visibility";

    [Test]
    [Category(UITestCategories.Shell)]
    public void TabBarVisibilityAfterMultiLevelPopToRoot()
    {
        App.WaitForElement("Tab 1");
        App.Tap("Tab 1");
        
        App.WaitForElement("PushPage1Button");
        App.Tap("PushPage1Button");
        
        App.WaitForElement("PushPage2Button");
        App.Tap("PushPage2Button");
        
        App.WaitForElement("PopToRootButton");
        App.Tap("PopToRootButton");
        
        App.WaitForElement("Tab 1");
        App.Tap("Tab 1");
        
        App.WaitForElement("TabBarVisibleLabel");
        
        VerifyScreenshot();
    }
}
