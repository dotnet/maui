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
	[TestCase("Issue33037ScrollViewButton", "Issue33037ScrollViewScroller", "Issue33037 Direct", null)]
	[TestCase("Issue33037GridScrollViewButton", "Issue33037GridScrollViewScroller", "Issue33037 Grid", null)]
	[TestCase("Issue33037ContentViewGridScrollViewButton", "Issue33037ContentViewGridScrollViewScroller", "Issue33037 Wrapped", null)]
	[TestCase("Issue33037DynamicContentViewGridScrollViewButton", "Issue33037DynamicContentViewGridScrollViewScroller", "Issue33037 Dynamic", null)]
	[TestCase("Issue33037ListViewButton", "Issue33037ListViewScroller", "Issue33037 List", null)]
	[TestCase("Issue33037CollectionViewButton", "Issue33037CollectionViewScroller", "Issue33037 Collection", null)]
	[TestCase("Issue33037LegacyCollectionViewButton", "Issue33037LegacyCollectionViewScroller", "Issue33037 Legacy Collection", null)]
	[TestCase("Issue33037NativeTableViewButton", "Issue33037NativeTableViewScroller", "Issue33037 Native", null)]
	[TestCase("Issue33037TableViewButton", "Issue33037TableViewScroller", "Issue33037 Table", null)]
	[TestCase("Issue33037FixedHeaderCollectionViewButton", "Issue33037FixedHeaderCollectionViewScroller", "Issue33037 Fixed Header", "Issue33037FixedHeader")]
	[TestCase("Issue33037ShortFixedHeaderCollectionViewButton", "Issue33037ShortFixedHeaderCollectionViewScroller", "Issue33037 Short Header", "Issue33037ShortFixedHeader")]
	public void LargeTitleCollapsesToVisibleStandardTitle(string buttonId, string scrollerId, string title, string fixedHeaderId)
	{
		RequireIOS26OrHigher();
		App.WaitForElement(buttonId).Click();

		try
		{
			var expandedTitleRect = GetExpandedNavigationTitleRect(title);
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

			App.ScrollDown(scrollerId, ScrollStrategy.Gesture, swipePercentage: 0.8, withInertia: false);

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
	public void NestedSafeAreaLongTableScrollsWithoutDoubleTopInset()
	{
		RequireIOS26OrHigher();
		App.WaitForElement("Issue33037NestedSafeAreaTableButton").Click();

		try
		{
			var titleRect = GetExpandedNavigationTitleRect("Issue33037 Nested Safe Area");
			var firstItemRect = App.WaitForElement("Nested Item 0").GetRect();

			Assert.That(firstItemRect.Y, Is.InRange(titleRect.Bottom, titleRect.Bottom + 40),
				"A nested native scroll view which already uses its container safe area must not receive the system top inset a second time.");

			App.ScrollDown("Issue33037NestedSafeAreaTableScroller", ScrollStrategy.Gesture, swipePercentage: 0.8, withInertia: false);
			App.WaitForElement("Nested Item 15");

			// A scroll view pinned below the navigation bar can scroll, but it does not geometrically
			// participate in UIKit's large-title collapse transition.
			var titleAfterScrollRect = GetNavigationTitleRect("Issue33037 Nested Safe Area");
			Assert.That(titleAfterScrollRect.Height, Is.EqualTo(titleRect.Height).Within(1));
		}
		finally
		{
			App.Back();
		}
	}

	[Test]
	[Category(UITestCategories.Navigation)]
	public void NestedSafeAreaShortTableDoesNotDoubleTopInset()
	{
		RequireIOS26OrHigher();
		App.WaitForElement("Issue33037NestedSafeAreaShortTableButton").Click();

		try
		{
			var titleRect = GetExpandedNavigationTitleRect("Issue33037 Nested Safe Area");
			var firstItemRect = App.WaitForElement("Nested Item 0").GetRect();

			Assert.That(firstItemRect.Y, Is.InRange(titleRect.Bottom, titleRect.Bottom + 40),
				"A nested native scroll view which already uses its container safe area must not receive the system top inset a second time.");
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
			var expandedTitleRect = GetExpandedNavigationTitleRect("Issue33037 Modal List");
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
	public void ModalListViewLargeTitleSurvivesFullScreenCoverWithRotation()
	{
		RequireIOS26OrHigher();
		App.WaitForElement("Issue33037ModalListViewButton").Click();

		try
		{
			var title = "Issue33037 Modal List";
			var expandedTitleRect = GetExpandedNavigationTitleRect(title);
			var firstItemRect = App.WaitForElement("Item 0").GetRect();

			App.WaitForElement("Issue33037ModalListViewCoverButton").Click();
			App.WaitForElement("Done");
			App.SetOrientationLandscape();
			App.WaitForElement("Done");
			App.SetOrientationPortrait();
			App.WaitForElement("Done").Click();

			var restoredTitleRect = GetExpandedNavigationTitleRect(title);
			var restoredFirstItemRect = App.WaitForElement("Item 0").GetRect();

			Assert.That(restoredTitleRect.Y, Is.EqualTo(expandedTitleRect.Y).Within(2),
				"The expanded title should return to its original position after a full-screen cover rotates while presented.");
			Assert.That(restoredTitleRect.Height, Is.EqualTo(expandedTitleRect.Height).Within(2),
				"The expanded title should return to its original height after a full-screen cover rotates while presented.");
			Assert.That(restoredFirstItemRect.Y, Is.EqualTo(firstItemRect.Y).Within(2),
				"The first row should not retain a gap or overlap after the full-screen cover is dismissed.");
			Assert.That(restoredFirstItemRect.Y, Is.GreaterThanOrEqualTo(restoredTitleRect.Bottom - 2),
				"The first row must remain below the restored expanded title.");
		}
		finally
		{
			App.SetOrientationPortrait();
			if (App.FindElements("Done").Any())
				App.WaitForElement("Done").Click();
			App.WaitForElement("Issue33037ModalListViewCloseButton").Click();
		}
	}

	[Test]
	[Category(UITestCategories.Navigation)]
	public void ModalListViewRecoversFromFullScreenCoverAfterCollapsing()
	{
		RequireIOS26OrHigher();
		App.WaitForElement("Issue33037ModalListViewButton").Click();

		try
		{
			var title = "Issue33037 Modal List";
			var scrollerId = "Issue33037ModalListViewScroller";
			var expandedTitleRect = GetExpandedNavigationTitleRect(title);
			var listRect = App.WaitForElement(scrollerId).GetRect();

			App.ScrollDown(scrollerId, ScrollStrategy.Gesture, swipePercentage: 0.8, withInertia: false);
			var collapsedTitleRect = GetNavigationTitleRect(title);

			App.WaitForElement("Issue33037ModalListViewCoverButton").Click();
			App.WaitForElement("Done").Click();

			var titleAfterDismiss = GetNavigationTitleRect(title);
			Assert.That(titleAfterDismiss.Height, Is.EqualTo(collapsedTitleRect.Height).Within(2),
				"Presenting a full-screen cover must not expand an intentionally collapsed title.");

			var centerX = listRect.X + listRect.Width / 2;
			var startY = expandedTitleRect.Bottom + 50;
			var endY = listRect.Bottom - 50;
			App.DragCoordinates(centerX, startY, centerX, endY);
			App.DragCoordinates(centerX, startY, centerX, endY);
			App.WaitForElement("Item 0");

			var restoredTitleRect = GetExpandedNavigationTitleRect(title);
			var restoredFirstItemRect = App.WaitForElement("Item 0").GetRect();
			Assert.That(restoredFirstItemRect.Y, Is.InRange(restoredTitleRect.Bottom - 2, restoredTitleRect.Bottom + 40),
				"The ListView should return to Item 0 without a gap or overlap after the cover round trip.");
		}
		finally
		{
			if (App.FindElements("Done").Any())
				App.WaitForElement("Done").Click();
			App.WaitForElement("Issue33037ModalListViewCloseButton").Click();
		}
	}

	[Test]
	[Category(UITestCategories.Navigation)]
	public async Task WebViewWithFixedControlsPreservesLargeTitle()
	{
		RequireIOS26OrHigher();
		App.WaitForElement("Issue33037WebViewButton").Click();

		try
		{
			App.WaitForElement("Ready");
			await Task.Delay(1000);
			var initialTitleRect = GetExpandedNavigationTitleRect("Issue33037 Web");

			App.WaitForElement("Issue33037WebViewScrollButton").Click();
			App.WaitForElement("Scrolled");

			var titleRect = GetNavigationTitleRect("Issue33037 Web");
			Assert.That(titleRect.Height, Is.EqualTo(initialTitleRect.Height).Within(2),
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
			var expandedTitleRect = GetExpandedNavigationTitleRect(title);
			var expandedScrollerRect = App.WaitForElement(scrollerId).GetRect();

			Assert.That(expandedTitleRect.Height, Is.GreaterThan(30),
				"The reporter scenario should initially display a large navigation title.");

			var firstItemRect = App.WaitForElement("Item 0").GetRect();
			Assert.That(firstItemRect.Y, Is.GreaterThanOrEqualTo(expandedTitleRect.Bottom - 2),
				"The reporter's first ListView row must remain below the translucent large-title navigation bar.");

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

			var centerX = expandedScrollerRect.X + expandedScrollerRect.Width / 2;
			var startY = expandedTitleRect.Bottom + 50;
			var endY = expandedScrollerRect.Bottom - 50;
			App.DragCoordinates(centerX, startY, centerX, endY);
			App.DragCoordinates(centerX, startY, centerX, endY);
			App.WaitForElement("Item 0");

			var restoredTitleRect = GetNavigationTitleRect(title);
			var stableRestoredTitleRect = GetNavigationTitleRect(title);
			Assert.That(restoredTitleRect.Height, Is.EqualTo(expandedTitleRect.Height).Within(2),
				"The reporter's large title should fully restore after reversing across the threshold.");
			Assert.That(stableRestoredTitleRect.Height, Is.EqualTo(restoredTitleRect.Height).Within(2),
				"The restored large title should remain stable after the reverse transition completes.");
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
			var expandedTitleRect = GetExpandedNavigationTitleRect("Issue33037 Programmatic");

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

	[Test]
	[Category(UITestCategories.Navigation)]
	[TestCase("Issue33037CollectionViewButton", "Issue33037 Collection")]
	[TestCase("Issue33037ListViewButton", "Issue33037 List")]
	[TestCase("Issue33037OverlayCollectionViewButton", "Issue33037 Overlay")]
	[TestCase("Issue33037NativeTableViewButton", "Issue33037 Native")]
	public void LargeTitleSurvivesOrientationRoundTrip(string buttonId, string title)
	{
		RequireIOS26OrHigher();
		App.WaitForElement(buttonId).Click();

		try
		{
			var expandedTitleRect = GetExpandedNavigationTitleRect(title);
			var firstItemRect = App.WaitForElement("Item 0").GetRect();

			App.SetOrientationLandscape();
			App.WaitForElement(title);

			App.SetOrientationPortrait();
			App.WaitForElement(title);

			// UIKit shrinks the navigation bar to the compact height class in landscape. If the
			// expanded height is not restored on the way back, the bar keeps measuring as collapsed
			// while it still draws the large title, so the title slides up into the status bar (and
			// can be clipped away entirely) and the page's top inset stays short.
			var restoredTitleRect = GetExpandedNavigationTitleRect(title);
			Assert.That(restoredTitleRect.Y, Is.EqualTo(expandedTitleRect.Y).Within(2),
				$"The '{title}' large title should return to its expanded position after a portrait/landscape/portrait round trip.");
			Assert.That(restoredTitleRect.Height, Is.EqualTo(expandedTitleRect.Height).Within(2),
				$"The '{title}' large title should return to its expanded height after a portrait/landscape/portrait round trip.");

			var restoredFirstItemRect = App.WaitForElement("Item 0").GetRect();
			Assert.That(restoredFirstItemRect.Y, Is.EqualTo(firstItemRect.Y).Within(2),
				"The first row should return to its original position after a portrait/landscape/portrait round trip.");
			Assert.That(restoredFirstItemRect.Y, Is.GreaterThanOrEqualTo(restoredTitleRect.Bottom - 2),
				"The first row must stay below the expanded navigation title after rotating back to portrait.");
		}
		finally
		{
			App.SetOrientationPortrait();
			App.Back();
		}
	}

	[Test]
	[Category(UITestCategories.Navigation)]
	[TestCase("Issue33037CollectionViewButton", "Issue33037CollectionViewScroller", "Issue33037 Collection")]
	[TestCase("Issue33037ListViewButton", "Issue33037ListViewScroller", "Issue33037 List")]
	public void CollapsedTitleStaysCollapsedThroughOrientationRoundTrip(string buttonId, string scrollerId, string title)
	{
		RequireIOS26OrHigher();
		App.WaitForElement(buttonId).Click();

		try
		{
			App.ScrollDown(scrollerId, ScrollStrategy.Gesture, 0.8, 500);
			var collapsedTitleRect = GetNavigationTitleRect(title);

			App.SetOrientationLandscape();
			App.WaitForElement(title);
			App.SetOrientationPortrait();

			var restoredTitleRect = GetNavigationTitleRect(title);
			Assert.That(restoredTitleRect.Height, Is.EqualTo(collapsedTitleRect.Height).Within(2),
				$"The collapsed '{title}' title should remain compact after a portrait/landscape/portrait round trip.");
			Assert.That(restoredTitleRect.Y, Is.EqualTo(collapsedTitleRect.Y).Within(2),
				$"The collapsed '{title}' title should return to its compact position after a portrait/landscape/portrait round trip.");
		}
		finally
		{
			App.SetOrientationPortrait();
			App.Back();
		}
	}

	[Test]
	[Category(UITestCategories.Navigation)]
	public void EdgeExtendedCollectionViewKeepsTopInsetFromTheFirstLayout()
	{
		RequireIOS26OrHigher();
		App.WaitForElement("Issue33037OverlayCollectionViewButton").Click();

		try
		{
			var expandedTitleRect = GetExpandedNavigationTitleRect("Issue33037 Overlay");
			var firstItemRect = App.WaitForElement("Item 0").GetRect();

			Assert.That(firstItemRect.Y, Is.InRange(expandedTitleRect.Bottom, expandedTitleRect.Bottom + 40),
				"The first row should settle exactly once below the expanded navigation title.");

			// The scenario page records the native top inset once per rendered frame and only
			// publishes the range once it has observed a full second of visible frames, so this waits
			// for that completion signal instead of racing a partial reading.
			Assert.That(
				App.WaitForTextToBePresentInElement("Issue33037OverlayCollectionViewObservationState", "complete"),
				Is.True,
				"The scenario page never finished observing the native top inset.");

			var recordedRange = App.WaitForElement("Issue33037OverlayCollectionViewTopInsetRange").GetText() ?? string.Empty;
			var (minimumTopInset, maximumTopInset) = ParseRecordedTopInsetRange(recordedRange);

			// Ceding the system inset to UIKit before its safe-area propagation actually arrives
			// leaves the edge-extended scroll view with no inset for the first frames after a push,
			// which is visible as rows rendered underneath the navigation bar.
			Assert.That(minimumTopInset, Is.GreaterThan(0),
				$"An edge-extended CollectionView must never be laid out without a top inset, not even on the first layout pass after navigation (recorded '{recordedRange}').");
			Assert.That(minimumTopInset, Is.GreaterThanOrEqualTo(expandedTitleRect.Bottom - expandedTitleRect.Height - 20),
				$"The recorded top inset should always cover the navigation bar (recorded '{recordedRange}').");

			// The opposite failure: handing ownership to UIKit while MAUI's manual copy of the system
			// inset is still applied stacks both and pushes the rows down twice as far.
			Assert.That(maximumTopInset, Is.LessThanOrEqualTo(expandedTitleRect.Bottom + 40),
				$"The system top inset must be applied exactly once; a doubled inset means UIKit and MAUI both supplied it (recorded '{recordedRange}').");
		}
		finally
		{
			App.Back();
		}
	}

	[Test]
	[Category(UITestCategories.Navigation)]
	public void OverlayButtonRespondsToTheFirstTapInLandscape()
	{
		RequireIOS26OrHigher();

		try
		{
			App.SetOrientationLandscape();
			App.WaitForElement("Issue33037OverlayCollectionViewButton").Click();
			App.WaitForElement("Issue33037OverlayCollectionViewScroller");

			var overlayButton = App.WaitForElement("Issue33037OverlayCollectionViewOverlayButton");
			overlayButton.Click();

			// The overlay button sits on top of the edge-extended scroller. A stale frame or a
			// scroll view still swallowing touches makes the first tap disappear, which the reporter
			// saw as needing to click twice when the app starts in landscape.
			Assert.That(
				App.WaitForTextToBePresentInElement("Issue33037OverlayCollectionViewOverlayButton", "Overlay 1"),
				Is.True,
				"The first tap on the bottom overlay button must be delivered when the page opens in landscape.");
		}
		finally
		{
			App.SetOrientationPortrait();
			App.Back();
		}
	}

	static (int Minimum, int Maximum) ParseRecordedTopInsetRange(string text)
	{
		var parts = text.Split(';');
		return parts.Length == 2
			? (ParseRecordedTopInset(parts[0]), ParseRecordedTopInset(parts[1]))
			: (-1, -1);
	}

	static int ParseRecordedTopInset(string text) =>
		int.TryParse(text, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var value)
			? value
			: -1;

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

	System.Drawing.Rectangle GetExpandedNavigationTitleRect(string title)
	{
		var titleElement = App.WaitForElement(
			() => App.FindElements(title).FirstOrDefault(element =>
			{
				var height = element.GetRect().Height;
				return height is > 30 and < 60;
			}),
			$"The navigation title '{title}' should expand before the scenario starts.");

		return titleElement.GetRect();
	}
}
#endif
