﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Unreported1 : _IssuesUITest
{
	public Unreported1(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "NRE when switching page on Appearing";

	[Fact]
	[Trait("Category", UITestCategories.FlyoutPage)]
	public void Unreported1Test()
	{
		App.WaitForElement("Label");
	}
}