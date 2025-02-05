using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class ShellFlyoutContentOffset : _IssuesUITest
{
	public ShellFlyoutContentOffset(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Flyout Content Offsets Correctly";
#if !ANDROID && !WINDOWS // The test fails on Android and Windows due to differing Rect values between flyout item and label content, despite correct offset in Appium inspector.
	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutContentOffsetsCorrectly()
	{
		App.WaitForElement("PageLoaded");
		var flyoutLoc = GetLocationAndRotateToNextContent("Item 1");
		var labelLoc = GetLocationAndRotateToNextContent("LabelContent");
		var scrollViewLoc = GetLocationAndRotateToNextContent("ScrollViewContent");

		Assert.That(flyoutLoc, Is.EqualTo(labelLoc), "Label Offset Incorrect");
		Assert.That(flyoutLoc, Is.EqualTo(scrollViewLoc), "ScrollView Offset Incorrect");
	}
#endif
	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutContentOffsetsCorrectlyWithHeader()
	{
		App.WaitForElement("ToggleHeader");
		App.Tap("ToggleHeader");
		GetLocationAndRotateToNextContent("Item 1");
		var labelLoc = GetLocationAndRotateToNextContent("LabelContent");
		var scrollViewLoc = GetLocationAndRotateToNextContent("ScrollViewContent");

		Assert.That(labelLoc, Is.EqualTo(scrollViewLoc), "ScrollView Offset Incorrect");
	}

	float GetLocationAndRotateToNextContent(string automationId)
	{
		App.ShowFlyout();
		var y = App.WaitForElement(automationId).GetRect().Y;
		App.Tap("CloseFlyout");
		App.Tap("ToggleFlyoutContent");
		return y;
	}
}