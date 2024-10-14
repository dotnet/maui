﻿#if !WINDOWS
// This test won't work on Windows right now because we can only test desktop, so touch events
// (like LongPress) don't really work. The test should work manually on a touch screen, though.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2981 : _IssuesUITest
{
	public Issue2981(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Long Press on ListView causes crash";

	// [Test]
	// [Category(UITestCategories.ListView)]
	// [FailsOnIOS]
	// public void Issue2981Test()
	// {
	// 	RunningApp.Screenshot("I am at Issue 1");
	// 	RunningApp.TouchAndHold(q => q.Marked("Cell1"));
	// 	RunningApp.Screenshot("Long Press first cell");
	// 	RunningApp.TouchAndHold(q => q.Marked("Cell2"));
	// 	RunningApp.Screenshot("Long Press second cell");
	// }
}
#endif