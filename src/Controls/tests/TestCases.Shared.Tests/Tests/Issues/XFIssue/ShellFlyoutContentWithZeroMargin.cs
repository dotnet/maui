using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Shell)]
public class ShellFlyoutContentWithZeroMargin : _IssuesUITest
{
	public ShellFlyoutContentWithZeroMargin(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Flyout Content With Zero Margin offsets correctly";

	//[Test]
	//public void FlyoutHeaderBehaviorFixed()
	//{
	//	RunningApp.Tap(nameof(FlyoutHeaderBehavior.Fixed));
	//	this.ShowFlyout();
	//	float startingHeight = GetFlyoutHeight();
	//	RunningApp.ScrollDown("Item 4", ScrollStrategy.Gesture);
	//	float endHeight = GetFlyoutHeight();

	//	Assert.AreEqual(startingHeight, endHeight);
	//}

	//[FailsOnAndroid]
	//[FailsOnIOS]
	//[Test]
	//public void FlyoutHeaderBehaviorCollapseOnScroll()
	//{
	//	RunningApp.Tap(nameof(FlyoutHeaderBehavior.CollapseOnScroll));
	//	this.ShowFlyout();
	//	float startingHeight = GetFlyoutHeight();
	//	RunningApp.ScrollDown("Item 4", ScrollStrategy.Gesture);
	//	float endHeight = GetFlyoutHeight();

	//	Assert.Greater(startingHeight, endHeight);
	//}

	//[Test]
	//[FailsOnIOS]
	//public void FlyoutHeaderBehaviorScroll()
	//{
	//	RunningApp.Tap(nameof(FlyoutHeaderBehavior.Scroll));
	//	this.ShowFlyout();

	//	var startingY = GetFlyoutY();
	//	RunningApp.ScrollDown("Item 5", ScrollStrategy.Gesture);
	//	var nextY = GetFlyoutY();

	//	while (nextY != null)
	//	{
	//		Assert.Greater(startingY.Value, nextY.Value);
	//		startingY = nextY;
	//		RunningApp.ScrollDown("Item 5", ScrollStrategy.Gesture);
	//		nextY = GetFlyoutY();
	//	}
	//}

	//float GetFlyoutHeight() =>
	//	RunningApp.WaitForElement("FlyoutHeaderId")[0].Rect.Height;

	//float? GetFlyoutY()
	//{
	//	var flyoutHeader =
	//		RunningApp.Query("FlyoutHeaderId");

	//	if (flyoutHeader.Length == 0)
	//		return null;

	//	return flyoutHeader[0].Rect.Y;
	//}
}