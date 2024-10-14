﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1763 : _IssuesUITest
{
	public Issue1763(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "First item of grouped ListView not firing .ItemTapped";

	// [Test]
	// [Category(UITestCategories.ListView)]
	// [FailsOnIOS]
	// [FailsOnAndroid]
	// public void TestIssue1763ItemTappedFiring()
	// {
	// 	RunningApp.WaitForElement(q => q.Marked("Contacts"));
	// 	RunningApp.Tap(q => q.Marked("Egor1"));
	// 	RunningApp.WaitForElement(q => q.Marked("Tapped a List item"));
	// 	RunningApp.Tap(q => q.Marked("Destruction"));
	// 	RunningApp.WaitForElement(q => q.Marked("Contacts"));
	// }
}