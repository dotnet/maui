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
            App.WaitForElement("Issue30957Button1");
            App.WaitForElement("Issue30957Button2");
            App.WaitForElement("Issue30957Button3");

            var button1Initial = App.WaitForElement("Issue30957Button1").GetRect();
            var button2Initial = App.WaitForElement("Issue30957Button2").GetRect();
            var button3Initial = App.WaitForElement("Issue30957Button3").GetRect();

            Assert.That(button2Initial.X, Is.GreaterThan(button1Initial.X),
                "Button2 should be positioned to the right of Button1 initially");
            Assert.That(button3Initial.X, Is.GreaterThan(button2Initial.X),
                "Button3 should be positioned to the right of Button2 initially");

            Assert.That(Math.Abs(button1Initial.Y - button2Initial.Y), Is.LessThan(10),
                "Button1 and Button2 should be on same row initially");
            Assert.That(Math.Abs(button2Initial.Y - button3Initial.Y), Is.LessThan(10),
                "Button2 and Button3 should be on same row initially");


            App.Tap("Issue30957ToggleButton");

            var button1Final = App.WaitForElement("Issue30957Button1").GetRect();
            var button2Final = App.WaitForElement("Issue30957Button2").GetRect();
            var button3Final = App.WaitForElement("Issue30957Button3").GetRect();

            Assert.That(Math.Abs(button1Final.Y - button2Final.Y), Is.LessThan(10),
                "CRITICAL: After font change, Button1 and Button2 should STILL be on same row (tolerance fix prevents unwanted wrapping)");
            Assert.That(Math.Abs(button1Final.Y - button3Final.Y), Is.LessThan(10),
                "CRITICAL: After font change, Button1 and Button3 should STILL be on same row (tolerance fix prevents unwanted wrapping)");
            Assert.That(Math.Abs(button2Final.Y - button3Final.Y), Is.LessThan(10),
                "CRITICAL: After font change, Button2 and Button3 should STILL be on same row (tolerance fix prevents unwanted wrapping)");

            Assert.That(button2Final.X, Is.GreaterThan(button1Final.X),
                "After font change, Button2 should still be to the right of Button1");
            Assert.That(button3Final.X, Is.GreaterThan(button2Final.X),
                "After font change, Button3 should still be to the right of Button2");

            var statusText = App.WaitForElement("Issue30957StatusLabel").GetText();
            Assert.That(statusText, Does.Contain("Font toggled"),
                "Status should show font was toggled");
        }
    }
}