﻿using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Github1776 : _IssuesUITest
{
	public Github1776(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Button Released not being triggered";

	[Fact]
	[Trait("Category", UITestCategories.Button)]
	public void GitHub1776Test()
	{
		App.WaitForElement("TheButton");
		App.Tap("TheButton");
		Assert.That(App.FindElement("PressedLabel").GetText(), Is.EqualTo("Pressed: 1"));
		Assert.That(App.FindElement("ReleasedLabel").GetText(), Is.EqualTo("Released: 1"));
		Assert.That(App.FindElement("ClickedLabel").GetText(), Is.EqualTo("Clicked: 1"));
		Assert.That(App.FindElement("CommandLabel").GetText(), Is.EqualTo("Command: 1"));
	}
}
