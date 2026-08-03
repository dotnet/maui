#if IOS // Validates iOS-specific UIScrollView AdjustedContentInset math with iOS-only instrumentation in the HostApp page
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36801 : _IssuesUITest
{
	public Issue36801(TestDevice device) : base(device) { }

	public override string Issue => "iOS ScrollToAsync clamps without AdjustedContentInset";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollToAsyncReachesInsetAwareExtremes()
	{
		App.WaitForElement("ScrollToEndButton");
		App.Tap("ScrollToEndButton");

		// The page compares the native content offset against the inset-aware maximum
		// (ContentSize + AdjustedContentInset.Bottom - Bounds.Height)
		var endSuccess = App.WaitForTextToBePresentInElement("EndResultLabel", "Success", timeout: TimeSpan.FromSeconds(5));
		var endText = App.FindElement("EndResultLabel").GetText();
		Assert.That(endSuccess, Is.True, $"Scroll to end did not reach the inset-aware maximum: {endText}");

		App.Tap("ScrollToTopButton");

		// Scrolling back to 0 must land on the natural rest position (-AdjustedContentInset.Top)
		var topSuccess = App.WaitForTextToBePresentInElement("TopResultLabel", "Success", timeout: TimeSpan.FromSeconds(5));
		var topText = App.FindElement("TopResultLabel").GetText();
		Assert.That(topSuccess, Is.True, $"Scroll to top did not land on the rest position: {topText}");
	}
}
#endif
