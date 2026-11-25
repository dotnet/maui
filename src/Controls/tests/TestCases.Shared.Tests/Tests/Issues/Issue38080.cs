#if ANDROID // Issue #38080 is Android-specific: Android RenderThread could crash while overscrolling an off-screen WebView.
using System;
using System.Drawing;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38080 : _IssuesUITest
{
	public Issue38080(TestDevice device) : base(device) { }

	public override string Issue => "Android WebView in ScrollView should survive overscroll and back navigation";

	[TestCase(true)]
	[TestCase(false)]
	[Category(UITestCategories.WebView)]
	public void FixedWebViewInScrollViewShouldSurviveOverscrollAndBackNavigation(bool topFirst)
	{
		App.WaitForElement("OpenIssue38080");
		App.Tap("OpenIssue38080");
		App.WaitForElement("Issue38080Ready");

		App.Tap("Issue38080Center");
		RefreshAndAssertReady();
		AssertVisible("Issue 38080 WebView HTML loaded", "The rendered HTML should be visible at the midpoint.", 12);
		App.Screenshot($"Issue38080-{DirectionName(topFirst)}-midpoint");

		if (topFirst)
		{
			GoToExtremeAndOverscroll("Issue38080Top", "Issue38080TopSentinel", scrollTowardBottom: false, "top", topFirst);
			GoToExtremeAndOverscroll("Issue38080Bottom", "Issue38080BottomSentinel", scrollTowardBottom: true, "bottom", topFirst);
		}
		else
		{
			GoToExtremeAndOverscroll("Issue38080Bottom", "Issue38080BottomSentinel", scrollTowardBottom: true, "bottom", topFirst);
			GoToExtremeAndOverscroll("Issue38080Top", "Issue38080TopSentinel", scrollTowardBottom: false, "top", topFirst);
		}

		App.Back();
		App.WaitForElement("OpenIssue38080", timeout: TimeSpan.FromSeconds(30));
	}

	void GoToExtremeAndOverscroll(string button, string sentinel, bool scrollTowardBottom, string name, bool topFirst)
	{
		App.Tap(button);
		AssertVisible(sentinel, $"The {name} sentinel should be visible before overscroll.", 12);
		App.Screenshot($"Issue38080-{DirectionName(topFirst)}-{name}-before-overscroll");

		for (int i = 0; i < 5; i++)
			DragOnScrollViewRail(scrollTowardBottom);

		AssertVisible(sentinel, $"The {name} sentinel should remain visible after overscroll.", 12);
		App.Screenshot($"Issue38080-{DirectionName(topFirst)}-{name}-after-overscroll");
		RefreshAndAssertReady();
	}

	void RefreshAndAssertReady()
	{
		App.RetryAssert(() =>
		{
			App.Tap("Issue38080Refresh");
			var status = App.FindElement("Issue38080Status").GetText();
			Assert.Multiple(() =>
			{
				Assert.That(status, Does.Contain("Loaded=True"), $"WebView local HTML was not loaded. Status: {status}");
				Assert.That(status, Does.Match(@"Native=[1-9]\d*x[1-9]\d*"), $"Native WebView must have positive dimensions. Status: {status}");
				Assert.That(status, Does.Contain("Attached=True"), $"Native WebView must be attached. Status: {status}");
				Assert.That(status, Does.Contain("Hardware=True"), $"Native WebView must be hardware accelerated. Status: {status}");
			});
		});
	}

	void DragOnScrollViewRail(bool scrollTowardBottom)
	{
		var rect = App.WaitForElement("Issue38080ScrollView").GetRect();
		var x = rect.X + 6f;
		var top = rect.Y + 24f;
		var bottom = rect.Y + rect.Height - 24f;
		App.DragCoordinates(x, scrollTowardBottom ? bottom : top, x, scrollTowardBottom ? top : bottom);
	}

	void AssertVisible(string automationId, string message, int minHeight)
	{
		App.RetryAssert(() =>
		{
			var elementRect = App.WaitForElement(automationId).GetRect();
			var scrollRect = App.WaitForElement("Issue38080ScrollView").GetRect();
			var visible = Rectangle.Intersect(elementRect, scrollRect);
			Assert.Multiple(() =>
			{
				Assert.That(visible.Width, Is.GreaterThan(20),
					$"{message} ElementRect={elementRect}; ScrollRect={scrollRect}; VisibleRect={visible}");
				Assert.That(visible.Height, Is.GreaterThanOrEqualTo(minHeight),
					$"{message} ElementRect={elementRect}; ScrollRect={scrollRect}; VisibleRect={visible}");
			});
		});
	}

	static string DirectionName(bool topFirst) => topFirst ? "top-first" : "bottom-first";
}
#endif
