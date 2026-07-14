#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36505 : _IssuesUITest
{
	public Issue36505(TestDevice device) : base(device) { }

	public override string Issue =>
		"[iOS] Span TapGestureRecognizer hitbox is mispositioned inside FormattedString when the font contains a STAT table (line-height/leading mismatch)";

	[Test]
	[Category(UITestCategories.Label)]
	public void SpanTapHitboxShouldAlignWithRenderedTextWhenFontHasStatTable()
	{
		var label = App.WaitForElement("SpanLabel");
		var location = label.GetRect();

		// LineRef is a standalone single-line label with the same font and size.
		// Its height = pure visual line height (UILabel, no leading on iOS 16+).
		var visualLineHeight = App.WaitForElement("LineRef").GetRect().Height;
		var lineCenterOffset = visualLineHeight / 2;
		var y = location.Y;

		// Tap "Click me" (line 16) at its visual centre.
		App.TapCoordinates(location.X + 10, y + (visualLineHeight * 15) + lineCenterOffset);

		App.WaitForTextToBePresentInElement("StatusLabel", "Success");
	}
		
}
#endif