#if TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/31670
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue23023 : _IssuesUITest
{
	public Issue23023(TestDevice device) : base(device)
	{
	}

	public override string Issue => "CarouselView does not scroll to the specified item at the end after resetting data even though the CurrentItem is updated correctly";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselScrollsToEndItemAfterReset()
	{
		// iOS 26 changes UICollectionView scroll callback behavior, causing incorrect
		// CurrentItem/Position during animated ScrollTo — tracked in:
		// https://github.com/dotnet/maui/issues/34965
		if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
		{
			Assert.Ignore("Skipped on iOS 26+ due to carousel scroll behavior change. Issue: https://github.com/dotnet/maui/issues/34965");
		}

		App.WaitForElement("Issue23023_ReloadItems");
		App.Tap("Issue23023_ReloadItems");
		App.Tap("Issue23023_ScrollToLastItem");

		VerifyScreenshot();
	}
}
#endif
