#if IOS || MACCATALYST // Regression reported on iOS against 10.0.101; the page instruments the native UIScrollView
using System.Text.RegularExpressions;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class ScrollViewInitialOffsetInGridRow : _IssuesUITest
{
	public ScrollViewInitialOffsetInGridRow(TestDevice device) : base(device) { }

	public override string Issue => "ScrollView in a Grid star row is not at the top when the page appears";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void StaticContentWithHeader() => AssertRestsAtTop("PushButton");

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void AsyncContentWithHeader() => AssertRestsAtTop("PushAsyncButton");

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void StaticContentUnderTheBar() => AssertRestsAtTop("PushNoHeaderButton");

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void AsyncContentUnderTheBar() => AssertRestsAtTop("PushAsyncNoHeaderButton");

	// Guard, not a repro: an ordinary resize of an already laid-out scroll view does not
	// scroll it, and the arrange-time offset restore must keep it that way
	[Test]
	[Category(UITestCategories.ScrollView)]
	public void StaticContentResizedLater() => AssertRestsAtTop("PushStaticResizeButton");

	void AssertRestsAtTop(string pushButton)
	{
		App.WaitForElement(pushButton);
		App.Tap(pushButton);
		try
		{
			App.WaitForElement("FirstItem");

			// Platform oracle: the native offset rests at -AdjustedContentInset.Top and ScrollY is 0.
			// The page also reports the inset it ended up with, which the visual oracle below uses.
			var success = App.WaitForTextToBePresentInElement("ResultLabel", "Success", timeout: TimeSpan.FromSeconds(10));
			var text = App.FindElement("ResultLabel").GetText() ?? "";
			TestContext.Out.WriteLine($"[{pushButton}] {text}");

			if (Device == TestDevice.iOS)
			{
				var adjustedTop = double.Parse(Regex.Match(text, @"adjusted=\(([-\d.]+),").Groups[1].Value,
					System.Globalization.CultureInfo.InvariantCulture);

				// User-visible oracle: the first item's top sits exactly at the visible top of the
				// ScrollView plus its adjusted inset, content Padding (8), and item Margin (8).
				// Catalyst Appium rectangles use desktop screen coordinates and cannot be combined
				// with a UIKit content inset, so its native offset oracle below is authoritative.
				var scroll = App.WaitForElement("TheScrollView").GetRect();
				var first = App.WaitForElement("FirstItem").GetRect();
				var visibleTop = scroll.Y + adjustedTop;
				Assert.That(first.Y, Is.EqualTo(visibleTop + 16).Within(2),
					$"First item Y={first.Y}, expected visible top {visibleTop} + 16 (scroll frame Y={scroll.Y}, adjustedTop={adjustedTop}); {text}");
			}
			Assert.That(success, Is.True, $"ScrollView did not rest at the top when the page appeared: {text}");
		}
		finally
		{
			// Each case pushes its own page: return to the launcher for the next one
			App.WaitForElement("BackButton");
			App.Tap("BackButton");
		}
	}
}
#endif
