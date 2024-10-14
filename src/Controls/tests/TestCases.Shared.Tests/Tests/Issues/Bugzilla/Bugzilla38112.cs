﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.TableView)]
public class Bugzilla38112 : _IssuesUITest
{
	public Bugzilla38112(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Switch becomes reenabled when previous ViewCell is removed from TableView";

	// TODO from Xamarin.UITest migration, test fails, look into it later
	// [Test]
	// [FailsOnIOS]
	// public void Bugzilla38112_SwitchIsStillOnScreen ()
	// {
	// 	App.WaitForElement("Click");
	// 	App.Tap("Click");
	// 	App.WaitForElement("switch3");
	// }

	// [Test]
	// [FailsOnIOS]
	// public void Bugzilla38112_SwitchIsStillDisabled ()
	// {
	// 	App.WaitForElement("Click");
	// 	App.Tap("Click");
	// 	App.WaitForElement("switch3");
	// 	App.Tap("switch3");
	// 	App.WaitForNoElement("FAIL");
	// 	Assert.That(App.FindElement("resultlabel").GetText()?.Equals("FAIL",
	// 		StringComparison.OrdinalIgnoreCase), Is.False);
	// }
}