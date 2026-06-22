#if TEST_FAILS_ON_WINDOWS // AutomationId for SwipeItem is not being set on Windows, causing test failures.
using System.Drawing;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue6016 : _IssuesUITest
	{
		public override string Issue => "SwipeView Threshold changes width and offset of the side menu";

		public Issue6016(TestDevice device) : base(device) { }

		// Waits up to `timeout` for the element's X position to satisfy `predicate`.
		// Returns the last observed X value.
		static float WaitForXPosition(IApp app, string automationId, Func<float, bool> predicate,
			TimeSpan? timeout = null)
		{
			timeout ??= TimeSpan.FromSeconds(3);
			var retryFrequency = TimeSpan.FromMilliseconds(200);
			var deadline = DateTime.Now + timeout.Value;
			float x = 0;
			do
			{
				x = app.WaitForElement(automationId).GetRect().X;
				if (predicate(x))
					break;
				Task.Delay(retryFrequency).Wait();
			}
			while (DateTime.Now < deadline);
			return x;
		}

		// Parameterized over swipe direction:
		//   swipeRight=true  → LeftItems  (drag right, content moves right when open)
		//   swipeRight=false → RightItems (drag left, content moves left when open)
		//
		// Drag distance is 50% of SwipeView width — density-independent; always exceeds
		// the ~100dp snap threshold regardless of screen density.
		[TestCase("DefaultContent", "ThresholdContent", "DefaultSwipeView", "ThresholdSwipeView", true, TestName = "LeftItems")]
		[TestCase("DefaultRightContent", "ThresholdRightContent", "DefaultRightSwipeView", "ThresholdRightSwipeView", false, TestName = "RightItems")]
		[Category(UITestCategories.SwipeView)]
		public void SwipeViewThresholdShouldNotChangeMenuWidth(
			string defaultContentId, string thresholdContentId,
			string defaultSwipeViewId, string thresholdSwipeViewId,
			bool swipeRight)
		{
			App.ScrollTo(defaultContentId);

			// Open the default (no-threshold) SwipeView and measure content displacement
			var defaultSwipeRect = App.WaitForElement(defaultSwipeViewId).GetRect();
			float initialDefaultX = App.WaitForElement(defaultContentId).GetRect().X;
			OpenSwipeView(defaultSwipeRect, swipeRight);
			float openDefaultX = WaitForXPosition(App, defaultContentId,
				x => swipeRight ? x > initialDefaultX + 5 : x < initialDefaultX - 5);
			float defaultMenuWidth = Math.Abs(openDefaultX - initialDefaultX);

			// Close the default SwipeView before opening the threshold one.
			// A scroll may be needed between them; leaving it open while scrolling
			// causes the subsequent drag to not register on the threshold SwipeView.
			App.TapCoordinates(
				swipeRight ? openDefaultX + 50 : openDefaultX - 50,
				defaultSwipeRect.CenterY());
			WaitForXPosition(App, defaultContentId,
				x => swipeRight ? x <= initialDefaultX + 5 : x >= initialDefaultX - 5);

			// Open the threshold (Threshold=200) SwipeView and measure content displacement
			App.ScrollTo(thresholdContentId);
			var thresholdSwipeRect = App.WaitForElement(thresholdSwipeViewId).GetRect();
			float initialThresholdX = App.WaitForElement(thresholdContentId).GetRect().X;
			OpenSwipeView(thresholdSwipeRect, swipeRight);
			float openThresholdX = WaitForXPosition(App, thresholdContentId,
				x => swipeRight ? x > initialThresholdX + 5 : x < initialThresholdX - 5);
			float thresholdMenuWidth = Math.Abs(openThresholdX - initialThresholdX);

			Assert.That(thresholdMenuWidth, Is.EqualTo(defaultMenuWidth).Within(5),
				$"SwipeView menu width should not change with Threshold. " +
				$"Default={defaultMenuWidth:F1}px, Threshold=200 → {thresholdMenuWidth:F1}px");
		}

		// Opens a SwipeView by dragging 50% of its width — density-independent gesture.
		void OpenSwipeView(Rectangle swipeViewRect, bool swipeRight)
		{
			float centerY = swipeViewRect.CenterY();
			float halfWidth = swipeViewRect.Width * 0.5f;
			if (swipeRight)
				App.DragCoordinates(swipeViewRect.X + 10, centerY, swipeViewRect.X + halfWidth, centerY);
			else
				App.DragCoordinates(swipeViewRect.X + swipeViewRect.Width - 10, centerY, swipeViewRect.X + halfWidth, centerY);
		}

		[Test]
		[Category(UITestCategories.SwipeView)]
		public void SwipeViewExecuteModeTriggers()
		{
			// Verify that SwipeMode.Execute triggers the SwipeItem when the swipe exceeds
			// the open distance (~80% of content width). Guards against regressions where
			// GetSwipeItemSize returns wrong size for Execute mode (e.g. 100dp instead of
			// contentWidth / items.Count), which would cause a visible gap during the swipe.

			App.ScrollTo("ExecuteContent");

			// Use SwipeView bounds for drag — the inner label is much narrower and
			// a label-relative drag would fall far short of the 48% trigger threshold.
			var executeRect = App.WaitForElement("ExecuteSwipeView").GetRect();

			// Drag right by 90% of SwipeView width — density-independent, exceeds ~80% trigger
			App.DragCoordinates(
				executeRect.X + 10, executeRect.CenterY(),
				executeRect.X + executeRect.Width * 0.9f, executeRect.CenterY());

			// After Execute mode triggers, SwipeView snaps closed; wait for result label
			bool executed = App.WaitForTextToBePresentInElement("ExecuteResultLabel", "Executed",
				timeout: TimeSpan.FromSeconds(3));

			Assert.That(executed, Is.True,
				"SwipeItem.Invoked should have fired after swiping past the Execute mode threshold");
		}
	}
}
#endif
