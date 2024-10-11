﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2767 : _IssuesUITest
{
	public Issue2767(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ArgumentException: NaN not valid for height";

	// [Test]
	// [Category(UITestCategories.Grid)]
	// [FailsOnIOS]
	// public void Issue2767Test()
	// {
	// 	RunningApp.WaitForElement("Label 1:1");
	// 	Assert.IsEmpty(RunningApp.Query("Collapsed"));
	// }
}