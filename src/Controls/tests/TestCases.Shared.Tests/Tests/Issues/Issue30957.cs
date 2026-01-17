using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue30957 : _IssuesUITest
    {
        public Issue30957(TestDevice testDevice) : base(testDevice)
        {
        }

        public override string Issue => "FlexLayout Wrap Misalignment with Dynamically-Sized Buttons in .NET MAUI";

        [Test]
        [Category(UITestCategories.Layout)]
        public void FlexLayoutWrappingWithToleranceWorksCorrectly()
        {

            App.WaitForElement("Issue30957ToggleButton");
            App.Tap("Issue30957ToggleButton");
            App.WaitForElement("Issue30957Button1");
            App.WaitForElement("Issue30957Button2");
            App.WaitForElement("Issue30957Button3");
            VerifyScreenshot();
        }
    }
}