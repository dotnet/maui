using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29954: _IssuesUITest
{
	public Issue29954(TestDevice device) : base(device)
	{
	}

	public override string Issue => "The status bar is blank when ManualMAUITests sample project debugging on the Android API 36 emulator";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void StatusBarShouldBeVisibleOnAndroidAPI36()
	{
		App.WaitForElement("StatusBarDescriptionLabel");
		
		App.WaitForElement("ApiLevelLabel");

		// Test opening modal which should also have proper status bar handling
		App.WaitForElement("OpenModalButton");
		App.Click("OpenModalButton");
		App.WaitForElement("ModalStatusBarTestLabel");

		// The modal content should not overlap with the status bar
		// Close modal and verify main page is still correct
		App.Click("CloseModalButton");
		App.WaitForElement("StatusBarDescriptionLabel");
	}
}