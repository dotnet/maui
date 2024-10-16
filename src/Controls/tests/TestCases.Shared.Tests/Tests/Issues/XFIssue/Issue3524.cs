﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3524 : _IssuesUITest
{
	public Issue3524(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ICommand binding from a TapGestureRecognizer on a Span doesn't work";

	// [Test]
	// [Category(UITestCategories.Gestures)]
	// [FailsOnIOS]
	// public void SpanGestureCommand()
	// {
	// 	RunningApp.WaitForElement(kText);
	// 	RunningApp.Tap(kText);
	// 	RunningApp.WaitForElement($"{kText}: 1");
	// }
}