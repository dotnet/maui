// Crash is Android-specific: RenderThread GL functor SIGSEGVs when a non-null ClipBounds
// routes an off-screen WebView's compositing through GLFunctorDrawable on overscroll.
#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38080 : _IssuesUITest
{
	public Issue38080(TestDevice device) : base(device) { }

	public override string Issue => "Android SIGSEGV crash in GLFunctorDrawable when a ScrollView with an off-screen WebView is overscrolled";

	[Test]
	[Category(UITestCategories.WebView)]
	public void OffScreenWebViewInScrollViewShouldNotCrashOnOverscroll()
	{
		App.WaitForElement("Issue38080NavigateButton");
		App.Tap("Issue38080NavigateButton");
		App.WaitForElement("Issue38080Ready");

		// ScrollUp/ScrollDown silently no-op when the element lookup returns null, which would
		// turn this regression test into a false positive. Wait so a failed lookup fails the test.
		App.WaitForElement("Issue38080ScrollView");
		AssertFixedSizeWebViewIsMeasuredAndHardwareAccelerated();
		ScrollToAndAssertWebViewRendered();

		// Overscroll at the top extreme: the WebView is off-screen below the fold and the
		// ScrollView overscrolls past its top edge.
		ScrollToExtremeAndAssert("Issue38080TopSentinel", "top", scrollDown: false, swipePercentage: 0.9);

		// Scroll/overscroll at the bottom extreme: the WebView is off-screen above the fold and the
		// ScrollView overscrolls past its bottom edge.
		// Use a mid-viewport gesture for the downward pass: once the WebView is visible near the
		// bottom of the viewport, a bottom-origin fling can be consumed by the WebView itself instead
		// of the parent ScrollView, leaving the test stuck at the midpoint.
		ScrollToExtremeAndAssert("Issue38080BottomSentinel", "bottom", scrollDown: true, swipePercentage: 0.45);

		// The WebView is now off-screen at the bottom; back navigation also reproduced the
		// RenderThread crash while compositing. Assert we returned to the home page alive.
		App.Back();
		App.WaitForElement("Issue38080NavigateButton");
	}

	void ScrollToExtremeAndAssert(string automationId, string extreme, bool scrollDown, double swipePercentage)
	{
		for (int i = 0; i < 60 && !TrySentinelDisplayed(automationId); i++)
			ScrollOnce(scrollDown, swipePercentage);

		AssertSentinelDisplayed(automationId, extreme);

		// Exercise actual overscroll at the edge after proving we reached it.
		for (int overscroll = 0; overscroll < 5; overscroll++)
			ScrollOnce(scrollDown, swipePercentage);

		AssertSentinelDisplayed(automationId, extreme);
	}

	void ScrollOnce(bool scrollDown, double swipePercentage)
	{
		if (scrollDown)
			App.ScrollDown("Issue38080ScrollView", ScrollStrategy.Gesture, swipePercentage, 100);
		else
			App.ScrollUp("Issue38080ScrollView", ScrollStrategy.Gesture, swipePercentage, 100);
	}

	bool TrySentinelDisplayed(string automationId) =>
		App.FindElements(automationId).Any(element => element.IsDisplayed());

	// Displayed is a viewport check, not a view-hierarchy presence check: it proves the sentinel
	// row is actually on screen at the extreme, so ignored/flaky scroll gestures fail the test
	// instead of letting it pass as a false positive.
	void AssertSentinelDisplayed(string automationId, string extreme)
	{
		App.RetryAssert(() =>
		{
			var sentinel = App.WaitForElement(automationId, timeout: TimeSpan.FromSeconds(5));
			Assert.That(sentinel.IsDisplayed(), Is.True,
				$"The {extreme} extreme was not reached: {automationId} is not displayed after scrolling");
		}, timeout: TimeSpan.FromSeconds(30));
	}

	void AssertFixedSizeWebViewIsMeasuredAndHardwareAccelerated()
	{
		App.RetryAssert(() =>
		{
			var statusText = App.WaitForElement("Issue38080ClipBoundsStatus", timeout: TimeSpan.FromSeconds(5)).GetText();
			Assert.That(statusText, Does.Match(@"Size=[1-9]\d*x[1-9]\d*"),
				$"The diagnostic assertion should only pass after the native WebView has non-zero dimensions. Actual diagnostics: {statusText}");
			Assert.That(statusText, Does.Contain("IsAttachedToWindow=True"),
				$"The diagnostic assertion should only pass after the native WebView is attached. Actual diagnostics: {statusText}");
			Assert.That(statusText, Does.Contain("HardwareAccelerated=True"),
				$"This regression must exercise Android hardware-accelerated WebView rendering. Actual diagnostics: {statusText}");
			Assert.That(statusText, Does.Contain("ParentClipChildren=True"),
				$"The native WebView should be clipped by its Android parent wrapper. Actual diagnostics: {statusText}");
			Assert.That(statusText, Does.Not.Contain("DiagnosticsTimeout=True"),
				$"The native WebView diagnostics timed out before reaching a ready attached/hardware-accelerated state. Actual diagnostics: {statusText}");
		}, timeout: TimeSpan.FromSeconds(30));
	}

	void ScrollToAndAssertWebViewRendered()
	{
		for (int i = 0; i < 20; i++)
		{
			if (IsWebViewRenderedAndDisplayed())
				return;

			App.ScrollDown("Issue38080ScrollView", ScrollStrategy.Gesture, 0.25, 100);
		}

		AssertWebViewRenderedAndDisplayed();
	}

	bool IsWebViewRenderedAndDisplayed()
	{
		var topMarker = App.FindElements("Issue38080WebViewTopMarker").FirstOrDefault();
		var loadStatus = App.FindElements("Issue38080WebViewLoadStatus").FirstOrDefault();
		return topMarker?.IsDisplayed() == true &&
			loadStatus?.IsDisplayed() == true &&
			loadStatus.GetText().Contains("HtmlProbe=Issue38080 WebView HTML loaded");
	}

	void AssertWebViewRenderedAndDisplayed()
	{
		var topMarker = App.WaitForElement("Issue38080WebViewTopMarker", timeout: TimeSpan.FromSeconds(1));
		Assert.That(topMarker.IsDisplayed(), Is.True,
			"The marker immediately above the WebView must be visible before overscroll.");

		var loadStatus = App.WaitForElement("Issue38080WebViewLoadStatus", timeout: TimeSpan.FromSeconds(1));
		Assert.That(loadStatus.IsDisplayed(), Is.True,
			"The WebView load status label immediately below the WebView should be visible with the midpoint check.");
		Assert.That(loadStatus.GetText(), Does.Contain("HtmlProbe=Issue38080 WebView HTML loaded"),
			"The WebView HTML must finish loading before the off-screen overscroll checks run.");
	}
}
#endif
