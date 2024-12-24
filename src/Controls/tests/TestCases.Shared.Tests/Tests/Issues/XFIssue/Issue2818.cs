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
	// 		var idiom = App.WaitForElement("Idiom");

	// 		// This behavior is currently broken on a phone device Issue 7270
	// 		if (idiom[0].ReadText() != "Tablet")
	// 			return;

	// 		App.Tap("OpenRootView");
	// 		App.Tap("CloseRootView");
	// 		App.SetOrientationLandscape();
	// 		App.Tap("OpenRootView");
	// 		var positionStart = App.WaitForElement("CloseRootView");
	// 		App.Tap("ShowLeftToRight");

	// 		var results = App.QueryUntilPresent(() =>
	// 		{
	// 			var secondPosition = App.Query("CloseRootView");

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
	// 		var idiom = App.WaitForElement("Idiom");
	// 		// This behavior is currently broken on a phone device Issue 7270
	// 		if (idiom[0].ReadText() != "Tablet")
	// 			return;

	// 		App.SetOrientationLandscape();
	// 		App.Tap("CloseRootView");
	// 		App.Tap("ShowLeftToRight");
	// 		var windowSize = App.WaitForElement("RootLayout")[0];
	// 		App.SendAppToBackground(TimeSpan.FromSeconds(5));
	// 		var newWindowSize = App.WaitForElement("RootLayout")[0];
	// 		Assert.AreEqual(newWindowSize.Rect.Width, windowSize.Rect.Width);
	// 		Assert.AreEqual(newWindowSize.Rect.Height, windowSize.Rect.Height);

	// 	}
	// #endif
}