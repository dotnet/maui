﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class ShellFlyoutContentOffset : _IssuesUITest
{
	public ShellFlyoutContentOffset(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Flyout Content Offsets Correctly";

	//[Test]
	//[Category(UITestCategories.Shell)]
	//[FailsOnAndroid]
	//[FailsOnIOSWhenRunningOnXamarinUITest]
	//public void FlyoutContentOffsetsCorrectly()
	//{
	//	RunningApp.WaitForElement("PageLoaded");
	//	var flyoutLoc = GetLocationAndRotateToNextContent("Item 1");
	//	var labelLoc = GetLocationAndRotateToNextContent("LabelContent");
	//	var scrollViewLoc = GetLocationAndRotateToNextContent("ScrollViewContent");

	//	Assert.AreEqual(flyoutLoc, labelLoc, "Label Offset Incorrect");
	//	Assert.AreEqual(flyoutLoc, scrollViewLoc, "ScrollView Offset Incorrect");
	//}

	//[Test]
	//[Category(UITestCategories.Shell)]
	//public void FlyoutContentOffsetsCorrectlyWithHeader()
	//{
	//	RunningApp.Tap("ToggleHeader");
	//	GetLocationAndRotateToNextContent("Item 1");
	//	var labelLoc = GetLocationAndRotateToNextContent("LabelContent");
	//	var scrollViewLoc = GetLocationAndRotateToNextContent("ScrollViewContent");

	//	Assert.AreEqual(labelLoc, scrollViewLoc, "ScrollView Offset Incorrect");
	//}

	//float GetLocationAndRotateToNextContent(string automationId)
	//{
	//	ShowFlyout();
	//	var y = RunningApp.WaitForElement(automationId)[0].Rect.Y;
	//	RunningApp.Tap("CloseFlyout");
	//	RunningApp.Tap("ToggleFlyoutContent");

	//	return y;
	//}
}