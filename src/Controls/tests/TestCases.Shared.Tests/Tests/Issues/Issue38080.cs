#if ANDROID
using System.Linq;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38080 : _IssuesUITest
{
	const string BottomMarker = "Issue38080BottomMarker";
	const string HomeMarker = "Issue38080HomeMarker";
	const string NavigateButton = "Issue38080NavigateButton";
	const string ReproPageMarker = "Issue38080ReproPageMarker";
	const string TopMarker = "Issue38080TopMarker";
	const string WebViewMarker = "Issue38080WebView";

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
		App.WaitForElement(ReproPageMarker);
		Assert.That(App.WaitForElement(AppiumQuery.ByAccessibilityId(TopMarker)).IsDisplayed(), Is.True);

		FastFlingUp();
		FastFlingUp();
		Assert.That(App.WaitForElement(AppiumQuery.ByAccessibilityId(TopMarker)).IsDisplayed(), Is.True);

		for (var pass = 0; pass < 3; pass++)
		{
			ScrollUntilDisplayed(WebViewMarker, FastFlingDown);
			var webView = App.WaitForElement(WebViewMarker);
			Assert.That(webView.IsDisplayed(), Is.True);
			Assert.That(webView.GetRect().Width, Is.GreaterThan(0));
			Assert.That(webView.GetRect().Height, Is.GreaterThan(0));

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

	void ScrollUntilDisplayed(string automationId, System.Action gesture)
	{
		for (var attempt = 0; attempt < 10; attempt++)
		{
			var elements = automationId == TopMarker
				? App.FindElements(AppiumQuery.ByAccessibilityId(automationId))
				: App.FindElements(automationId);

			if (elements.Any(element => element.IsDisplayed()))
				return;

			gesture();
		}

		Assert.Fail($"Element '{automationId}' was not visible after 10 real touch gestures.");
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
