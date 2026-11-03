using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue27110 : _IssuesUITest
{
	public Issue27110(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Subsequent non animated CollectionView scroll requests stop working on Windows";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void RepeatedNonAnimatedScrollRequestsContinueToWork()
	{
		App.WaitForElement("ScrollToProboscisMonkeyButton");

		App.Tap("ScrollToProboscisMonkeyButton");
		var previousY = GetTargetItemY();

		string[] positionButtons =
		[
			"SelectStartButton",
			"SelectCenterButton",
			"SelectEndButton"
		];

		foreach (var positionButton in positionButtons)
		{
			App.Tap(positionButton);
			App.Tap("ScrollToProboscisMonkeyButton");
			previousY = WaitForTargetItemYChange(previousY);
		}
	}

	double WaitForTargetItemYChange(double previousY)
	{
		var currentY = previousY;
		App.RetryAssert(() =>
		{
			currentY = GetTargetItemY();
			Assert.That(currentY, Is.Not.EqualTo(previousY), "The Proboscis Monkey item's on-screen Y coordinate did not change.");
		});
		return currentY;
	}

	double GetTargetItemY()
	{
		return App.WaitForElement("Proboscis Monkey").GetRect().Y;
	}
}
