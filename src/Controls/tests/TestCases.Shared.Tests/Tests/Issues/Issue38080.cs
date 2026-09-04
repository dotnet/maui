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

		// Overscroll at the top extreme: the WebView is off-screen below the fold and the
		// ScrollView overscrolls past its top edge.
		for (int i = 0; i < 6; i++)
			App.ScrollUp("Issue38080ScrollView", ScrollStrategy.Gesture, 0.9, 100);

		// Prove the flings reached the top extreme.
		AssertSentinelDisplayed("Issue38080TopSentinel", "top");

		// Scroll/overscroll at the bottom extreme: the WebView is off-screen above the fold and the
		// ScrollView overscrolls past its bottom edge.
		for (int i = 0; i < 15; i++)
			App.ScrollDown("Issue38080ScrollView", ScrollStrategy.Gesture, 0.9, 100);

		// Prove the flings reached the bottom extreme: the bottom sentinel is ~100 rows below the
		// initial viewport, so ignored/flaky gestures would never display it and the test fails.
		AssertSentinelDisplayed("Issue38080BottomSentinel", "bottom");

		// The WebView is now off-screen at the bottom; back navigation also reproduced the
		// RenderThread crash while compositing. Assert we returned to the home page alive.
		App.Back();
		App.WaitForElement("Issue38080NavigateButton");
	}

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
}
#endif
