using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue19997 : _IssuesUITest
    {
        public Issue19997(TestDevice device) : base(device)
        {
        }

        public override string Issue => "[Android, iOS, MacOS]Entry ClearButton Color Not Updating on Theme Change";

        [Test]
        [Category(UITestCategories.Entry)]
        public void EntryClearButtonColorShouldUpdateOnThemeChange()
        {
            App.WaitForElement("EntryWithAppThemeBinding");
            App.Tap("EntryWithAppThemeBinding");
            App.Tap("ThemeButton");
            VerifyScreenshot();
        }
    }
}