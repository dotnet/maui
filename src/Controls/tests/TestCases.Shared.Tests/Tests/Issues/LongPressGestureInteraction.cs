using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class LongPressGestureInteraction : _IssuesUITest
	{
		public LongPressGestureInteraction(TestDevice device) : base(device) { }

		public override string Issue => "LongPress Gesture Interaction Tests";

		// Helper method for long press - TouchAndHold not supported on Mac Catalyst
		void PerformLongPress(string elementId)
		{
#if MACCATALYST
			App.LongPress(elementId);
#else
			App.TouchAndHold(elementId);
#endif
		}

		[Test]
		[Category(UITestCategories.Gestures)]
		public void LongPressWithTap_BothFireIndependently()
		{
			// Wait for element to be ready
			App.WaitForElement("TapAndLongPressFrame");

			// Quick tap should only fire Tap
			// Use TapElement helper which uses TapCoordinates - see UtilExtensions for details
			App.TapElement("TapAndLongPressFrame");
			App.WaitForElement("TapLabel");

			var tapLabel = App.FindElement("TapLabel").GetText();
			var longPressLabel = App.FindElement("LongPressLabel").GetText();

			Assert.That(tapLabel, Is.EqualTo("Tap Count: 1"), "Tap should fire on quick tap");
			Assert.That(longPressLabel, Is.EqualTo("Long Press Count: 0"), "LongPress should NOT fire on quick tap");

			// Long press should fire LongPress
			PerformLongPress("TapAndLongPressFrame");
			App.WaitForElement("LongPressLabel");

			longPressLabel = App.FindElement("LongPressLabel").GetText();
			Assert.That(longPressLabel, Is.EqualTo("Long Press Count: 1"), "LongPress should fire after holding");
		}

		[Test]
		[Category(UITestCategories.Gestures)]
		public void LongPressWithSwipe_SwipeCancelsLongPress()
		{
			// Swipe + LongPress on the same element is not reliably supported on Windows
			// (Manipulation system conflicts with Pointer events used by LongPressGestureHandler)
			// and macOS Catalyst (discrete UISwipeGestureRecognizer conflicts with continuous
			// UILongPressGestureRecognizer when simulated via Appium).
#if WINDOWS || MACCATALYST
			Assert.Ignore("Swipe + LongPress gesture combination on the same element is not supported on this platform.");
#endif

			App.WaitForElement("SwipeAndLongPressFrame");

			// Swipe left (right to left) - should fire swipe but NOT long press
			App.SwipeRightToLeft("SwipeAndLongPressFrame");
			App.WaitForElement("SwipeLabel");

			var swipeLabel = App.FindElement("SwipeLabel").GetText();
			var longPressLabel = App.FindElement("LongPress2Label").GetText();

			Assert.That(swipeLabel, Is.EqualTo("Swipe Count: 1"), "Swipe should fire");
			Assert.That(longPressLabel, Is.EqualTo("Long Press Count: 0"), "LongPress should be cancelled by swipe movement");
		}

		[Test]
		[Category(UITestCategories.Gestures)]
		public void LongPressInScrollView_FiresWhenStill()
		{
			App.WaitForElement("LongPressInScrollFrame");

			// Hold still - should fire LongPress
			PerformLongPress("LongPressInScrollFrame");
			App.WaitForElement("LongPress3Label");

			var label = App.FindElement("LongPress3Label").GetText();
			Assert.That(label, Is.EqualTo("Long Press Count: 1"), "LongPress should fire when holding still in ScrollView");
		}

		[Test]
		[Category(UITestCategories.Gestures)]
		public void MultipleLongPress_AllWorkIndependently()
		{
			App.WaitForElement("LongPress1");
			App.WaitForElement("LongPress2");

			// Long press first frame
			PerformLongPress("LongPress1");
			App.WaitForElement("LongPress4Label");

			var label1 = App.FindElement("LongPress4Label").GetText();
			var label2 = App.FindElement("LongPress5Label").GetText();

			Assert.That(label1, Is.EqualTo("LongPress1 Count: 1"), "First LongPress should fire");
			Assert.That(label2, Is.EqualTo("LongPress2 Count: 0"), "Second LongPress should NOT fire");

			// Long press second frame
			PerformLongPress("LongPress2");
			App.WaitForElement("LongPress5Label");

			label1 = App.FindElement("LongPress4Label").GetText();
			label2 = App.FindElement("LongPress5Label").GetText();

			Assert.That(label1, Is.EqualTo("LongPress1 Count: 1"), "First LongPress count should remain 1");
			Assert.That(label2, Is.EqualTo("LongPress2 Count: 1"), "Second LongPress should fire");
		}
	}
}
