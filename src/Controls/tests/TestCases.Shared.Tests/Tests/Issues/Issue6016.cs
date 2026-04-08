using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue6016 : _IssuesUITest
	{
		public override string Issue => "SwipeView Threshold changes width and offset of the side menu";

		public Issue6016(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.SwipeView)]
		public void SwipeViewThresholdShouldNotChangeMenuWidth()
		{
			// Measure menu width by how far the content is displaced when the SwipeView snaps open.
			// SwipeItem AutomationId is not accessible via Appium on iOS, so we use content displacement.
			// Bug: Threshold was used as SwipeItem width, so content with Threshold=200 was displaced 200pts
			// instead of the default 100pts (SwipeItemWidth).

			// Record initial X of DefaultContent, then swipe right to open (LeftItems)
			var defaultContentRect = App.WaitForElement("DefaultContent").GetRect();
			float initialDefaultX = defaultContentRect.X;
			App.DragCoordinates(
				defaultContentRect.X + 10, defaultContentRect.Y + defaultContentRect.Height / 2,
				defaultContentRect.X + 200, defaultContentRect.Y + defaultContentRect.Height / 2);

			// Poll until snap animation settles (content moves right)
			float openDefaultX = initialDefaultX;
			for (int i = 0; i < 15; i++)
			{
				openDefaultX = App.WaitForElement("DefaultContent").GetRect().X;
				if (openDefaultX > initialDefaultX + 5)
					break;
				System.Threading.Thread.Sleep(200);
			}
			float defaultMenuWidth = openDefaultX - initialDefaultX;

			// Close the default SwipeView by tapping inside the opened content
			App.TapCoordinates(openDefaultX + 50, defaultContentRect.Y + defaultContentRect.Height / 2);

			// Wait for close animation to settle before opening the next SwipeView
			for (int i = 0; i < 10; i++)
			{
				if (App.WaitForElement("DefaultContent").GetRect().X <= initialDefaultX + 5)
					break;
				System.Threading.Thread.Sleep(200);
			}

			// Record initial X of ThresholdContent, then swipe right.
			// Use a 350px drag (vs 200px for default) — Android Appium coordinates are in physical pixels,
			// so on a 2.5x density emulator 100dp ≈ 250px. A 340px drag ≈ 132dp safely exceeds
			// the min(Threshold=200, menuWidth=100) = 100dp trigger on any standard emulator.
			var thresholdContentRect = App.WaitForElement("ThresholdContent").GetRect();
			float initialThresholdX = thresholdContentRect.X;
			App.DragCoordinates(
				thresholdContentRect.X + 10, thresholdContentRect.Y + thresholdContentRect.Height / 2,
				thresholdContentRect.X + 350, thresholdContentRect.Y + thresholdContentRect.Height / 2);

			// Poll until snap animation settles
			float openThresholdX = initialThresholdX;
			for (int i = 0; i < 15; i++)
			{
				openThresholdX = App.WaitForElement("ThresholdContent").GetRect().X;
				if (openThresholdX > initialThresholdX + 5)
					break;
				System.Threading.Thread.Sleep(200);
			}
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

			// Record initial X of DefaultRightContent, then swipe left to open (RightItems)
			var defaultRightContentRect = App.WaitForElement("DefaultRightContent").GetRect();
			float initialDefaultRightX = defaultRightContentRect.X;
			App.DragCoordinates(
				defaultRightContentRect.X + defaultRightContentRect.Width - 10, defaultRightContentRect.Y + defaultRightContentRect.Height / 2,
				defaultRightContentRect.X + defaultRightContentRect.Width - 200, defaultRightContentRect.Y + defaultRightContentRect.Height / 2);

			// Poll until snap animation settles (content moves left)
			float openDefaultRightX = initialDefaultRightX;
			for (int i = 0; i < 15; i++)
			{
				openDefaultRightX = App.WaitForElement("DefaultRightContent").GetRect().X;
				if (openDefaultRightX < initialDefaultRightX - 5)
					break;
				System.Threading.Thread.Sleep(200);
			}
			float defaultRightMenuWidth = initialDefaultRightX - openDefaultRightX;

			// Close the default right SwipeView by tapping inside the opened content
			App.TapCoordinates(openDefaultRightX + 50, defaultRightContentRect.Y + defaultRightContentRect.Height / 2);

			// Wait for close animation to settle before opening the next SwipeView
			for (int i = 0; i < 10; i++)
			{
				if (App.WaitForElement("DefaultRightContent").GetRect().X >= initialDefaultRightX - 5)
					break;
				System.Threading.Thread.Sleep(200);
			}

			// Scroll to ThresholdRightContent, then swipe left.
			// Use a 350px drag (vs 200px for default) — same reasoning as left-items test:
			// exceeds min(Threshold=200, menuWidth=100) = 100dp trigger on any standard emulator.
			App.ScrollTo("ThresholdRightContent");
			var thresholdRightContentRect = App.WaitForElement("ThresholdRightContent").GetRect();
			float initialThresholdRightX = thresholdRightContentRect.X;
			App.DragCoordinates(
				thresholdRightContentRect.X + thresholdRightContentRect.Width - 10, thresholdRightContentRect.Y + thresholdRightContentRect.Height / 2,
				thresholdRightContentRect.X + thresholdRightContentRect.Width - 350, thresholdRightContentRect.Y + thresholdRightContentRect.Height / 2);

			// Poll until snap animation settles
			float openThresholdRightX = initialThresholdRightX;
			for (int i = 0; i < 15; i++)
			{
				openThresholdRightX = App.WaitForElement("ThresholdRightContent").GetRect().X;
				if (openThresholdRightX < initialThresholdRightX - 5)
					break;
				System.Threading.Thread.Sleep(200);
			}
			float thresholdRightMenuWidth = initialThresholdRightX - openThresholdRightX;

			// The menu width (content displacement) must be the same regardless of Threshold
			Assert.That(thresholdRightMenuWidth, Is.EqualTo(defaultRightMenuWidth).Within(5),
				$"SwipeView right menu width should not change with Threshold. Default width: {defaultRightMenuWidth}, Threshold=200 width: {thresholdRightMenuWidth}");
		}
	}
}
