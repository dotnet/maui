using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class IssueCarouselViewNoLoopTapGesture : _IssuesUITest
	{
		public override string Issue => "Verify tap gestures work correctly with CarouselView Loop";

		public IssueCarouselViewNoLoopTapGesture(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void VerifyCarouselViewPositionButtonTapWithLoopEnabled()
		{
			// This test verifies that button tap works when CarouselView.Loop = true
			App.WaitForElement("PositionButton");

			// Verify initial state
			App.WaitForElement("ResultLabel");
			var initialText = App.FindElement("ResultLabel").GetText();
			Assert.That(initialText, Is.EqualTo("Button not clicked"));

			// Tap the button - this should work even with Loop = true
			App.Tap("PositionButton");

			// Verify the button click was successful
			var updatedText = App.FindElement("ResultLabel").GetText();
			Assert.That(updatedText, Is.EqualTo("Button clicked successfully!"));
		}

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void VerifyToggleLoopButtonWorksCorrectly()
		{
			// Test that toggling loop mode also works
			App.WaitForElement("ToggleLoopButton");

			// Verify initial loop state 
			var initialButtonText = App.FindElement("ToggleLoopButton").GetText();
			Assert.That(initialButtonText, Is.EqualTo("True"));

			// Toggle loop to false
			App.Tap("ToggleLoopButton");

			// Verify loop was toggled
			var resultLabel = App.FindElement("ResultLabel").GetText();
			Assert.That(resultLabel, Is.EqualTo("Loop toggled to: False"));

			// Now test that the position button still works with Loop = false
			App.Tap("PositionButton");
			var updatedResultLabel = App.FindElement("ResultLabel").GetText();
			Assert.That(updatedResultLabel, Is.EqualTo("Button clicked successfully!"));
		}

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void VerifyCarouselViewPositionChange()
		{
			// This test verifies that the carousel position changes when button is tapped
			App.WaitForElement("PositionButton");
			App.WaitForElement("carouselview");

			// Tap the button to change position
			App.Tap("PositionButton");

			// The carousel should now be at position 2 (showing "Percentage View")
			// Wait a moment for the position change to take effect
			Thread.Sleep(1000);

			VerifyScreenshot();
		}
	}
}