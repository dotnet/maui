#if ANDROID
using System;
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
	const int FlingCount = 8;

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
		App.WaitForElement(TopMarker);
		App.WaitForElement("Issue38080WebView");

		Assert.That(
			App.WaitForTextToBePresentInElement(
				TopMarker,
				"Loaded:True",
				timeout: TimeSpan.FromSeconds(10)),
			Is.True,
			"The offline WebView content did not finish loading.");

		var status = App.FindElement(TopMarker).GetText();
		Assert.That(status, Does.Contain("Attached:True"));
		Assert.That(status, Does.Contain("Hardware:True"));
		Assert.That(status, Does.Not.Contain("Width:0"));
		Assert.That(status, Does.Not.Contain("Height:0"));

		var webView = App.FindElement("Issue38080WebView").GetRect();
		Assert.That(webView.Width, Is.GreaterThan(0));
		Assert.That(webView.Height, Is.GreaterThan(0));

		FastFlingUp();
		FastFlingUp();
		Assert.That(App.WaitForElement(TopMarker).IsDisplayed(), Is.True);

		for (var i = 0; i < FlingCount; i++)
		{
			FastFlingDown();
		}

		Assert.That(App.WaitForElement(BottomMarker).IsDisplayed(), Is.True);
		FastFlingDown();
		Assert.That(App.WaitForElement(BottomMarker).IsDisplayed(), Is.True);

		for (var i = 0; i < FlingCount; i++)
		{
			FastFlingUp();
		}

		Assert.That(App.WaitForElement(TopMarker).IsDisplayed(), Is.True);
		FastFlingUp();
		Assert.That(App.WaitForElement(TopMarker).IsDisplayed(), Is.True);

		App.Back();
		App.WaitForElement(HomeMarker);
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
