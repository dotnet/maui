using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32279 : _IssuesUITest
{
	const string TapAnchorNoBackground = "TapAnchorNoBackground";
	const string TapAnchorWithBackground = "TapAnchorWithBackground";
	const string ResultLabelNoBackground = "ResultLabelNoBackground";
	const string ResultLabelWithBackground = "ResultLabelWithBackground";

	public Issue32279(TestDevice device) : base(device) { }

	public override string Issue => "TapGestureRecognizer does not work on layouts without a Background on Windows";

	[Test]
	[Category(UITestCategories.Gestures)]
	public void TapOnLayoutWithNoBackgroundShouldWork()
	{
		var anchor = App.WaitForElement(TapAnchorNoBackground).GetRect();

		// Tap below the label, inside the ContentView surface
		App.TapCoordinates(anchor.CenterX(), anchor.Y + anchor.Height + 50);

		var result = App.FindElement(ResultLabelNoBackground).GetText();

		Assert.That(result, Is.EqualTo("Tapped"));
	}

	[Test]
	[Category(UITestCategories.Gestures)]
	public void TapOnLayoutWithBackgroundShouldWork()
	{
		var anchor = App.WaitForElement(TapAnchorWithBackground).GetRect();

		// Tap below the label, inside the ContentView surface
		App.TapCoordinates(anchor.CenterX(), anchor.Y + anchor.Height + 50);

		var result = App.FindElement(ResultLabelWithBackground).GetText();

		Assert.That(result, Is.EqualTo("Tapped"));
	}
}