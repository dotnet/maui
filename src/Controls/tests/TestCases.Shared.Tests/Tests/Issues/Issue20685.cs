#if MACCATALYST || WINDOWS// MenuBarItems are only supported on desktop platforms
using System;
using System.Threading.Tasks;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue20685 : _IssuesUITest
    {
        public Issue20685(TestDevice testDevice) : base(testDevice)
        {
        }

        public override string Issue => "MenuBarItem Commands not working on Mac Catalyst";

        [Test]
        [Category(UITestCategories.Shell)]
        public void MenuFlyoutItem_ClickedEventWorks()
        {
            // Click on the clicked events menu item
            App.WaitForElement("Clicked Events");
            App.Tap("Clicked Events");
            App.WaitForElement("Clicked Event Item");
            App.Tap("Clicked Event Item");

            // Verify the result is displayed correctly
            Assert.That(App.WaitForElement("ResultLabel")?.GetText(), Is.EqualTo("Clicked event handler executed"));
        }

        [Test]
        [Category(UITestCategories.Shell)]
        public void MenuFlyoutItem_CommandWorks()
        {
            // Click on the commands menu item
            App.WaitForElement("Commands");
            App.Tap("Commands");
            //App.TapMenuBarItem("Commands");
            App.WaitForElement("Command Item");
            App.Tap("Command Item");

            // Verify the result is displayed correctly
            Assert.That(App.WaitForElement("ResultLabel")?.GetText(), Is.EqualTo("Command executed"));
        }

        [Test]
        [Category(UITestCategories.Shell)]
        public void MenuFlyoutItem_CommandWithParameterWorks()
        {
            // Click on the commands with parameter menu item
            App.WaitForElement("Command With Param");
            App.Tap("Command With Param");
            //App.TapMenuBarItem("Command With Param");
            App.WaitForElement("Command with Parameter");
            App.Tap("Command with Parameter");

            // Verify the result is displayed correctly
            Assert.That(App.WaitForElement("ResultLabel")?.GetText(), Is.EqualTo("Command executed with parameter: Test Parameter"));
        }

       
    }
}
#endif
