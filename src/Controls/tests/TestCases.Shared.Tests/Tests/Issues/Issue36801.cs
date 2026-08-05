#if IOS || MACCATALYST // Validates UIScrollView AdjustedContentInset math with platform instrumentation in the HostApp page
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36801 : _IssuesUITest
{
	public Issue36801(TestDevice device) : base(device) { }

	public override string Issue => "iOS ScrollToAsync clamps without AdjustedContentInset";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollToAsyncReachesInsetAwareExtremes()
	{
		App.WaitForElement("ScrollToEndButton");

		// A ScrollToAsync issued before the first layout must also land on the inset-aware
		// maximum once the deferred request drains and the adjusted insets settle
		var deferredSuccess = App.WaitForTextToBePresentInElement("DeferredResultLabel", "Success", timeout: TimeSpan.FromSeconds(10));
		var deferredText = App.FindElement("DeferredResultLabel").GetText();
		Assert.That(deferredSuccess, Is.True, $"Deferred (pre-layout) scroll to end did not settle on the inset-aware maximum: {deferredText}");

		App.Tap("ScrollToEndButton");

		// The page measures where the content platform view actually rests: its last pixel
		// must sit exactly at the bottom of the unobscured viewport
		var endSuccess = App.WaitForTextToBePresentInElement("EndResultLabel", "Success", timeout: TimeSpan.FromSeconds(10));
		var endText = App.FindElement("EndResultLabel").GetText();
		Assert.That(endSuccess, Is.True, $"Scroll to end did not reach the inset-aware maximum: {endText}");

		// Independent user-visible oracle: the probe label at the very end of the content
		// must now be fully inside the page, above the bottom edge
		var probeRect = App.WaitForElement("ProbeLabel").GetRect();
		var pageRect = App.WaitForElement("PageRoot").GetRect();
		Assert.That(probeRect.Y, Is.GreaterThanOrEqualTo(pageRect.Y), "Probe label should be inside the page after scrolling to end");
		Assert.That(probeRect.Y + probeRect.Height, Is.LessThanOrEqualTo(pageRect.Y + pageRect.Height + 1),
			"Probe label should be fully visible above the bottom edge after scrolling to end");

		App.Tap("ScrollToTopButton");

		// Scrolling back to 0 must land on the natural rest position (-AdjustedContentInset.Top)
		var topSuccess = App.WaitForTextToBePresentInElement("TopResultLabel", "Success", timeout: TimeSpan.FromSeconds(10));
		var topText = App.FindElement("TopResultLabel").GetText();
		Assert.That(topSuccess, Is.True, $"Scroll to top did not land on the rest position: {topText}");
	}

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollToElementEndLandsInsideVisibleViewport()
	{
		App.WaitForElement("ScrollToProbeButton");
		App.Tap("ScrollToProbeButton");

		// The page asserts, in window coordinates, that the probe's bottom edge rests exactly
		// at the bottom of the unobscured viewport (frame bottom minus AdjustedContentInset.Bottom)
		var elementSuccess = App.WaitForTextToBePresentInElement("ElementResultLabel", "Success", timeout: TimeSpan.FromSeconds(10));
		var elementText = App.FindElement("ElementResultLabel").GetText();
		Assert.That(elementSuccess, Is.True, $"ScrollToAsync(element, End) did not align the element with the visible viewport bottom: {elementText}");

		var probeRect = App.WaitForElement("ProbeLabel").GetRect();
		var pageRect = App.WaitForElement("PageRoot").GetRect();
		Assert.That(probeRect.Y + probeRect.Height, Is.LessThanOrEqualTo(pageRect.Y + pageRect.Height + 1),
			"Probe label should be fully visible after ScrollToAsync(element, End)");
	}
}
#endif
