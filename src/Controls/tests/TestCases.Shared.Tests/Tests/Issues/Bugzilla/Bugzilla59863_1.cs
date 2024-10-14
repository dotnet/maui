﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Gestures)]
public class Bugzilla59863_1 : _IssuesUITest
{
	public Bugzilla59863_1(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TapGestureRecognizer extremely finicky1";

	// [Test]
	// [FailsOnIOS]
	// public void SingleTapWithOnlyDoubleTapRecognizerShouldRegisterNothing()
	// {
	// 	RunningApp.WaitForElement(DoubleTapBoxId);
	// 	RunningApp.Tap(DoubleTapBoxId);

	// 	RunningApp.WaitForElement($"0 {Doubles} on {DoubleTapBoxId}");
	// }

	// [Test]
	// [FailsOnIOS]
	// public void DoubleTapWithOnlyDoubleTapRecognizerShouldRegisterOneDoubleTap()
	// {
	// 	RunningApp.WaitForElement(DoubleTapBoxId);
	// 	RunningApp.DoubleTap(DoubleTapBoxId);

	// 	RunningApp.WaitForElement($"1 {Doubles} on {DoubleTapBoxId}");
	// }
}