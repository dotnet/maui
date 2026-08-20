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
	[TestCase("Issue33037NativeTableViewButton", "Issue33037NativeTableViewScroller", "Issue33037 Native", "Item 40", null, null)]
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
	public void ModalListViewLargeTitleRemainsVisibleAfterScrollRoundTrip()
	{
		RequireIOS26OrHigher();
		App.WaitForElement("Issue33037ModalListViewButton").Click();

		try
		{
			var expandedTitleRect = App.WaitForElement("Issue33037 Modal List").GetRect();
			var expandedListRect = App.WaitForElement("Issue33037ModalListViewScroller").GetRect();

			App.ScrollDown("Issue33037ModalListViewScroller", swipePercentage: 0.8);
			App.WaitForElement("Item 17");

			var collapsedTitleRect = App.WaitForElement("Issue33037 Modal List").GetRect();
			var collapsedListRect = App.WaitForElement("Issue33037ModalListViewScroller").GetRect();
			Assert.That(collapsedTitleRect.Height, Is.GreaterThan(0),
				"The modal navigation title should remain visible after collapsing.");
			Assert.That(collapsedTitleRect.Height, Is.LessThan(expandedTitleRect.Height),
				"The modal navigation title should collapse after scrolling down.");
			Assert.That(collapsedTitleRect.Height, Is.LessThan(30),
				"The modal navigation title should use the collapsed standard-title size.");
			Assert.That(collapsedTitleRect.Y, Is.LessThan(130),
				"The modal navigation title should remain in the navigation bar after collapsing.");
			Assert.That(collapsedTitleRect.X + collapsedTitleRect.Width / 2,
				Is.EqualTo(collapsedListRect.X + collapsedListRect.Width / 2).Within(10),
				"The collapsed title should be centered in the navigation bar.");
			Assert.That(collapsedListRect.Y, Is.EqualTo(expandedListRect.Y).Within(2),
				"The scroll host must remain edge-to-edge while the navigation bar collapses.");
			Assert.That(collapsedListRect.Height, Is.EqualTo(expandedListRect.Height).Within(2),
				"The scroll host height must remain stable while the navigation bar collapses.");

			App.ScrollUp("Issue33037ModalListViewScroller", swipePercentage: 0.8);
			App.WaitForElement("Item 0");

			var restoredTitleRect = App.WaitForElement(
				"Issue33037 Modal List",
				"The modal navigation title disappeared after scrolling back to the top.").GetRect();
			Assert.That(restoredTitleRect.Height, Is.GreaterThan(collapsedTitleRect.Height),
				"The modal navigation title should expand again after scrolling back to the top.");
		}
		finally
		{
			App.WaitForElement("Issue33037ModalListViewCloseButton").Click();
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

		[Test]
		[Category(UITestCategories.Navigation)]
		public void OrdinaryHeaderPreservesSafeAreaLayout()
		{
			RequireIOS26OrHigher();
			App.WaitForElement("Issue33037OrdinaryHeaderButton").Click();

			try
			{
				var titleRect = App.WaitForElement("Issue33037 Ordinary Header").GetRect();
				var headerRect = App.WaitForElement("Issue33037OrdinaryHeader").GetRect();
				var scrollerRect = App.WaitForElement("Issue33037OrdinaryHeaderScroller").GetRect();

				Assert.That(headerRect.Y, Is.GreaterThanOrEqualTo(titleRect.Y + titleRect.Height - 2),
					"An ordinary fixed header must remain below the navigation title.");
				Assert.That(scrollerRect.Y, Is.GreaterThanOrEqualTo(headerRect.Y + headerRect.Height - 2),
					"The scroll host must remain below an ordinary fixed header rather than being delegated edge-to-edge.");
			}

			[Test]
			[Category(UITestCategories.Navigation)]
			public void MultipleScrollCandidatesPreserveSafeAreaLayout()
			{
				RequireIOS26OrHigher();
				App.WaitForElement("Issue33037MultipleCandidatesButton").Click();

				try
				{
					var first = App.WaitForElement("Issue33037FirstCandidate").GetRect();
					var second = App.WaitForElement("Issue33037SecondCandidate").GetRect();

					Assert.That(first.Y, Is.GreaterThan(20),
						"Ambiguous scroll candidates must remain in the root safe-area layout.");
					Assert.That(second.Y, Is.EqualTo(first.Y).Within(2),
						"Neither ambiguous candidate should receive delegated top-inset ownership.");
				}
				finally
				{
					App.Back();
				}
			}

			[Test]
			[Category(UITestCategories.Navigation)]
			public void ExplicitSafeAreaOwnershipResetsDelegation()
			{
				RequireIOS26OrHigher();
				App.WaitForElement("Issue33037ExplicitSafeAreaButton").Click();

				try
				{
					var delegatedRect = App.WaitForElement("Issue33037ExplicitSafeAreaScroller").GetRect();
					Assert.That(delegatedRect.Y, Is.LessThanOrEqualTo(2),
						"The implicit scroll host should initially receive delegated top-inset ownership.");

					App.WaitForElement("Issue33037ExplicitSafeAreaToggle").Click();
					App.WaitForElement("Explicit safe-area ownership active");
					var explicitRect = App.WaitForElement("Issue33037ExplicitSafeAreaScroller").GetRect();

					Assert.That(explicitRect.Y, Is.GreaterThan(20),
						"Setting explicit SafeAreaEdges must reset delegated ownership and restore safe-area layout.");
				}
				finally
				{
					App.Back();
				}
			}

			[Test]
			[Category(UITestCategories.Navigation)]
			public void LargeTitleNeverPreservesSafeAreaLayout()
			{
				RequireIOS26OrHigher();
				App.WaitForElement("Issue33037LargeTitleNeverButton").Click();

				try
				{
					var title = App.WaitForElement("Issue33037 No Large Title").GetRect();
					var scroller = App.WaitForElement("Issue33037LargeTitleNeverScroller").GetRect();

					Assert.That(title.Height, Is.LessThan(60),
						"LargeTitleDisplayMode.Never should keep the compact navigation title.");
					Assert.That(scroller.Y, Is.GreaterThan(20),
						"A page which opts out of large titles must retain the normal safe-area layout.");
				}
				finally
				{
					App.Back();
				}
			}
			finally
			{
				App.Back();
			}
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
