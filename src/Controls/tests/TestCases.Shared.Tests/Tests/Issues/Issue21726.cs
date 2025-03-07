#if IOS //This test case verifies that the sample is working exclusively on IOS platforms "due to use of UIKit APIs".
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue21726 : _IssuesUITest
    {
        public Issue21726(TestDevice device) : base(device)
        {
        }

        public override string Issue => "Modal with a bottom sheet should not crash iOS Keyboard Scroll";

        [Test]
		[Category(UITestCategories.SoftInput)]
		public void PushViewControllerWithNullWindow()
        {
            App.WaitForElement("AddVC");
            App.Click("AddVC");
            App.WaitForElement("TextField1").Click();
            App.WaitForElement("Button1").Click();
            var mainPageElement = App.WaitForElement("AddVC");
            ClassicAssert.NotNull(mainPageElement);
        }
    }
}
#endif