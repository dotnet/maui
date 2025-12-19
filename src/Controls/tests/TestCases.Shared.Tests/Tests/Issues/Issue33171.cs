using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33171 : _IssuesUITest
{
	public override string Issue => "When TitleBar.IsVisible = false the caption buttons become unresponsive on Windows";

	public Issue33171(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Window)]
	public void TitleBarCaptionButtonsResponsiveWhenIsVisibleFalse()
	{
		App.WaitForElement("ToggleTitleBarVisibilityButton");
		App.Tap("ToggleTitleBarVisibilityButton");
		App.Tap("ReduceWidthButton");
		var windowSize = App.FindElement("WindowSizeLabel").GetText();
		App.TapMaximizeButton();
		App.Tap("GetStatusButton");	
		var newWindowSize = App.FindElement("WindowSizeLabel").GetText();
		Assert.That(newWindowSize, Is.Not.EqualTo(windowSize), "Window size should change after maximizing the window.");
	}
}