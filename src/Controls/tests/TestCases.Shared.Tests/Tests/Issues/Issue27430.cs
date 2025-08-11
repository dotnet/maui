using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue27430 : _IssuesUITest
    {
        public Issue27430(TestDevice device) : base(device) { }

        public override string Issue => "False positives of PointerGestureRecognizer on Windows with a second minimized window";

        [Test]
        [Category(UITestCategories.Gestures)]
        [Category(UITestCategories.Windows)]
#if !WINDOWS
        [Ignore("This test is specific to Windows platform where the issue occurs")]
#endif
        public void PointerGestureRecognizerElementsArePresent()
        {
            // Verify the test page loads with all required elements
            App.WaitForElement("TestGrid");
            App.WaitForElement("EventCountLabel");
            App.WaitForElement("LastEventLabel");
            App.WaitForElement("OpenWindowButton");

            // Verify initial state
            var eventCountLabel = App.FindElement("EventCountLabel");
            Assert.That(eventCountLabel.GetText(), Is.EqualTo("Events: 0 Enter, 0 Exit"));

            var lastEventLabel = App.FindElement("LastEventLabel");
            Assert.That(lastEventLabel.GetText(), Is.EqualTo("Last Event: None"));
        }

        [Test]
        [Category(UITestCategories.Gestures)]
        [Category(UITestCategories.Windows)]
#if !WINDOWS
        [Ignore("This test is specific to Windows platform where the issue occurs")]
#endif
        public void SecondWindowCanBeOpened()
        {
            // Verify we can open second window without crashes
            App.WaitForElement("OpenWindowButton");
            
            var openButton = App.FindElement("OpenWindowButton");
            Assert.That(openButton.GetText(), Is.EqualTo("Open Second Window"));

            // Open second window
            App.Tap("OpenWindowButton");

            // Give it a moment for the window operation to complete
            System.Threading.Thread.Sleep(1000);

            // Verify the main UI is still responsive
            App.WaitForElement("TestGrid");
            App.WaitForElement("EventCountLabel");
        }

        [Test]
        [Category(UITestCategories.Gestures)]
        [Category(UITestCategories.Windows)]
#if !WINDOWS
        [Ignore("This test is specific to Windows platform where the issue occurs")]
#endif
        public void InstructionsAreDisplayed()
        {
            // Verify instructions explain the test scenario
            App.WaitForElement("InstructionsLabel");
            var instructions = App.FindElement("InstructionsLabel");
            
            Assert.That(instructions.GetText(), Does.Contain("Hover over the grid"));
            Assert.That(instructions.GetText(), Does.Contain("Open Second Window"));
            Assert.That(instructions.GetText(), Does.Contain("should NOT blink"));
        }

        [Test]
        [Category(UITestCategories.Gestures)]
        [Category(UITestCategories.Windows)]
#if !WINDOWS
        [Ignore("This test is specific to Windows platform where the issue occurs")]
#endif
        public void TestGridIsProperlyConfigured()
        {
            // Verify test grid is present and has the expected properties
            App.WaitForElement("TestGrid");
            var testGrid = App.FindElement("TestGrid");
            Assert.That(testGrid, Is.Not.Null);
            
            // The grid should be visible and interactable
            Assert.That(testGrid.IsEnabled(), Is.True);
        }
    }
}