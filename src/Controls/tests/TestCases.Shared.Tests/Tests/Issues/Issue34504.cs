using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34504 : _IssuesUITest
{
	public Issue34504(TestDevice device) : base(device) { }

	public override string Issue => "[iOS] Span TapGestureRecognizer does not work on the second line of the span, if the span is wrapped to the next line";

	[Test]
	[Category(UITestCategories.Label)]
	// The bug only manifests on iOS 26+ (not Android / Windows / MacCatalyst),
	// but the test is safe to run on all platforms — it will simply pass on unaffected ones.
	public void SpanTapGestureOnSecondLineShouldWork()
	{
		// Navigate to the second page — the bug only reproduces on a pushed page.
		App.WaitForElement("NavigateButton");
		App.Tap("NavigateButton");

		var labelRect = App.WaitForElement("SpanLabel").GetRect();

		// Ensure the label wrapped to multiple lines; if it's single-line the tap
		// cannot exercise the second-line bug and the test would be a false-positive.
		Assert.That(labelRect.Height, Is.GreaterThan(40), "SpanLabel must be tall enough to indicate multi-line text before tapping the second line.");

		// Tap near the bottom of the label to hit the second wrapped line of a span.
		App.TapCoordinates(labelRect.X + labelRect.Width / 2, labelRect.Y + labelRect.Height * 0.75f);

		Assert.That(App.WaitForElement("StatusLabel").GetText(), Is.EqualTo("Success"));
	}
}
