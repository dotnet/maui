using System.Diagnostics;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue6016 : _IssuesUITest
	{
		public override string Issue => "SwipeView Threshold changes width and offset of the side menu";

		public Issue6016(TestDevice device) : base(device) { }

		/// <summary>
		/// Waits for the X position of an element to stabilize (stop changing) within a timeout.
		/// Returns the final stable X value.
		/// </summary>
		static float WaitForXToSettle(IApp app, string automationId, Func<float, bool> settledCondition, TimeSpan? timeout = null)
		{
			var deadline = Stopwatch.StartNew();
			var maxWait = timeout ?? TimeSpan.FromSeconds(3);
			float currentX = app.WaitForElement(automationId).GetRect().X;

			while (deadline.Elapsed < maxWait)
			{
				currentX = app.WaitForElement(automationId).GetRect().X;
				if (settledCondition(currentX))
					return currentX;
				Task.Delay(100).Wait();
			}

			return currentX;
		}

		[Test]
		[Category(UITestCategories.SwipeView)]
		public void SwipeViewThresholdShouldNotChangeMenuWidth()
		{
			// Measure menu width by how far the content is displaced when the SwipeView snaps open.
			// SwipeItem AutomationId is not accessible via Appium on iOS, so we use content displacement.
			// Bug: Threshold was used as SwipeItem width, so content with Threshold=200 was displaced 200pts
			// instead of the default 100pts (SwipeItemWidth).

			// Record initial X of DefaultContent, then swipe right to open (LeftItems).
			// Use element-width-relative drag distance (67%) to be density-independent.
			var defaultContentRect = App.WaitForElement("DefaultContent").GetRect();
			float initialDefaultX = defaultContentRect.X;
			float dragDistance = defaultContentRect.Width * 0.67f;
			App.DragCoordinates(
				defaultContentRect.X + 10, defaultContentRect.Y + defaultContentRect.Height / 2,
				defaultContentRect.X + dragDistance, defaultContentRect.Y + defaultContentRect.Height / 2);

			// Wait until snap animation settles (content moves right)
			float openDefaultX = WaitForXToSettle(App, "DefaultContent", x => x > initialDefaultX + 5);
			float defaultMenuWidth = openDefaultX - initialDefaultX;

			// Close the default SwipeView by tapping inside the opened content
			App.TapCoordinates(openDefaultX + 50, defaultContentRect.Y + defaultContentRect.Height / 2);

			// Wait for close animation to settle before opening the next SwipeView
			WaitForXToSettle(App, "DefaultContent", x => x <= initialDefaultX + 5);

			// Record initial X of ThresholdContent, then swipe right.
			// Use the same element-width-relative drag (67%) — density-independent and sufficient
			// to exceed the min(Threshold=200, menuWidth=100) = 100dp open trigger.
			var thresholdContentRect = App.WaitForElement("ThresholdContent").GetRect();
			float initialThresholdX = thresholdContentRect.X;
			float thresholdDragDistance = thresholdContentRect.Width * 0.67f;
			App.DragCoordinates(
				thresholdContentRect.X + 10, thresholdContentRect.Y + thresholdContentRect.Height / 2,
				thresholdContentRect.X + thresholdDragDistance, thresholdContentRect.Y + thresholdContentRect.Height / 2);

			// Wait until snap animation settles
			float openThresholdX = WaitForXToSettle(App, "ThresholdContent", x => x > initialThresholdX + 5);
			float thresholdMenuWidth = openThresholdX - initialThresholdX;

			// The menu width (content displacement) must be the same regardless of Threshold
			Assert.That(thresholdMenuWidth, Is.EqualTo(defaultMenuWidth).Within(5),
				$"SwipeView menu width should not change with Threshold. Default width: {defaultMenuWidth}, Threshold=200 width: {thresholdMenuWidth}");
		}

		[Test]
		[Category(UITestCategories.SwipeView)]
		public void SwipeViewThresholdShouldNotChangeRightMenuWidth()
		{
			// Verify that Threshold does not affect the width of RightItems menus.
			// When RightItems open, content shifts LEFT; menu width = initialX - openX.
			// Bug: Threshold was used as SwipeItem width, so Threshold=200 displaced
			// content by 200pts instead of the default 100pts (SwipeItemWidth).

			// Scroll to ensure DefaultRightContent is visible before dragging
			App.ScrollTo("DefaultRightContent");

			// Record initial X of DefaultRightContent, then swipe left to open (RightItems).
			// Use the SwipeView element rect for drag coordinates (gives full width including padding).
			var defaultRightSwipeRect = App.WaitForElement("DefaultRightSwipeView").GetRect();
			var defaultRightContentRect = App.WaitForElement("DefaultRightContent").GetRect();
			float initialDefaultRightX = defaultRightContentRect.X;
			TestContext.WriteLine($"[RightMenu] DefaultRightContent initial rect: X={defaultRightContentRect.X}, Y={defaultRightContentRect.Y}, W={defaultRightContentRect.Width}, H={defaultRightContentRect.Height}");
			TestContext.WriteLine($"[RightMenu] DefaultRightSwipeView rect: X={defaultRightSwipeRect.X}, Y={defaultRightSwipeRect.Y}, W={defaultRightSwipeRect.Width}, H={defaultRightSwipeRect.Height}");

			// Swipe left using the SwipeView element for density-independent coordinates
			float swipeStartX = defaultRightSwipeRect.X + defaultRightSwipeRect.Width - 10;
			float swipeEndX = defaultRightSwipeRect.X + defaultRightSwipeRect.Width * 0.33f;
			float swipeY = defaultRightSwipeRect.Y + defaultRightSwipeRect.Height / 2;
			TestContext.WriteLine($"[RightMenu] Default drag: ({swipeStartX}, {swipeY}) -> ({swipeEndX}, {swipeY}), distance={swipeStartX - swipeEndX}");
			App.DragCoordinates(swipeStartX, swipeY, swipeEndX, swipeY);

			// Wait until snap animation settles (content moves left)
			float openDefaultRightX = WaitForXToSettle(App, "DefaultRightContent", x => x < initialDefaultRightX - 5);
			float defaultRightMenuWidth = initialDefaultRightX - openDefaultRightX;
			TestContext.WriteLine($"[RightMenu] Default: initialX={initialDefaultRightX}, openX={openDefaultRightX}, menuWidth={defaultRightMenuWidth}");

			// Close the default right SwipeView by tapping inside the opened content
			App.TapCoordinates(openDefaultRightX + 50, defaultRightContentRect.Y + defaultRightContentRect.Height / 2);

			// Wait for close animation to settle before opening the next SwipeView
			WaitForXToSettle(App, "DefaultRightContent", x => x >= initialDefaultRightX - 5);

			// Scroll to ThresholdRightContent, then swipe left.
			App.ScrollTo("ThresholdRightContent");
			var thresholdRightSwipeRect = App.WaitForElement("ThresholdRightSwipeView").GetRect();
			var thresholdRightContentRect = App.WaitForElement("ThresholdRightContent").GetRect();
			float initialThresholdRightX = thresholdRightContentRect.X;
			TestContext.WriteLine($"[RightMenu] ThresholdRightContent initial rect: X={thresholdRightContentRect.X}, Y={thresholdRightContentRect.Y}, W={thresholdRightContentRect.Width}, H={thresholdRightContentRect.Height}");
			TestContext.WriteLine($"[RightMenu] ThresholdRightSwipeView rect: X={thresholdRightSwipeRect.X}, Y={thresholdRightSwipeRect.Y}, W={thresholdRightSwipeRect.Width}, H={thresholdRightSwipeRect.Height}");

			// Swipe left using the SwipeView element for density-independent coordinates
			swipeStartX = thresholdRightSwipeRect.X + thresholdRightSwipeRect.Width - 10;
			swipeEndX = thresholdRightSwipeRect.X + thresholdRightSwipeRect.Width * 0.33f;
			swipeY = thresholdRightSwipeRect.Y + thresholdRightSwipeRect.Height / 2;
			TestContext.WriteLine($"[RightMenu] Threshold drag: ({swipeStartX}, {swipeY}) -> ({swipeEndX}, {swipeY}), distance={swipeStartX - swipeEndX}");
			App.DragCoordinates(swipeStartX, swipeY, swipeEndX, swipeY);

			// Wait until snap animation settles
			float openThresholdRightX = WaitForXToSettle(App, "ThresholdRightContent", x => x < initialThresholdRightX - 5);
			float thresholdRightMenuWidth = initialThresholdRightX - openThresholdRightX;
			TestContext.WriteLine($"[RightMenu] Threshold: initialX={initialThresholdRightX}, openX={openThresholdRightX}, menuWidth={thresholdRightMenuWidth}");

			// The menu width (content displacement) must be the same regardless of Threshold
			Assert.That(thresholdRightMenuWidth, Is.EqualTo(defaultRightMenuWidth).Within(5),
				$"SwipeView right menu width should not change with Threshold. Default width: {defaultRightMenuWidth}, Threshold=200 width: {thresholdRightMenuWidth}");
		}

		[Test]
		[Category(UITestCategories.SwipeView)]
		public void SwipeViewExecuteModeTriggers()
		{
			// Verify that SwipeMode.Execute triggers the SwipeItem when the swipe exceeds
			// the open distance (~80% of content width). This guards against regressions where
			// GetSwipeItemSize returns the wrong size for Execute mode (e.g., 100dp instead of
			// contentWidth / items.Count), which would cause a visible gap during the swipe.

			App.ScrollTo("ExecuteContent");

			var executeSwipeRect = App.WaitForElement("ExecuteSwipeView").GetRect();
			var executeContentRect = App.WaitForElement("ExecuteContent").GetRect();
			TestContext.WriteLine($"[Execute] ExecuteContent rect: X={executeContentRect.X}, Y={executeContentRect.Y}, W={executeContentRect.Width}, H={executeContentRect.Height}");
			TestContext.WriteLine($"[Execute] ExecuteSwipeView rect: X={executeSwipeRect.X}, Y={executeSwipeRect.Y}, W={executeSwipeRect.Width}, H={executeSwipeRect.Height}");

			// Drag right by 90% of the SwipeView width — exceeds the ~80% Execute trigger threshold.
			// Using SwipeView element rect for density-independent distance.
			float dragStartX = executeSwipeRect.X + 10;
			float dragEndX = executeSwipeRect.X + executeSwipeRect.Width * 0.9f;
			float dragY = executeSwipeRect.Y + executeSwipeRect.Height / 2;
			TestContext.WriteLine($"[Execute] Drag: ({dragStartX}, {dragY}) -> ({dragEndX}, {dragY}), distance={dragEndX - dragStartX}");
			App.DragCoordinates(dragStartX, dragY, dragEndX, dragY);

			// After execution the SwipeView snaps closed; wait for the result label to update
			App.WaitForElement("ExecuteResultLabel");
			string labelText = "Not Executed";
			var deadline = Stopwatch.StartNew();
			while (deadline.Elapsed < TimeSpan.FromSeconds(5))
			{
				labelText = App.WaitForElement("ExecuteResultLabel").GetText() ?? string.Empty;
				TestContext.WriteLine($"[Execute] Label text at {deadline.ElapsedMilliseconds}ms: '{labelText}'");
				if (labelText == "Executed")
					break;
				Task.Delay(200).Wait();
			}

			Assert.That(labelText, Is.EqualTo("Executed"),
				"SwipeItem.Invoked should have fired after swiping past the Execute mode threshold");
		}
	}
}
