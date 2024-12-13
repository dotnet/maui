using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla32801 : _IssuesUITest
{
        #if ANDROID
        const string Tab1 = "TAB 1";    
        #else
        const string Tab1 = "Tab 1";
        #endif
        const string AddButton = "btnAdd";
        const string StackButton = "btnStack";

        public Bugzilla32801(TestDevice testDevice) : base(testDevice)
        {
        }

        public override string Issue => "Memory Leak in TabbedPage + NavigationPage";

        [Test]
        [Category(UITestCategories.TabbedPage)]
        public void Bugzilla32801Test()
        {
                App.WaitForElement(AddButton);
                App.Tap(AddButton);
                App.WaitForElementTillPageNavigationSettled(AddButton);
                App.Tap(AddButton);
                App.WaitForElementTillPageNavigationSettled(StackButton);
                App.Tap(StackButton);
                App.WaitForElement("Stack 3");
                App.Tap(Tab1);
                App.Tap(StackButton);
                App.WaitForElement("Stack 1");
        }
}