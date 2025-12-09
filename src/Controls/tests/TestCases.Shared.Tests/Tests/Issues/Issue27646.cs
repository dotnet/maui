#if WINDOWS // MacCatalyst don't support programmatic window resizing. So, ignored test on MacCatalyst.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue27646 : _IssuesUITest
{
	public override string Issue => "AdaptiveTrigger not firing when changing window width programmatically only";

	public Issue27646(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Window)]
	public void AdaptiveTriggerShouldFireWhenWindowWidthChangedProgrammatically()
	{
		App.WaitForElement("ResizeButton");

		App.Tap("ResizeButton");
		
		var indicatorAfterFirstClick = App.FindElement("IndicatorLabel");
		Assert.That(indicatorAfterFirstClick.GetText(), Is.EqualTo("Narrow Window"),
			"Label should show 'Narrow Window' after resizing to 550px");

		App.Tap("ResizeButton");
		
		var indicatorAfterSecondClick = App.FindElement("IndicatorLabel");
		Assert.That(indicatorAfterSecondClick.GetText(), Is.EqualTo("Wide Window"),
			"Label should show 'Wide Window' after resizing to 650px");
	}
}
#endif
