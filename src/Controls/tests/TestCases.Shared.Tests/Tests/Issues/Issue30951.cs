using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue30951 : _IssuesUITest
    {
        public Issue30951(TestDevice device) : base(device)
        {
        }

        public override string Issue => "Fix Android ScrollView to measure content correctly";

        [Test]
        [Category(UITestCategories.ScrollView)]
        public void Issue30951_ScrollViewContentMeasurementFix()
        {
            App.WaitForElement("Issue30951_MealTypeLabel");
            VerifyScreenshot();
        }
    }
}

