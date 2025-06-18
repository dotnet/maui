#if WINDOWS // mac does not respond to the window size change. so, ignored test on mac.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue27223 : _IssuesUITest
    {
        public Issue27223(TestDevice testDevice) : base(testDevice)
        {
        }

        public override string Issue => "SizeChanged event fires when size hasn't changed";

        [Test]
        [Category(UITestCategories.Layout)]
        public void SizeChangedOnlyFiresWhenSizeChanges()
        {
            App.WaitForElement("Label");
            var initialValue = App.FindElement("Label").GetText();
            App.Tap("Button1");
            var finalValue = App.FindElement("Label").GetText();
            Assert.That(initialValue, Is.EqualTo(finalValue));
        }
    }
}
#endif
