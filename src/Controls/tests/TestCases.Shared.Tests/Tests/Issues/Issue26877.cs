using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue26877 : _IssuesUITest
    {
        public Issue26877(TestDevice testDevice) : base(testDevice)
        {
        }

        public override string Issue => "The iOS platform page cannot scroll to the bottom";

        [Test]
        [Category(UITestCategories.Shape)]
        [Category(UITestCategories.ScrollView)]
        public void ScrollToBottom()
        {
            App.WaitForElement("ScrollToBottomPage");
            App.ScrollTo("Label");
            App.WaitForElement("Label");
        }
    }
}
