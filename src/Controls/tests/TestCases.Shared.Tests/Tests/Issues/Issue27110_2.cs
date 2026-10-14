using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue27110_2 : _IssuesUITest
{
	public Issue27110_2(TestDevice device) : base(device)
	{
	}

	public override string Issue => "ScrollTo a grouped CollectionView item by object";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void RepeatedNonAnimatedGroupedObjectScrollRequestsContinueToWork()
	{
		App.WaitForElement("ScrollToProboscisMonkeyButton");

		// Initial MakeVisible request establishes the starting item position.
		App.Tap("ScrollToProboscisMonkeyButton");
		var previousY = App.WaitForElement("Proboscis Monkey").GetRect().Y;

		// Select Start and verify the grouped item moves.
		App.Tap("SelectStartButton");
		App.Tap("ScrollToProboscisMonkeyButton");
		var currentY = previousY;
		App.RetryAssert(() =>
		{
			currentY = App.WaitForElement("Proboscis Monkey").GetRect().Y;
			Assert.That(
				currentY,
				Is.Not.EqualTo(previousY),
				"The Proboscis Monkey item's Y coordinate did not change after the grouped Start request.");
		});
		previousY = currentY;

		// Select Center and verify the grouped item moves again.
		App.Tap("SelectCenterButton");
		App.Tap("ScrollToProboscisMonkeyButton");
		App.RetryAssert(() =>
		{
			currentY = App.WaitForElement("Proboscis Monkey").GetRect().Y;
			Assert.That(
				currentY,
				Is.Not.EqualTo(previousY),
				"The Proboscis Monkey item's Y coordinate did not change after the grouped Center request.");
		});
		previousY = currentY;

		// Select End and verify the grouped item moves again.
		App.Tap("SelectEndButton");
		App.Tap("ScrollToProboscisMonkeyButton");
		App.RetryAssert(() =>
		{
			currentY = App.WaitForElement("Proboscis Monkey").GetRect().Y;
			Assert.That(
				currentY,
				Is.Not.EqualTo(previousY),
				"The Proboscis Monkey item's Y coordinate did not change after the grouped End request.");
		});
	}
}
