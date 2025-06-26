﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3276 : _IssuesUITest
{
	const string First = "First";
	const string Second = "Second";

	public Issue3276(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Crashing Unknown cell parent type on ContextAction Bindings";

	[Fact]
	[Trait("Category", UITestCategories.ContextActions)]
	public void Issue3276Test()
	{
		App.TapTab(Second);
		App.TapTab(First);
		App.WaitForElement("second 1");
	}
}