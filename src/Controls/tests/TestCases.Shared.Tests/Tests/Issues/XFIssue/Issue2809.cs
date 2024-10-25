﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2809 : _IssuesUITest
{
	public Issue2809(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Secondary ToolbarItems cause app to hang during PushAsync";

	// [Test]
	// [Category(UITestCategories.DisplayAlert)]
	// [FailsOnIOS]
	// 	public void TestPageDoesntCrash()
	// 	{
	// 		ShouldShowMenu();
	// 		RunningApp.Tap(c => c.Marked("Item 1"));
	// 		RunningApp.Screenshot("Didn't crash");
	// 	}

	// 	void ShouldShowMenu()
	// 	{
	// #if ANDROID
	// 		//show secondary menu
	// 		RunningApp.TapOverflowMenuButton();
	// #elif WINDOWS
	// 		RunningApp.Tap ("MoreButton");
	// #endif
	// 	}
}