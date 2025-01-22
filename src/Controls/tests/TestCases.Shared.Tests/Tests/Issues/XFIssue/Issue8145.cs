﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8145 : _IssuesUITest
{
	public Issue8145(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell System.ObjectDisposedException: Cannot access a disposed object. Object name: Android.Support.Design.Widget.BottomSheetDialog";

	[Test]
	[Category(UITestCategories.Shell)]
	public void Issue8145ShellToolbarDisposedException()
	{

#if MACCATALYST
		// In CI the window goes to left bottom corner in Catalyst randomly to avoid the flakiness, use full screen mode to prevent dock overlap with UI elements.
		EnterFullScreen();
#endif
		App.WaitForElement("More");
		App.Tap("More");
		App.WaitForElementTillPageNavigationSettled("target");
		App.Tap("target");
		App.WaitForElementTillPageNavigationSettled("Success");
	}
}