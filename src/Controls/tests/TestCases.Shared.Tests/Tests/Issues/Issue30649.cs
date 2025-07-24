#if !WINDOWS // The fix is related with Android, iOS and Mac.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue30649 : _IssuesUITest
	{
		public Issue30649(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "GraphicsView event handlers should respect IsEnabled property";

		[Test]
		[Category(UITestCategories.GraphicsView)]
		[Category(UITestCategories.IsEnabled)]
		[Category(UITestCategories.Gestures)]
		public void GraphicsViewEventsRespectIsEnabled()
		{
			// Wait for the page to load
			App.WaitForElement("TestGraphicsView");
			App.WaitForElement("EventCountLabel");
			App.WaitForElement("ToggleIsEnabledButton");
			App.WaitForElement("ResetCountButton");

			// Reset the count to start fresh
			App.Click("ResetCountButton");

			// Verify initial state shows 0 events
			var initialCount = App.FindElement("EventCountLabel").GetText();
			Assert.That(initialCount, Is.EqualTo("Touch Events: 0"));

			// Tap the GraphicsView when enabled - should trigger events
			App.Click("TestGraphicsView");
			
			// Wait a moment for the event to be processed
			System.Threading.Thread.Sleep(500);
			
			var countAfterFirstTap = App.FindElement("EventCountLabel").GetText();
			Assert.That(countAfterFirstTap, Is.EqualTo("Touch Events: 1"), 
				"GraphicsView should respond to touch when IsEnabled is true");

			// Disable the GraphicsView
			App.Click("ToggleIsEnabledButton");
			
			// Verify button text changed to indicate GraphicsView is now disabled
			var disableButtonText = App.FindElement("ToggleIsEnabledButton").GetText();
			Assert.That(disableButtonText, Is.EqualTo("Enable GraphicsView"));

			// Tap the GraphicsView when disabled - should NOT trigger events
			App.Click("TestGraphicsView");
			
			// Wait a moment to ensure no event is processed
			System.Threading.Thread.Sleep(500);
			
			var countAfterDisabledTap = App.FindElement("EventCountLabel").GetText();
			Assert.That(countAfterDisabledTap, Is.EqualTo("Touch Events: 1"), 
				"GraphicsView should NOT respond to touch when IsEnabled is false");

			// Re-enable the GraphicsView
			App.Click("ToggleIsEnabledButton");
			
			// Verify button text changed back
			var enableButtonText = App.FindElement("ToggleIsEnabledButton").GetText();
			Assert.That(enableButtonText, Is.EqualTo("Disable GraphicsView"));

			// Tap the GraphicsView when re-enabled - should trigger events again
			App.Click("TestGraphicsView");
			
			// Wait a moment for the event to be processed
			System.Threading.Thread.Sleep(500);
			
			var countAfterReenabledTap = App.FindElement("EventCountLabel").GetText();
			Assert.That(countAfterReenabledTap, Is.EqualTo("Touch Events: 2"), 
				"GraphicsView should respond to touch again when IsEnabled is set back to true");
		}
	}
}
#endif