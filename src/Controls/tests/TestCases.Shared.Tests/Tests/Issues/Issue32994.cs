using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32994 : _IssuesUITest
{
        public Issue32994(TestDevice device) : base(device)
        {
        }

        public override string Issue => "Shell TabBarIsVisible binding not working on ShellContent";

        [Test, Order(1)]
        [Category(UITestCategories.Shell)]
        public void TabBarVisibilityHidesOnPage1UsingDirectSet()
        {
                App.WaitForElement("HidePage1TabBar");
                App.Tap("HidePage1TabBar");
                VerifyScreenshot();
        }

        [Test, Order(2)]
        [Category(UITestCategories.Shell)]
        public void TabBarVisibilityShowsOnPage1UsingDirectSet()
        {
                App.WaitForElement("ShowPage1TabBar");
                App.Tap("ShowPage1TabBar");
                App.WaitForElement("Tab1");
        }

        [Test, Order(3)]
        [Category(UITestCategories.Shell)]
        public void TabBarVisibilityShowsOnPage2UsingBinding()
        {
#if WINDOWS
                // In Windows, multiple shell contents on the same tab are displayed in a dropdown,
                // requiring the tab to be clicked first before selecting the specific shell content
                App.WaitForElement("ShowPage2TabBar");
                App.Tap("ShowPage2TabBar");
                App.TapTab("Tab1");
                App.WaitForElement("Page2");
                App.Tap("Page2");
                App.WaitForElement("Tab1");
#else
                App.WaitForElement("ShowPage2TabBar");
                App.Tap("ShowPage2TabBar");
                App.TapTab("Page2");
                App.WaitForElement("Tab1");
#endif
        }

        [Test, Order(4)]
        [Category(UITestCategories.Shell)]
        public void TabBarVisibilityHidesOnPage2UsingBinding()
        {
#if WINDOWS
                App.TapTab("Tab1");
                App.WaitForElement("Page1");
                App.Tap("Page1");
                App.WaitForElement("HidePage2TabBar");
                App.Tap("HidePage2TabBar");
                App.TapTab("Tab1");
                App.WaitForElement("Page2");
                App.Tap("Page2");
                VerifyScreenshot();
#else
                App.TapTab("Page1");
                App.WaitForElement("HidePage2TabBar");
                App.Tap("HidePage2TabBar");
                App.TapTab("Page2");
                VerifyScreenshot();
#endif
        }
}
