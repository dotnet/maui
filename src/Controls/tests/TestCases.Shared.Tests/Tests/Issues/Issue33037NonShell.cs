#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33037NonShell : _IssuesUITest
{
	public Issue33037NonShell(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "iOS Large Title display disappears when scrolling in non-Shell NavigationPage";

	[Test]
	[Category(UITestCategories.Navigation)]
	[TestCase("Issue33037ScrollViewButton", "Issue33037ScrollViewScroller", "Issue33037 Direct", "Item 40", "DirectScrollView", null)]
	[TestCase("Issue33037GridScrollViewButton", "Issue33037GridScrollViewScroller", "Issue33037 Grid", "Item 40", "GridScrollView", null)]
	[TestCase("Issue33037ContentViewGridScrollViewButton", "Issue33037ContentViewGridScrollViewScroller", "Issue33037 Wrapped", "Item 40", "ContentViewGridScrollView", null)]
	[TestCase("Issue33037DynamicContentViewGridScrollViewButton", "Issue33037DynamicContentViewGridScrollViewScroller", "Issue33037 Dynamic", "Item 40", "DynamicContentViewGridScrollView", null)]
	[TestCase("Issue33037ListViewButton", "Issue33037ListViewScroller", "Issue33037 List", "Item 40", "ListView", null)]
	[TestCase("Issue33037CollectionViewButton", "Issue33037CollectionViewScroller", "Issue33037 Collection", "Item 40", "CollectionView", null)]
	[TestCase("Issue33037TableViewButton", "Issue33037TableViewScroller", "Issue33037 Table", "Item 40", null, null)]
	[TestCase("Issue33037CandidateSelectionButton", "Issue33037CandidateSelectionScroller", "Issue33037 Candidates", "Item 40", null, null)]
	[TestCase("Issue33037FixedHeaderCollectionViewButton", "Issue33037FixedHeaderCollectionViewScroller", "Issue33037 Fixed Header", "Item 40", "FixedHeaderCollectionView", "Issue33037FixedHeader")]
	[TestCase("Issue33037ShortFixedHeaderCollectionViewButton", "Issue33037ShortFixedHeaderCollectionViewScroller", "Issue33037 Short Header", "Item 16", null, "Issue33037ShortFixedHeader")]
	public void LargeTitleCollapsesToVisibleStandardTitle(string buttonId, string scrollerId, string title, string targetItem, string scenarioName, string fixedHeaderId)
	{
		RequireIOS26OrHigher();
		App.WaitForElement(buttonId).Click();

		try
		{
			var expandedTitleRect = App.WaitForElement(title).GetRect();
			App.WaitForElement(scrollerId);

			if (buttonId == "Issue33037GridScrollViewButton")
			{
				VerifyScreenshot(
					"Issue33037NonShell_GridScrollView_BeforeScroll",
					tolerance: 0.5,
					retryTimeout: TimeSpan.FromSeconds(2));
			}

			App.ScrollDown(scrollerId, swipePercentage: 0.8);
			App.ScrollDown(scrollerId, swipePercentage: 0.8);
			App.WaitForElement(targetItem);

			var collapsedTitle = App.WaitForElement(title, $"Timed out waiting for collapsed title '{title}'");
			var collapsedTitleRect = collapsedTitle.GetRect();

			Assert.That(collapsedTitleRect.Height, Is.GreaterThan(0),
				$"The '{title}' navigation title should remain visible in the standard navigation bar after collapsing.");
			Assert.That(collapsedTitleRect.Height, Is.LessThan(expandedTitleRect.Height),
				$"The '{title}' navigation title should be shorter after the nested scroller moves.");
			Assert.That(collapsedTitleRect.Height, Is.LessThan(60),
				$"The '{title}' navigation title should use the collapsed standard-title size.");
			Assert.That(collapsedTitleRect.Y, Is.LessThan(130),
				$"The '{title}' navigation title should remain in the navigation bar after collapsing.");

			if (!string.IsNullOrEmpty(fixedHeaderId))
			{
				var fixedHeaderRect = App.WaitForElement(fixedHeaderId).GetRect();
				Assert.That(fixedHeaderRect.Y, Is.GreaterThanOrEqualTo(collapsedTitleRect.Y + collapsedTitleRect.Height - 2),
					"The fixed header should remain below the collapsed navigation title.");
			}

			if (!string.IsNullOrEmpty(scenarioName))
			{
				VerifyScreenshot(
					$"Issue33037NonShell_{scenarioName}_AfterScroll",
					cropBottom: fixedHeaderId is null ? 0 : 2016,
					tolerance: fixedHeaderId is null ? 0.5 : 1.5,
					retryTimeout: TimeSpan.FromSeconds(2));
			}
		}
		finally
		{
			App.Back();
		}
	}

	[Test]
	[Category(UITestCategories.Navigation)]
	public void ProgrammaticWebViewScrollCollapsesLargeTitle()
	{
		RequireIOS26OrHigher();
		App.WaitForElement("Issue33037WebViewButton").Click();

		try
		{
			App.WaitForElement("Ready");
			var expandedTitleRect = App.WaitForElement("Issue33037 Web").GetRect();

			App.WaitForElement("Issue33037WebViewScrollButton").Click();
			App.WaitForElement("Scrolled");

			var collapsedTitleRect = App.WaitForElement("Issue33037 Web").GetRect();
			Assert.That(collapsedTitleRect.Height, Is.LessThan(expandedTitleRect.Height),
				"The navigation title should be shorter after the wrapped WebView scrolls.");
			Assert.That(collapsedTitleRect.Height, Is.LessThan(60),
				"The navigation title should collapse after the wrapped WebView scrolls.");
		}
		finally
		{
			App.Back();
		}
	}

	[Test]
	[Category(UITestCategories.Navigation)]
	public void ProgrammaticScrollCollapsesLargeTitle()
	{
		RequireIOS26OrHigher();
		App.WaitForElement("Issue33037ProgrammaticCollectionViewButton").Click();

		try
		{
			App.WaitForElement("Issue33037ProgrammaticCollectionViewScroller");
			var expandedTitleRect = App.WaitForElement("Issue33037 Programmatic").GetRect();

			App.WaitForElement("Issue33037ProgrammaticScrollButton").Click();
			App.WaitForElement("Item 50");

			var collapsedTitleRect = App.WaitForElement("Issue33037 Programmatic").GetRect();
			Assert.That(collapsedTitleRect.Height, Is.LessThan(expandedTitleRect.Height),
				"The navigation title should be shorter after CollectionView.ScrollTo changes the wrapped scroller offset.");
			Assert.That(collapsedTitleRect.Height, Is.LessThan(60),
				"The navigation title should collapse after CollectionView.ScrollTo changes the wrapped scroller offset.");
		}
		finally
		{
			App.Back();
		}
	}

	[Test]
	[Category(UITestCategories.Navigation)]
	public void OnAppearingProgrammaticScrollCollapsesLargeTitle()
	{
		RequireIOS26OrHigher();
		App.WaitForElement("Issue33037AppearingCollectionViewButton").Click();

		try
		{
			App.WaitForElement("Issue33037AppearingCollectionViewScroller");
			App.WaitForElement("Item 50");

			var collapsedTitleRect = App.WaitForElement("Issue33037 Appearing").GetRect();
			Assert.That(collapsedTitleRect.Height, Is.LessThan(60),
				"The navigation title should collapse when CollectionView.ScrollTo runs during OnAppearing.");

			VerifyScreenshot(
				"Issue33037NonShell_AppearingCollectionView_AfterScroll",
				tolerance: 0.5,
				retryTimeout: TimeSpan.FromSeconds(2));
		}
		finally
		{
			App.Back();
		}
	}

	[Test]
	[Category(UITestCategories.Navigation)]
	public void HiddenNavigationBarPreservesTopSafeArea()
	{
		RequireIOS26OrHigher();
		App.WaitForElement("Issue33037HiddenNavigationBarButton").Click();

		try
		{
			var topMarker = App.WaitForElement("Issue33037HiddenNavigationBarTopMarker").GetRect();
			Assert.That(topMarker.Y, Is.GreaterThan(20),
				"Content should remain below the status bar when the navigation bar is hidden.");
		}
		finally
		{
			App.WaitForElement("Issue33037HiddenNavigationBarBackButton").Click();
		}
	}

	void RequireIOS26OrHigher()
	{
		if (App is not AppiumIOSApp iosApp || !HelperExtensions.IsIOS26OrHigher(iosApp))
			Assert.Ignore("Issue #33037 only affects iOS 26 and later.");
	}
}
#endif
