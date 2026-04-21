#if ANDROID // This test is Android-only because Issue #33510 (RefreshView triggering pull-to-refresh when scrolling inside a WebView) only reproduces on Android, and the test uses Android-specific Appium touch gesture APIs.
using NUnit.Framework;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Interactions;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.RefreshView)]
public class Issue33510 : _IssuesUITest
{
	const string StatusLabel = "StatusLabel";
	const string TestRefreshView = "TestRefreshView";

	public Issue33510(TestDevice device) : base(device)
	{
	}

	public override string Issue => "[Android] RefreshView triggers pull-to-refresh immediately when scrolling up inside a WebView";

	[Test]
	public void PullToRefreshShouldNotTriggerWhenWebViewIsScrolledDown()
	{
		VerifyInternetConnectivity();
		var androidApp = WaitForAndroidApp();
		var refreshViewRect = App.WaitForElement(TestRefreshView).GetRect();
		var x = refreshViewRect.CenterX();

		// Scroll content down by swiping up inside the WebView
		for (var i = 0; i < 3; i++)
		{
			DragInsideWebView(androidApp, x,
				refreshViewRect.Y + (refreshViewRect.Height * 70 / 100),
				x,
				refreshViewRect.Y + (refreshViewRect.Height * 25 / 100));
		}

		// Attempt pull-to-refresh (drag down) while content is still scrolled down
		DragInsideWebView(androidApp, x,
			refreshViewRect.Y + (refreshViewRect.Height * 30 / 100),
			x,
			refreshViewRect.Y + (refreshViewRect.Height * 80 / 100));

		Assert.That(App.FindElement(StatusLabel).GetText(), Does.Not.Contain("Refresh triggered"),
			"RefreshView should not trigger while WebView content is not at the top.");
	}

	AppiumAndroidApp WaitForAndroidApp()
	{
		if (App is not AppiumAndroidApp androidApp)
		{
			Assert.Ignore("Issue #33510 is Android-specific.");
			return null!;
		}

		Assert.That(
			App.WaitForTextToBePresentInElement(StatusLabel, "WebView ready", timeout: TimeSpan.FromSeconds(30)),
			Is.True,
			"WebView never finished loading.");

		return androidApp;
	}

	static void DragInsideWebView(AppiumAndroidApp androidApp, int fromX, int fromY, int toX, int toY)
	{
		var touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
		var dragSequence = new ActionSequence(touchDevice, 0);

		dragSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, fromX, fromY, TimeSpan.Zero));
		dragSequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
		dragSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, toX, toY, TimeSpan.FromMilliseconds(450)));
		dragSequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));

		androidApp.Driver.PerformActions([dragSequence]);
	}
}
#endif
