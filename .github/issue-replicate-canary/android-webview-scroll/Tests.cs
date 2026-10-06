#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38452 : _IssuesUITest
{
	public Issue38452(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Non-scrollable WebView forwards vertical gestures to its parent ScrollView";

	[Test]
	[Category(UITestCategories.WebView)]
	public void DraggingNonOverflowingWebViewScrollsTheOuterPage()
	{
		var loading = Stopwatch.StartNew();
		do
		{
			App.Tap("ProbeShortWebView");
			if (App.WaitForElement("ShortWebViewState").GetText() == "Short HTML loaded; no vertical overflow")
				break;
			if (loading.Elapsed >= TimeSpan.FromSeconds(8))
				throw new TimeoutException("The short local HTML was not loaded with a measurable, non-overflowing native WebView.");
			Task.Delay(200).Wait();
		} while (true);

		var control = App.WaitForElement("ControlSurface").GetRect();
		App.DragCoordinates(control.CenterX(), control.Y + control.Height * 0.8f,
			control.CenterX(), control.Y + control.Height * 0.2f);
		var controlOffset = WaitForOuterScroll();
		if (controlOffset <= 20)
			throw new TimeoutException($"The ordinary label gesture did not scroll the outer page: {controlOffset:F1}.");

		App.Tap("ResetOuterScroll");
		if (!App.WaitForTextToBePresentInElement("OuterScrollPosition", "0.0", TimeSpan.FromSeconds(5)))
			throw new TimeoutException("The outer ScrollView did not return to the top before the WebView gesture.");
		var webView = App.WaitForElement("ShortWebView").GetRect();
		var viewport = App.WaitForElement("OuterScrollView").GetRect();
		if (webView.Height <= 0 || webView.Y < viewport.Y ||
			webView.Y + webView.Height > viewport.Y + viewport.Height)
			throw new TimeoutException("The short WebView was not fully inside the native scroll viewport.");
		App.DragCoordinates(webView.CenterX(), webView.Y + webView.Height * 0.8f,
			webView.CenterX(), webView.Y + webView.Height * 0.2f);
		var webViewOffset = WaitForOuterScroll();
		Assert.That(webViewOffset, Is.GreaterThan(20),
			$"A gesture starting on non-overflowing HTML must scroll its parent, as the label control did ({controlOffset:F1}).");
	}

	double WaitForOuterScroll()
	{
		var wait = Stopwatch.StartNew();
		double offset;
		do
		{
			var text = App.WaitForElement("OuterScrollPosition").GetText();
			if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out offset) ||
				!double.IsFinite(offset))
				throw new TimeoutException("The outer ScrollView did not expose a finite numeric scroll position.");
			if (offset > 20 || wait.Elapsed >= TimeSpan.FromSeconds(3))
				return offset;
			Task.Delay(200).Wait();
		} while (true);
	}
}
#endif
