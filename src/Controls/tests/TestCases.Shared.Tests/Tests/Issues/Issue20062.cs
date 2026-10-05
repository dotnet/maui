#if TEST_FAILS_ON_WINDOWS // When selecting a single item, the selection background is applied to all items.
#if MACCATALYST
using System.Drawing;
using ImageMagick;
#endif
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20062 : _IssuesUITest
{
	public Issue20062(TestDevice device) : base(device) { }

	public override string Issue => "CollectionView - SelectedItem visual state manager not working";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 6)]
	public void CollectionViewSelectionChangesVisualState()
	{
		App.WaitForElement("CollectionView");
		App.WaitForElement("Item0");
		App.Tap("Item0");
#if MACCATALYST
		VerifySelection(0);
#endif
		App.WaitForElement("Item2");
		App.Tap("Item2");
#if MACCATALYST
		VerifySelection(2);
#else
		VerifyScreenshot();
#endif
	}

#if MACCATALYST
	void VerifySelection(int selectedIndex)
	{
		App.RetryAssert(() =>
		{
			var selectedCells = App.FindElements(AppiumQuery.ByXPath("//XCUIElementTypeCell[@selected='true']"));
			Assert.That(selectedCells.Count, Is.EqualTo(1));
			App.WaitForElement(AppiumQuery.ByXPath(
				$"//XCUIElementTypeCell[@selected='true'][.//*[@identifier='Item{selectedIndex}']]"));

			var window = App.WaitForElement(AppiumQuery.ByXPath("//XCUIElementTypeWindow")).GetRect();
			var cells = Enumerable.Range(0, 4).Select(index =>
			{
				var cell = App.WaitForElement(AppiumQuery.ByXPath(
					$"//XCUIElementTypeCell[.//*[@identifier='Item{index}']]")).GetRect();
				cell.Offset(-window.X, -window.Y);
				return cell;
			}).ToArray();
			using var screenshot = new MagickImage(TakeScreenshot());
			Assert.That(screenshot.Width, Is.EqualTo(window.Width), "Expected a window-local screenshot.");
			Assert.That(screenshot.Height, Is.EqualTo(window.Height), "Expected a window-local screenshot.");
			AssertSelectionColors(screenshot, cells, selectedIndex);
		});
	}

	static void AssertSelectionColors(MagickImage screenshot, Rectangle[] cells, int selectedIndex)
	{
		using var pixels = screenshot.GetPixels();
		for (var index = 0; index < cells.Length; index++)
		{
			var cell = cells[index];
			Assert.That(cell.Width, Is.GreaterThan(8));
			Assert.That(cell.Height, Is.GreaterThan(8));
			Assert.That(cell.Left, Is.GreaterThan(0));
			Assert.That(cell.Top, Is.GreaterThanOrEqualTo(0));
			Assert.That(cell.Right, Is.LessThanOrEqualTo(screenshot.Width));
			Assert.That(cell.Bottom, Is.LessThanOrEqualTo(screenshot.Height));

			// Inspect the rendered fill and straight stroke, independently of text and rounded-corner rasterization.
			var fill = pixels.GetPixel(cell.Left + cell.Width / 2, cell.Top + cell.Height / 2).ToColor()!;
			if (index == selectedIndex)
			{
				Assert.That(fill.G, Is.Zero);
				Assert.That(fill.B, Is.EqualTo(MagickColors.Blue.B));
				Assert.That(fill.R, Is.LessThan(fill.B), $"Item{index} must paint its selected blue background.");
			}
			else
			{
				Assert.That(fill, Is.EqualTo(MagickColors.White), $"Item{index} must paint its normal white background.");
			}

			foreach (var y in new[] { cell.Top + cell.Height / 3, cell.Top + cell.Height / 2, cell.Top + cell.Height * 2 / 3 })
			{
				var hasRedStroke = Enumerable.Range(cell.Left - 1, 4).Any(x =>
				{
					var color = pixels.GetPixel(x, y).ToColor()!;
					return color.R > color.G && color.R > color.B;
				});
				Assert.That(hasRedStroke, Is.EqualTo(index == selectedIndex),
					$"Only the selected item may paint a red stroke (Item{index}, y={y}).");
			}
		}
	}
#endif
}
#endif