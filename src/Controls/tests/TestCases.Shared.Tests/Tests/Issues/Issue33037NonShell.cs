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
	[TestCase("Issue33037ScrollViewButton", "Issue33037ScrollViewScroller", "Issue33037 Direct", "Item 40", null)]
	[TestCase("Issue33037GridScrollViewButton", "Issue33037GridScrollViewScroller", "Issue33037 Grid", "Item 40", null)]
	[TestCase("Issue33037ContentViewGridScrollViewButton", "Issue33037ContentViewGridScrollViewScroller", "Issue33037 Wrapped", "Item 40", null)]
	[TestCase("Issue33037DynamicContentViewGridScrollViewButton", "Issue33037DynamicContentViewGridScrollViewScroller", "Issue33037 Dynamic", "Item 40", null)]
	[TestCase("Issue33037ListViewButton", "Issue33037ListViewScroller", "Issue33037 List", "Item 40", null)]
	[TestCase("Issue33037CollectionViewButton", "Issue33037CollectionViewScroller", "Issue33037 Collection", "Item 40", null)]
	[TestCase("Issue33037LegacyCollectionViewButton", "Issue33037LegacyCollectionViewScroller", "Issue33037 Legacy Collection", "Item 40", null)]
	[TestCase("Issue33037NativeTableViewButton", "Issue33037NativeTableViewScroller", "Issue33037 Native", "Item 40", null)]
	[TestCase("Issue33037TableViewButton", "Issue33037TableViewScroller", "Issue33037 Table", "Item 40", null)]
	[TestCase("Issue33037FixedHeaderCollectionViewButton", "Issue33037FixedHeaderCollectionViewScroller", "Issue33037 Fixed Header", "Item 40", "Issue33037FixedHeader")]
	[TestCase("Issue33037ShortFixedHeaderCollectionViewButton", "Issue33037ShortFixedHeaderCollectionViewScroller", "Issue33037 Short Header", "Item 16", "Issue33037ShortFixedHeader")]
	public void LargeTitleCollapsesToVisibleStandardTitle(string buttonId, string scrollerId, string title, string targetItem, string fixedHeaderId)
	{
		RequireIOS26OrHigher();
		App.WaitForElement(buttonId).Click();

		try
		{
			var expandedTitleRect = GetNavigationTitleRect(title);
			App.WaitForElement(scrollerId);

			if (buttonId == "Issue33037ScrollViewButton")
			{
				var firstItemRect = App.WaitForElement("Issue33037DirectInstructions").GetRect();
				Assert.That(firstItemRect.Y, Is.InRange(expandedTitleRect.Bottom, expandedTitleRect.Bottom + 40),
					"The delegated top inset should position direct ScrollView content exactly once below the expanded title.");
			}
			else if (buttonId == "Issue33037LegacyCollectionViewButton")
			{
				var headerRect = App.WaitForElement("Issue33037LegacyCollectionViewHeader").GetRect();
				Assert.That(headerRect.Y, Is.GreaterThanOrEqualTo(expandedTitleRect.Bottom),
					"The legacy CollectionView header should start below the expanded navigation title.");
			}

			App.ScrollDown(scrollerId, swipePercentage: 0.8);
			App.ScrollDown(scrollerId, swipePercentage: 0.8);
			App.WaitForElement(targetItem);

			var collapsedTitleRect = GetNavigationTitleRect(title);

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
			var expandedTitleRect = GetNavigationTitleRect("Issue33037 Modal List");
			var expandedListRect = App.WaitForElement("Issue33037ModalListViewScroller").GetRect();

			App.ScrollDown("Issue33037ModalListViewScroller", swipePercentage: 0.8);
			App.WaitForElement("Item 17");

			var collapsedTitleRect = GetNavigationTitleRect("Issue33037 Modal List");
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

			var centerX = expandedListRect.X + expandedListRect.Width / 2;
			var startY = expandedTitleRect.Bottom + 50;
			var endY = expandedListRect.Bottom - 50;
			App.DragCoordinates(centerX, startY, centerX, endY);
			App.DragCoordinates(centerX, startY, centerX, endY);
			App.WaitForElement("Item 0");

			var restoredTitleRect = GetNavigationTitleRect("Issue33037 Modal List");
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
	public void WebViewWithFixedControlsPreservesLargeTitle()
	{
		RequireIOS26OrHigher();
		App.WaitForElement("Issue33037WebViewButton").Click();

		try
		{
			App.WaitForElement("Ready");
			var expandedTitleRect = GetNavigationTitleRect("Issue33037 Web");

			App.WaitForElement("Issue33037WebViewScrollButton").Click();
			App.WaitForElement("Scrolled");

			var titleRect = GetNavigationTitleRect("Issue33037 Web");
			Assert.That(titleRect.Height, Is.EqualTo(expandedTitleRect.Height).Within(2),
				"A WebView with fixed controls should retain its existing large-title layout.");
		}
		finally
		{
			App.Back();
		}
	}

	[Test]
	[Category(UITestCategories.Navigation)]
	public void ReporterScenarioDoesNotEnterMalformedTitleGeometry()
	{
		RequireIOS26OrHigher();
		App.WaitForElement("Issue33037ReporterScenarioButton").Click();

		try
		{
			var title = "Large Title Demo";
			var scrollerId = "Issue33037ReporterScroller";
			var expandedTitleRect = GetNavigationTitleRect(title);
			var expandedScrollerRect = App.WaitForElement(scrollerId).GetRect();

			Assert.That(expandedTitleRect.Height, Is.GreaterThan(30),
				"The reporter scenario should initially display a large navigation title.");

			App.ScrollDown(scrollerId, ScrollStrategy.Gesture, swipePercentage: 0.2);
			var collapsedTitleRect = GetNavigationTitleRect(title);
			var collapsedScrollerRect = App.WaitForElement(scrollerId).GetRect();

			Assert.That(collapsedTitleRect.Height, Is.LessThan(30),
				"The reporter scenario should display the compact title after crossing the collapse threshold.");
			Assert.That(collapsedScrollerRect.Y, Is.EqualTo(expandedScrollerRect.Y).Within(2),
				"The reporter's ListView frame must remain stable while the navigation bar collapses.");
			Assert.That(collapsedScrollerRect.Height, Is.EqualTo(expandedScrollerRect.Height).Within(2),
				"The reporter's ListView height must remain stable while the navigation bar collapses.");

			for (var i = 0; i < 3; i++)
			{
				App.ScrollUp(scrollerId, ScrollStrategy.Gesture, swipePercentage: 0.05);
				AssertValidReporterTitleGeometry(title, expandedTitleRect, collapsedScrollerRect);
				App.ScrollDown(scrollerId, ScrollStrategy.Gesture, swipePercentage: 0.05);
				AssertValidReporterTitleGeometry(title, expandedTitleRect, collapsedScrollerRect);
			}
		}
		finally
		{
			App.WaitForElement("Issue33037ReporterCloseButton").Click();
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
			var expandedTitleRect = GetNavigationTitleRect("Issue33037 Programmatic");

			App.WaitForElement("Issue33037ProgrammaticScrollButton").Click();
			App.WaitForElement("Item 50");

			var collapsedTitleRect = GetNavigationTitleRect("Issue33037 Programmatic");
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

			var collapsedTitleRect = GetNavigationTitleRect("Issue33037 Appearing");
			Assert.That(collapsedTitleRect.Height, Is.LessThan(30),
				"The navigation title should collapse when CollectionView.ScrollTo runs during OnAppearing.");
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

	[Test]
	[Category(UITestCategories.Navigation)]
	public void OrdinaryHeaderPreservesSafeAreaLayout()
	{
		RequireIOS26OrHigher();
		App.WaitForElement("Issue33037OrdinaryHeaderButton").Click();

		try
		{
			var titleRect = GetNavigationTitleRect("Issue33037 Ordinary Header");
			var headerRect = App.WaitForElement("Issue33037OrdinaryHeader").GetRect();
			var scrollerRect = App.WaitForElement("Issue33037OrdinaryHeaderScroller").GetRect();

			Assert.That(headerRect.Y, Is.GreaterThanOrEqualTo(titleRect.Y + titleRect.Height - 2),
				"An ordinary fixed header must remain below the navigation title.");
			Assert.That(scrollerRect.Y, Is.GreaterThanOrEqualTo(headerRect.Y + headerRect.Height - 2),
				"The scroll host must remain below an ordinary fixed header rather than being delegated edge-to-edge.");
		}
		finally
		{
			App.Back();
		}
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
			var title = GetNavigationTitleRect("Issue33037 No Large Title");
			var firstItem = App.WaitForElement("Item 0").GetRect();

			Assert.That(title.Height, Is.LessThan(60),
				"LargeTitleDisplayMode.Never should keep the compact navigation title.");
			Assert.That(firstItem.Y, Is.GreaterThanOrEqualTo(title.Bottom - 2),
				"A page which opts out of large titles must keep visible content below the navigation title.");
		}
		finally
		{
			App.Back();
		}
	}

	[Test]
	[Category(UITestCategories.Navigation)]
	public void OpaqueNavigationBarPreservesSafeAreaLayout()
	{
		RequireIOS26OrHigher();
		App.WaitForElement("Issue33037OpaqueNavigationButton").Click();

		try
		{
			var scroller = App.WaitForElement("Issue33037OpaqueNavigationScroller").GetRect();
			Assert.That(scroller.Y, Is.GreaterThan(20),
				"An opaque navigation bar must keep content in the normal safe-area layout.");
		}
		finally
		{
			App.WaitForElement("Issue33037OpaqueNavigationCloseButton").Click();
		}
	}

	void RequireIOS26OrHigher()
	{
		if (App is not AppiumIOSApp iosApp || !HelperExtensions.IsIOS26OrHigher(iosApp))
			Assert.Ignore("Issue #33037 only affects iOS 26 and later.");
	}

	void AssertValidReporterTitleGeometry(
		string title,
		System.Drawing.Rectangle expandedTitleRect,
		System.Drawing.Rectangle expectedScrollerRect)
	{
		var titleElements = App.FindElements(title);
		Assert.That(titleElements, Is.Not.Empty,
			"The reporter scenario must keep the navigation title visible while crossing the threshold.");

		var titleRects = titleElements
			.Select(titleElement => titleElement.GetRect())
			.Where(rect => rect.Height < 50)
			.ToArray();
		Assert.That(titleRects, Is.Not.Empty,
			"The accessibility tree did not expose the reporter's navigation title text.");

		var titleRect = titleRects[0];
		foreach (var additionalTitleRect in titleRects.Skip(1))
		{
			Assert.That(additionalTitleRect.X, Is.EqualTo(titleRect.X).Within(2));
			Assert.That(additionalTitleRect.Y, Is.EqualTo(titleRect.Y).Within(2));
			Assert.That(additionalTitleRect.Width, Is.EqualTo(titleRect.Width).Within(2));
			Assert.That(additionalTitleRect.Height, Is.EqualTo(titleRect.Height).Within(2),
				"The accessibility tree exposed simultaneous navigation titles with different geometry.");
		}

		var scrollerRect = App.WaitForElement("Issue33037ReporterScroller").GetRect();
		var hasCompactGeometry = titleRect.Height < 30 && titleRect.Y < expandedTitleRect.Y - 5;
		var hasExpandedGeometry = titleRect.Height > 30 && Math.Abs(titleRect.Y - expandedTitleRect.Y) <= 5;
		Assert.That(hasCompactGeometry || hasExpandedGeometry, Is.True,
			$"The title entered malformed threshold geometry: {titleRect}.");
		Assert.That(scrollerRect.Y, Is.EqualTo(expectedScrollerRect.Y).Within(2),
			"The reporter's ListView frame must not move during threshold gestures.");
		Assert.That(scrollerRect.Height, Is.EqualTo(expectedScrollerRect.Height).Within(2),
			"The reporter's ListView height must not change during threshold gestures.");
	}

	System.Drawing.Rectangle GetNavigationTitleRect(string title)
	{
		App.WaitForElement(title);
		var titleElements = App.FindElements(title);
		Assert.That(titleElements, Is.Not.Empty,
			$"The navigation title '{title}' should be visible.");

		return titleElements
			.Select(titleElement => titleElement.GetRect())
			.OrderBy(rect => rect.Height)
			.First();
	}
}
#endif
