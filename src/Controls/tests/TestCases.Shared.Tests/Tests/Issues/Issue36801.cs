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

	// The three ContentInsetAdjustmentBehavior modes bake the safe area into the content
	// differently, so each is exercised. The page asserts the resolved native behavior, so a
	// mode that silently drifted fails instead of quietly testing a different branch —
	// exactly what happened when the landscape-notch fix (#35533, since reverted in #36580)
	// briefly remapped Default to Never.
	[Test]
	[Category(UITestCategories.ScrollView)]
	[TestCase("ModeDefaultButton", "Automatic")]
	[TestCase("ModeNoneButton", "Never")]
	// Also resolves to Never, but bakes the safe area into the arranged content, which None
	// does not — so it is the case that actually exercises the measured extent's baked padding
	[TestCase("ModeAllButton", "Never")]
	[TestCase("ModeContainerButton", "Always")]
	public void ScrollToExtremesInEachInsetMode(string modeButton, string expectedMode)
	{
		App.WaitForElement(modeButton);
		App.Tap(modeButton);

		App.Tap("ScrollToEndButton");
		var endSuccess = App.WaitForTextToBePresentInElement("EndResultLabel", "Success", timeout: TimeSpan.FromSeconds(10));
		var endText = App.FindElement("EndResultLabel").GetText();
		Assert.That(endSuccess, Is.True, $"[{expectedMode}] scroll to end did not reach the inset-aware maximum: {endText}");
		Assert.That(endText, Does.Contain($"mode={expectedMode}"), $"[{expectedMode}] resolved to a different inset mode: {endText}");

		App.Tap("ScrollToTopButton");
		var topSuccess = App.WaitForTextToBePresentInElement("TopResultLabel", "Success", timeout: TimeSpan.FromSeconds(10));
		var topText = App.FindElement("TopResultLabel").GetText();
		Assert.That(topSuccess, Is.True, $"[{expectedMode}] scroll to top did not land on the rest position: {topText}");
	}

	// Element targets resolve against the effective viewport, and each inset mode obscures it
	// differently: Automatic/Always through AdjustedContentInset, SafeAreaEdges.All by baking
	// the safe area into the content where AdjustedContentInset never reports it. The page's
	// oracle measures the probe's bottom edge against the unobscured viewport bottom in window
	// coordinates, so the mode where MAUI itself obscures the viewport is proven too.
	[Test]
	[Category(UITestCategories.ScrollView)]
	[TestCase("ModeDefaultButton", "Automatic")]
	[TestCase("ModeNoneButton", "Never")]
	// Also resolves to Never, but bakes the safe area into the content — the case where the
	// viewport shrink comes from MAUI's own arrange instead of a UIKit inset
	[TestCase("ModeAllButton", "Never")]
	[TestCase("ModeContainerButton", "Always")]
	public void ScrollToElementEndInEachInsetMode(string modeButton, string expectedMode)
	{
		App.WaitForElement(modeButton);
		App.Tap(modeButton);

		App.Tap("ScrollToProbeButton");
		var elementSuccess = App.WaitForTextToBePresentInElement("ElementResultLabel", "Success", timeout: TimeSpan.FromSeconds(10));
		var elementText = App.FindElement("ElementResultLabel").GetText();
		Assert.That(elementSuccess, Is.True, $"[{expectedMode}] ScrollToAsync(element, End) did not align the element with the visible viewport bottom: {elementText}");
		Assert.That(elementText, Does.Contain($"mode={expectedMode}"), $"[{expectedMode}] resolved to a different inset mode: {elementText}");
	}
}
#endif
