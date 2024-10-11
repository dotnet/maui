﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Bugzilla53179_1 : _IssuesUITest
{
	public Bugzilla53179_1(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "1PopAsync crashing after RemovePage when support packages are updated to 25.1.1";

	// [Test]
	// [Category(UITestCategories.Navigation)]
	// public void PopAsyncAfterRemovePageDoesNotCrash()
	// {
	// 	RunningApp.WaitForElement(StartTest);
	// 	RunningApp.Tap(StartTest);
	// 	RunningApp.WaitForElement(RootLabel);
	// }
}