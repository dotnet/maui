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

            var buttonRectsInitial = new[] {
                App.WaitForElement("Issue30957Button1").GetRect(),
                App.WaitForElement("Issue30957Button2").GetRect(),
                App.WaitForElement("Issue30957Button3").GetRect()
            };

            for (int i = 0; i < buttonRectsInitial.Length - 1; i++)
            {
                Assert.That(buttonRectsInitial[i+1].X, Is.GreaterThan(buttonRectsInitial[i].X), $"Button{i+2} should be right of Button{i+1} (initial)");
                Assert.That(Math.Abs(buttonRectsInitial[i].Y - buttonRectsInitial[i+1].Y), Is.LessThan(10), $"Button{i+1} and Button{i+2} should be on the same row (initial)");
            }

            App.Tap("Issue30957ToggleButton");

            var buttonRectsFinal = new[] {
                App.WaitForElement("Issue30957Button1").GetRect(),
                App.WaitForElement("Issue30957Button2").GetRect(),
                App.WaitForElement("Issue30957Button3").GetRect()
            };

            for (int i = 0; i < buttonRectsFinal.Length - 1; i++)
            {
                Assert.That(Math.Abs(buttonRectsFinal[i].Y - buttonRectsFinal[i+1].Y), Is.LessThan(10), $"Button{i+1} and Button{i+2} should remain on the same row after font change");
                Assert.That(buttonRectsFinal[i+1].X, Is.GreaterThan(buttonRectsFinal[i].X), $"Button{i+2} should be right of Button{i+1} after font change");
            }

            var statusText = App.WaitForElement("Issue30957StatusLabel").GetText();
            Assert.That(statusText, Does.Contain("Font toggled"), "Status label should indicate font was toggled");
        }
    }
}