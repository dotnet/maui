using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue29245NoLoop : _IssuesUITest
	{
		public override string Issue => "Verify tap gestures work correctly with CarouselView no Loop";

		public Issue29245NoLoop(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void VerifyCarouselViewButtonTapWithLoopDisabled()
		{
			// This test verifies that button tap works correctly when CarouselView.Loop = false
			// This ensures our fix doesn't break existing functionality
			App.WaitForElement("NoLoopButton");

			// Verify initial state
			App.WaitForElement("NoLoopResultLabel");
			var initialText = App.FindElement("NoLoopResultLabel").GetText();
			Assert.That(initialText, Is.EqualTo("No Loop: Button not clicked"));
	
			// Tap the button - this should work with Loop = false (existing behavior)
			App.Tap("NoLoopButton");

			// Verify the button click was successful
			var updatedText = App.FindElement("NoLoopResultLabel").GetText();
			Assert.That(updatedText, Is.EqualTo("No Loop: Button clicked successfully!"));
		}

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void VerifyCarouselViewPositionChangeWithoutLoop()
		{
			// This test verifies that the carousel position changes when button is tapped (Loop = false)
			App.WaitForElement("NoLoopButton");
			App.WaitForElement("CarouselViewNoLoop");

			// Tap the button to change position
			App.Tap("NoLoopButton");

			// The carousel should now be at position 1 (showing "Second View")
			// Wait a moment for the position change to take effect
			Thread.Sleep(1000);

			VerifyScreenshot();
		}
	}
}