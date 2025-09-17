using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue30350 : _IssuesUITest
    {
        public Issue30350(TestDevice device) : base(device)
        {
        }

        public override string Issue => "IImage downsize broken starting from 9.0.80 and not fixed in 9.0.81";

        [Test]
        [Category(UITestCategories.Image)]
        public void DownSizeImageAppearProperly()
        {
            App.WaitForElement("WaitForStubControl");
            VerifyScreenshot();
        }
    }
}
