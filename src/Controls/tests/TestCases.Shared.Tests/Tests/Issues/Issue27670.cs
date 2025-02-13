#if IOS // Failed on other platforms as the modal page presentation style is supported only on the iOS platform.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue27670 : _IssuesUITest
    {
        public override string Issue => "NavigationPage fails to push the modal page via PushModalAsync after using PageSheet";

        public Issue27670(TestDevice device) : base(device)
        {
        }

        [Test]
        [Category(UITestCategories.Shell)]
        public void ShouldPushNavigationPageUsingPageSheet()
        {
            App.WaitForElement("Button");
            App.Tap("Button");
            App.Tap("ModalButton");
            App.Tap("NavigateButton");
            App.WaitForElement("Label");
            var rect = App.FindElement("ModalButton").GetRect();
            var centerX = rect.X + rect.Width / 2;
            var centerY = rect.Y + rect.Height / 2;
            App.DragCoordinates(centerX, centerY, centerX, centerY + 500);
            App.Tap("ModalButton");
            VerifyScreenshot();
        }
    }
}
#endif