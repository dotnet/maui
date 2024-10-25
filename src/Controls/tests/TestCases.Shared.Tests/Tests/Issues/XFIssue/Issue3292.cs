﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3292 : _IssuesUITest
{
	public Issue3292(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TableSection.Title property binding fails in XAML";

	// [Test]
	// [Category(UITestCategories.TableView)]
	// [FailsOnIOS]
	// public void Issue3292Test()
	// {
	// 	App.WaitForElement(q => q.Marked("Hello World Changed"));
	// }
}