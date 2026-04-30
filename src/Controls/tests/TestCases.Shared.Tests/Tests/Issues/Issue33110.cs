using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33110 : _IssuesUITest
{
	public Issue33110(TestDevice device) : base(device)
	{
	}

	public override string Issue => "GraphicsView dirtyRect dimensions should be integers, not fractional values";

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsViewDirtyRectShouldHaveIntegerDimensions()
	{
		App.WaitForElement("CheckButton");
		App.Tap("CheckButton");
		Assert.That(App.WaitForElement("ResultLabel").GetText(), Is.EqualTo("Pass"));
	}
}
