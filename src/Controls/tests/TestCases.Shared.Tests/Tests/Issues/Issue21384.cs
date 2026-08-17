using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21384 : _IssuesUITest
{
	public Issue21384(TestDevice device) : base(device)
	{
	}

	public override string Issue => "InputTransparent has no effect on Windows";

	[Test]
	[Category(UITestCategories.InputTransparent)]
	public void InputTransparentLabelAllowsButtonClick()
	{
		App.WaitForElement("CounterButton");
		App.Tap("CounterButton");

		Assert.That(App.FindElement("CounterButton").GetText(), Is.EqualTo("Clicked 1 time"));
	}

	[Test]
	[Category(UITestCategories.InputTransparent)]
	public void InputTransparentClippedLayoutAllowsButtonClick()
	{
		App.WaitForElement("LayoutCounterButton");
		App.Tap("LayoutCounterButton");

		Assert.That(App.FindElement("LayoutCounterButton").GetText(), Is.EqualTo("Clicked 1 time"));
	}
}
