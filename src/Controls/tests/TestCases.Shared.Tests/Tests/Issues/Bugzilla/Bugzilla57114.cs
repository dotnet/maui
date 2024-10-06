﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla57114 : _IssuesUITest
	{
		const string Testing = "Testing...";
		const string Success = "Success";
		const string ViewAutomationId = "_57114View";

		public Bugzilla57114(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Forms gestures are not supported on UIViews that have native gestures";

		// Crash after navigation
		/*
		[Test]
		[Category(UITestCategories.Gestures)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroid]
		[FailsOnIOS]
		[FailsOnMac]
		public void _57114BothTypesOfGesturesFire()
		{
			App.WaitForNoElement(Testing);
			App.Tap(ViewAutomationId);
			App.WaitForNoElement(Success);
		}
		*/
	}
}