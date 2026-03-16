#if ANDROID || IOS || MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34257 : _IssuesUITest
{
	public Issue34257(TestDevice device)
		: base(device)
	{
	}

	public override string Issue => "CollectionView vertical grid item spacing updates all rows and columns";

	void WaitForText(string elementId, string expectedText, int timeoutSec = 5)
	{
		var endTime = DateTime.Now.AddSeconds(timeoutSec);
		while (DateTime.Now < endTime)
		{
			var text = App.WaitForElement(elementId).GetText();
			if (text == expectedText)
				return;

			Thread.Sleep(100);
		}

		var finalText = App.WaitForElement(elementId).GetText();
		Assert.That(finalText, Is.EqualTo(expectedText), $"Timed out waiting for {elementId} text to be '{expectedText}'");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void UpdatingHorizontalSpacingShouldResizeBothColumns()
	{
		var firstColumnBefore = App.WaitForElement("FirstColumnTopItem").GetRect();
		var secondColumnBefore = App.WaitForElement("SecondColumnTopItem").GetRect();
		var gapBefore = secondColumnBefore.X - (firstColumnBefore.X + firstColumnBefore.Width);

		Assert.That(Math.Abs(firstColumnBefore.Width - secondColumnBefore.Width), Is.LessThanOrEqualTo(5),
			$"Expected the initial column widths to match, but first was {firstColumnBefore.Width} and second was {secondColumnBefore.Width}.");

		App.Tap("ApplyHorizontalSpacingButton");
		WaitForText("StatusLabel", "Spacing=0,80");

		var firstColumnAfter = App.WaitForElement("FirstColumnTopItem").GetRect();
		var secondColumnAfter = App.WaitForElement("SecondColumnTopItem").GetRect();
		var gapAfter = secondColumnAfter.X - (firstColumnAfter.X + firstColumnAfter.Width);

		Assert.That(firstColumnAfter.Width, Is.LessThan(firstColumnBefore.Width - 10),
			$"Expected the first column width to shrink after applying horizontal spacing, but it changed from {firstColumnBefore.Width} to {firstColumnAfter.Width}.");
		Assert.That(secondColumnAfter.Width, Is.LessThan(secondColumnBefore.Width - 10),
			$"Expected the second column width to shrink after applying horizontal spacing, but it changed from {secondColumnBefore.Width} to {secondColumnAfter.Width}.");
		Assert.That(gapAfter, Is.GreaterThan(gapBefore + 20),
			$"Expected the gap between columns to increase, but it changed from {gapBefore} to {gapAfter}.");
		Assert.That(Math.Abs(firstColumnAfter.Width - secondColumnAfter.Width), Is.LessThanOrEqualTo(5),
			$"Expected the updated column widths to match, but first was {firstColumnAfter.Width} and second was {secondColumnAfter.Width}.");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void UpdatingVerticalSpacingShouldResizeBothRows()
	{
		var topRowBefore = App.WaitForElement("FirstColumnTopItem").GetRect();
		var bottomRowBefore = App.WaitForElement("FirstColumnBottomItem").GetRect();
		var gapBefore = bottomRowBefore.Y - (topRowBefore.Y + topRowBefore.Height);

		Assert.That(Math.Abs(topRowBefore.Height - bottomRowBefore.Height), Is.LessThanOrEqualTo(5),
			$"Expected the initial row heights to match, but top was {topRowBefore.Height} and bottom was {bottomRowBefore.Height}.");

		App.Tap("ApplyVerticalSpacingButton");
		WaitForText("StatusLabel", "Spacing=40,0");

		var topRowAfter = App.WaitForElement("FirstColumnTopItem").GetRect();
		var bottomRowAfter = App.WaitForElement("FirstColumnBottomItem").GetRect();
		var gapAfter = bottomRowAfter.Y - (topRowAfter.Y + topRowAfter.Height);
		var topRowMoved = Math.Abs(topRowAfter.Y - topRowBefore.Y) > 5;
		var topRowShrank = topRowAfter.Height < topRowBefore.Height - 10;

		Assert.That(topRowMoved || topRowShrank, Is.True,
			$"Expected the top row to participate in the vertical spacing update, but Y stayed {topRowBefore.Y}->{topRowAfter.Y} and height stayed {topRowBefore.Height}->{topRowAfter.Height}.");
		Assert.That(gapAfter, Is.GreaterThan(gapBefore + 20),
			$"Expected the gap between rows to increase, but it changed from {gapBefore} to {gapAfter}.");
		Assert.That(Math.Abs(topRowAfter.Height - bottomRowAfter.Height), Is.LessThanOrEqualTo(5),
			$"Expected the updated row heights to match, but top was {topRowAfter.Height} and bottom was {bottomRowAfter.Height}.");
	}
}
#endif
