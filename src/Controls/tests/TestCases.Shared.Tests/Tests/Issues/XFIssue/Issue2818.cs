using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.FlyoutPage)]
public class Issue2818 : _IssuesUITest
{
	public Issue2818(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Right-to-Left FlyoutPage in Xamarin.Forms Hamburger icon issue";

// 	[Test]
// 	public void RootViewMovesAndContentIsVisible()
// 	{
// 		var idiom = RunningApp.WaitForElement("Idiom");

// 		// This behavior is currently broken on a phone device Issue 7270
// 		if (idiom[0].ReadText() != "Tablet")
// 			return;

// 		RunningApp.Tap("OpenRootView");
// 		RunningApp.Tap("CloseRootView");
// 		RunningApp.SetOrientationLandscape();
// 		RunningApp.Tap("OpenRootView");
// 		var positionStart = RunningApp.WaitForElement("CloseRootView");
// 		RunningApp.Tap("ShowLeftToRight");

// 		var results = RunningApp.QueryUntilPresent(() =>
// 		{
// 			var secondPosition = RunningApp.Query("CloseRootView");

// 			if (secondPosition.Length == 0)
// 				return null;

// 			if (secondPosition[0].Rect.X < positionStart[0].Rect.X)
// 				return secondPosition;

// 			return null;
// 		});

// 		Assert.IsNotNull(results, "Flyout View Did not change flow direction correctly");
// 		Assert.AreEqual(1, results.Length, "Flyout View Did not change flow direction correctly");

// 	}

// #if __IOS__
// 	[Test]
// 	public void RootViewSizeDoesntChangeAfterBackground()
// 	{
// 		var idiom = RunningApp.WaitForElement("Idiom");
// 		// This behavior is currently broken on a phone device Issue 7270
// 		if (idiom[0].ReadText() != "Tablet")
// 			return;

// 		RunningApp.SetOrientationLandscape();
// 		RunningApp.Tap("CloseRootView");
// 		RunningApp.Tap("ShowLeftToRight");
// 		var windowSize = RunningApp.WaitForElement("RootLayout")[0];
// 		RunningApp.SendAppToBackground(TimeSpan.FromSeconds(5));
// 		var newWindowSize = RunningApp.WaitForElement("RootLayout")[0];
// 		Assert.AreEqual(newWindowSize.Rect.Width, windowSize.Rect.Width);
// 		Assert.AreEqual(newWindowSize.Rect.Height, windowSize.Rect.Height);

// 	}
// #endif
}