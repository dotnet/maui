﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla57749 : _IssuesUITest
{
	public Bugzilla57749(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "After enabling a disabled button it is not clickable";

	// [Test]
	// [FailsOnIOS]
	// public async Task Bugzilla57749Test()
	// {
	// 	await Task.Delay(500);
	// 	RunningApp.Tap(c => c.Marked("btnClick"));
	// 	RunningApp.WaitForElement (q => q.Marked ("Button was clicked"));
	// 	RunningApp.Tap("Ok");
	// }
}