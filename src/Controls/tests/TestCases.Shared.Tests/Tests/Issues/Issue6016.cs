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
	}
}
