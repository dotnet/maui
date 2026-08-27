#if IOS || MACCATALYST   // This issue is only reproducible on iOS and MacCatalyst, so skip the test on other platforms.
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
		// On MacCatalyst the rendered position is slightly higher, so shift up by
		// half a visual line to land reliably on the tappable span.
		TapClickMe(location.X + 10, y, visualLineHeight, lineCenterOffset);

		Assert.That(App.WaitForTextToBePresentInElement("StatusLabel", "Success", timeout: TimeSpan.FromSeconds(3)),
			Is.True,
			"Tapping the span at its visual position should trigger the TapGestureRecognizer");
	}

	void TapClickMe(float x, float y, float visualLineHeight, float lineCenterOffset)
	{
#if MACCATALYST
			App.TapCoordinates(x, y + (visualLineHeight * 15) + lineCenterOffset - visualLineHeight * 0.5f);
#else
			App.TapCoordinates(x, y + (visualLineHeight * 15) + lineCenterOffset);
#endif
	}
}
#endif