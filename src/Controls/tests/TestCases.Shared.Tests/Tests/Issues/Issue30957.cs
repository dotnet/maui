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
            App.WaitForElement("ToggleFontButton");
            App.WaitForElement("Button1");
            App.WaitForElement("Button2");
            App.WaitForElement("Button3");

            var button1Initial = App.WaitForElement("Button1").GetRect();
            var button2Initial = App.WaitForElement("Button2").GetRect();
            var button3Initial = App.WaitForElement("Button3").GetRect();

            Assert.That(button2Initial.X, Is.GreaterThan(button1Initial.X),
                "Button2 should be positioned to the right of Button1 initially");
            Assert.That(button3Initial.X, Is.GreaterThan(button2Initial.X),
                "Button3 should be positioned to the right of Button2 initially");

            App.Tap("ToggleFontButton");

            var button1Final = App.WaitForElement("Button1").GetRect();
            var button2Final = App.WaitForElement("Button2").GetRect();
            var button3Final = App.WaitForElement("Button3").GetRect();

            Assert.That(button2Final.X, Is.GreaterThan(button1Final.X),
                "After font toggle, Button2 should still be to the right of Button1");

            var statusText = App.WaitForElement("StatusLabel").GetText();
            Assert.That(statusText, Does.Contain("Font toggled"),
                "Status should show font was toggled");
        }
    }
}
