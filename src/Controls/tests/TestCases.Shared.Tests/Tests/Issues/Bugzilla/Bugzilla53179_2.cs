﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Bugzilla53179_2 : _IssuesUITest
{
	public Bugzilla53179_2(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Removing page during OnAppearing throws exception";

	// [Test]
	// [Category(UITestCategories.Navigation)]
	// [FailsOnAndroid]
	// public void RemovePageOnAppearingDoesNotCrash()
	// {
	// 	RunningApp.WaitForElement(Success);
	// }
}