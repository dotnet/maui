using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue22306_1 : _IssuesUITest
    {
        public Issue22306_1(TestDevice device) : base(device) { }

        public override string Issue => "Button with images measures correctly";

        [Test]
        [Category(UITestCategories.Button)]
        public void ButtonMeasuresLargerThanDefault()
        {
            var rotateButtonRect = App.WaitForElement("RotateButton").GetRect();
            var button3Rect = App.WaitForElement("Button3").GetRect();

            Assert.That(rotateButtonRect.Height > button3Rect.Height, $"rotateButtonRect.Height is {rotateButtonRect.Height} is not greater than button3Rect.Height {button3Rect.Height}");
        }

        [Test]
        [Category(UITestCategories.Button)]
        public void ButtonLayoutResizesWithImagePosition()
        {
            this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

            App.WaitForElement("RotateButton");
            VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Top");

            App.WaitForElement("RotateButton").Tap();
            VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Right");

            App.WaitForElement("RotateButton").Tap();
            VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Bottom");

            App.WaitForElement("RotateButton").Tap();
            VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Left");
        }
    }
}
