#if ANDROID
using System.Linq;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38080 : _IssuesUITest
{
	const string AppPackage = "com.microsoft.maui.uitests";
	const string BottomMarker = "Issue38080BottomMarker";
	const string HomeMarker = "Issue38080HomeMarker";
	const string NavigateButton = "Issue38080NavigateButton";
	const string ReproPageMarker = "Issue38080ReproPageMarker";
	const string TopMarker = "Issue38080TopMarker";
	const string WarmupContinueButton = "Issue38080WarmupContinueButton";
	const string WarmupReadyMarker = "Issue38080WarmupReadyMarker";
	const string WebViewMarker = "Issue38080WebView";
	static readonly AppiumQuery ReproWebViewQuery = AppiumQuery.ByXPath(
		$"//android.widget.ScrollView[@resource-id='{AppPackage}:id/{ReproPageMarker}']" +
		$"//android.webkit.WebView[@package='{AppPackage}' and not(ancestor::android.webkit.WebView)]");
	static readonly AppiumQuery WarmupWebViewQuery = AppiumQuery.ByXPath(
		$"//android.webkit.WebView[@package='{AppPackage}' and not(ancestor::android.webkit.WebView)]");

	public Issue38080(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Android WebView crashes during ScrollView overscroll";

	[Test]
	[Category(UITestCategories.WebView)]
	public void FastOverscrollAndBackNavigationWithOffscreenWebViewDoesNotCrash()
	{
		App.WaitForElement(HomeMarker);
		App.Tap(NavigateButton);
		Assert.That(App.WaitForElement(AppiumQuery.ByAccessibilityId(WarmupReadyMarker)).IsDisplayed(), Is.True);

		if (!SaveUIDiagnosticInfo("Issue38080-WarmupReady"))
			throw new InvalidOperationException("Could not capture the Issue 38080 warm-up viewport.");

		var warmupWebView = App.WaitForElement(WarmupWebViewQuery);
		Assert.That(warmupWebView.IsDisplayed(), Is.True);
		Assert.That(warmupWebView.GetRect().Width, Is.GreaterThan(0));
		Assert.That(warmupWebView.GetRect().Height, Is.GreaterThan(0));
		Assert.That(App.WaitForElement(WarmupContinueButton).IsEnabled(), Is.True);

		App.Tap(WarmupContinueButton);
		App.WaitForElement(ReproPageMarker);
		Assert.That(App.WaitForElement(AppiumQuery.ByAccessibilityId(TopMarker)).IsDisplayed(), Is.True);

		PrepareUntilDisplayed(WebViewMarker, SlowDragDown, "PrepareWebView");
		AssertWebViewDisplayed();
		PrepareUntilDisplayed(TopMarker, SlowDragUp, "ReturnTop");

		FastFlingUp();
		FastFlingUp();
		Assert.That(App.WaitForElement(AppiumQuery.ByAccessibilityId(TopMarker)).IsDisplayed(), Is.True);

		for (var pass = 0; pass < 3; pass++)
		{
			ScrollUntilDisplayed(WebViewMarker, FastFlingDown);
			AssertWebViewDisplayed();

			ScrollUntilDisplayed(BottomMarker, FastFlingDown);
			FastFlingDown();
			FastFlingDown();
			Assert.That(App.WaitForElement(BottomMarker).IsDisplayed(), Is.True);

			ScrollUntilDisplayed(WebViewMarker, FastFlingUp);
			ScrollUntilDisplayed(TopMarker, FastFlingUp);
			FastFlingUp();
			FastFlingUp();
			Assert.That(App.WaitForElement(AppiumQuery.ByAccessibilityId(TopMarker)).IsDisplayed(), Is.True);
		}

		App.Back();
		App.WaitForElement(HomeMarker);
	}

	void AssertWebViewDisplayed()
	{
		var webView = App.WaitForElement(ReproWebViewQuery);
		Assert.That(webView.IsDisplayed(), Is.True);
		Assert.That(webView.GetRect().Width, Is.GreaterThan(0));
		Assert.That(webView.GetRect().Height, Is.GreaterThan(0));
	}

	void ScrollUntilDisplayed(string automationId, System.Action gesture)
	{
		for (var attempt = 0; attempt < 10; attempt++)
		{
			var elements = FindElementsForMarker(automationId);

			if (elements.Any(element => element.IsDisplayed()))
				return;

			gesture();
		}

		Assert.Fail($"Element '{automationId}' was not visible after 10 real touch gestures.");
	}

	void PrepareUntilDisplayed(string automationId, System.Action gesture, string stage)
	{
		for (var completedGestures = 0; completedGestures <= 10; completedGestures++)
		{
			if (!SaveUIDiagnosticInfo($"Issue38080-{stage}-After{completedGestures}Gestures"))
				throw new InvalidOperationException($"Could not capture the Issue 38080 {stage} viewport after {completedGestures} preparation gestures.");

			var elements = FindElementsForMarker(automationId);

			if (elements.Any(element => element.IsDisplayed()))
				return;

			if (completedGestures < 10)
				gesture();
		}

		Assert.Fail($"Element '{automationId}' was not visible after 10 preparation gestures.");
	}

	System.Collections.Generic.IReadOnlyCollection<IUIElement> FindElementsForMarker(string automationId)
	{
		if (automationId == TopMarker)
			return App.FindElements(AppiumQuery.ByAccessibilityId(automationId));

		if (automationId == WebViewMarker)
			return App.FindElements(ReproWebViewQuery);

		return App.FindElements(automationId);
	}

	void SlowDragDown()
	{
		App.ScrollDown(
			ReproPageMarker,
			ScrollStrategy.Gesture,
			swipePercentage: 0.5,
			swipeSpeed: 1000,
			withInertia: false);
		App.WaitForElement(ReproPageMarker);
	}

	void SlowDragUp()
	{
		App.ScrollUp(
			ReproPageMarker,
			ScrollStrategy.Gesture,
			swipePercentage: 0.5,
			swipeSpeed: 1000,
			withInertia: false);
		App.WaitForElement(ReproPageMarker);
	}

	void FastFlingDown()
	{
		App.ScrollDown(
			ReproPageMarker,
			ScrollStrategy.Gesture,
			swipePercentage: 0.9,
			swipeSpeed: 100,
			withInertia: true);
		App.WaitForElement(ReproPageMarker);
	}

	void FastFlingUp()
	{
		App.ScrollUp(
			ReproPageMarker,
			ScrollStrategy.Gesture,
			swipePercentage: 0.9,
			swipeSpeed: 100,
			withInertia: true);
		App.WaitForElement(ReproPageMarker);
	}
}
#endif
