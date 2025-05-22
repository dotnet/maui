using NUnit.Framework;
using OpenQA.Selenium;
using UITest.Appium;
using UITest.Core;

#if MACCATALYST
namespace Microsoft.Maui.TestCases.Tests.Tests.Issues;

public class Issue28945: _IssuesUITest
{
    public Issue28945(TestDevice device) : base(device)
    {
    }

    public override string Issue => "Add Focus propagation to MauiView";

    [Test]
    [System.ComponentModel.Category(UITestCategories.Layout)]
    public void ContentViewPropagatesFocus()
    {
        App.WaitForElement("WaitForStubControl");

        if (App is not AppiumApp app)
            return;

        // https://developer.apple.com/documentation/xctest/xcuikeyboardkey?language=objc
        string[] keys = ["XCUIKeyboardKeyTab"]; // Tab Key

        app.Driver.ExecuteScript("macos: keys", new Dictionary<string, object>
        {
            { "keys", keys },
        });
        
        App.Tap("WaitForStubControl");
        
        VerifyScreenshot();
    }
}
#endif