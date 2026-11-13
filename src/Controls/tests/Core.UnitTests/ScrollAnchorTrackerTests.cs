using System;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	/// <summary>
	/// Drives <see cref="ScrollAnchorTracker"/> the way a platform does: report where the view is after each change, and
	/// scroll to the edge it follows without animating when it says so.
	/// </summary>
	public class ScrollAnchorTrackerTests
	{
		[Fact]
		public void StartsEngagedAndScrollsToTheEndOnTheFirstLook()
		{
			var view = new FakeScrollView(extent: 1000, viewport: 400);

			Assert.True(view.Tracker.IsAnchored);
			Assert.Equal(600, view.Look());
			Assert.Equal(600, view.Offset);
		}

		[Fact]
		public void ContentThatFitsIsAtTheEndWithNothingToScroll()
		{
			var view = new FakeScrollView(extent: 300, viewport: 400);

			Assert.Null(view.Look());
			Assert.True(view.Tracker.IsAnchored);

			view.Extent = 380;
			Assert.Null(view.Look());
			Assert.Equal(0, view.Offset);
		}

		[Fact]
		public void FollowsAppendedItems()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);

			view.Extent += 80;
			Assert.Equal(680, view.Look());

			view.Extent += 80;
			view.Extent += 120;
			Assert.Equal(880, view.Look());
			Assert.True(view.Tracker.IsAnchored);
		}

		[Fact]
		public void FollowsTheLastItemGrowingWhileTextStreamsIn()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);

			for (var step = 1; step <= 20; step++)
			{
				view.Extent += 17.5;
				Assert.Equal(view.Extent - 400, view.Look());
			}

			Assert.True(view.Tracker.IsAnchored);
			Assert.Equal(0, view.Metrics.DistanceToEnd);
		}

		[Fact]
		public void FollowsContentGrowingByLessThanAPixelAtATime()
		{
			// A device pixel at a time is a third of a unit at three pixels per unit: no single change is worth a scroll, but
			// together they move the end on.
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);

			for (var step = 1; step <= 80; step++)
			{
				view.Extent += 1.0 / 3;
				view.Look();
				Assert.True(view.Metrics.DistanceToEnd <= 0.5, $"After {step} steps, the end is {view.Metrics.DistanceToEnd} away.");
			}

			Assert.True(view.Tracker.IsAnchored);
		}

		[Fact]
		public void FollowsContentAddedAboveTheEndWhileAtTheEnd()
		{
			// A grouped log: the new entry lands in an earlier group, above the last one.
			var view = FakeScrollView.AtEnd(extent: 2000, viewport: 500);

			view.Extent += 64;
			Assert.Equal(1564, view.Look());
		}

		[Fact]
		public void FollowsItemsRemovedWhenThePlatformClampsTheOffset()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);

			// The platform moves the offset back with the shorter content in the same layout pass.
			view.Extent = 900;
			view.Offset = 500;
			Assert.Null(view.Look());
			Assert.True(view.Tracker.IsAnchored);
		}

		[Fact]
		public void FollowsItemsRemovedWhenThePlatformLeavesTheOffsetPastTheEnd()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);

			view.Extent = 900;
			Assert.Equal(500, view.Look());
			Assert.True(view.Tracker.IsAnchored);
		}

		[Fact]
		public void FollowsTheViewportResizing()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);

			// The keyboard shows...
			view.Viewport = 250;
			Assert.Equal(750, view.Look());

			// ...and hides; the platform clamps the offset to the new end.
			view.Viewport = 400;
			view.Offset = 600;
			Assert.Null(view.Look());
			Assert.True(view.Tracker.IsAnchored);
		}

		[Fact]
		public void FollowsAHeaderOrFooterChangingSize()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);

			view.Extent += 48;
			Assert.Equal(648, view.Look());
		}

		[Fact]
		public void AUserScrollAwayFromTheEndDisengages()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);

			view.Offset -= 4;
			Assert.Null(view.Look(userScrolling: true));
			Assert.True(view.Tracker.IsAnchored);

			view.Offset -= 6;
			Assert.Null(view.Look(userScrolling: true));
			Assert.False(view.Tracker.IsAnchored);

			// Once the user has left, nothing pulls the view back: not the end of the gesture, not new content.
			Assert.Null(view.Look());
			view.Extent += 300;
			Assert.Null(view.Look());
			Assert.Equal(590, view.Offset);
		}

		[Fact]
		public void AFlingAwayFromTheEndDisengages()
		{
			var view = FakeScrollView.AtEnd(extent: 5000, viewport: 400);

			foreach (var step in new[] { 120, 90, 60, 30, 10 })
			{
				view.Offset -= step;
				view.Look(userScrolling: true);
			}

			Assert.False(view.Tracker.IsAnchored);
			Assert.Null(view.Look());
		}

		[Fact]
		public void AUserScrollThatStaysWithinTheThresholdKeepsFollowing()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);

			view.Offset -= 6;
			Assert.Null(view.Look(userScrolling: true));
			Assert.True(view.Tracker.IsAnchored);

			// When the gesture ends, the anchor settles the view on the end.
			Assert.Equal(600, view.Look());
		}

		[Fact]
		public void NeverScrollsWhileTheUserScrolls()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);

			view.Offset -= 2;
			view.Extent += 200;
			Assert.Null(view.Look(userScrolling: true));
			view.Extent += 200;
			Assert.Null(view.Look(userScrolling: true));
			Assert.Equal(598, view.Offset);
		}

		[Fact]
		public void ContentGrowingWhileTheUserScrollsDoesNotCountAsTheUserMovingAway()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);

			view.Offset -= 3;
			view.Look(userScrolling: true);
			view.Extent += 250;
			view.Look(userScrolling: true);
			view.Offset -= 3;
			view.Look(userScrolling: true);
			Assert.True(view.Tracker.IsAnchored);

			// The gesture ends: catch up with the end, which moved on while the user held the view.
			Assert.Equal(850, view.Look());
		}

		[Fact]
		public void TheBounceAtTheEndOfAFlingDoesNotDisengage()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);

			// Flung past the end, then springing back to it.
			foreach (var offset in new[] { 640, 670, 660, 630, 610, 600 })
			{
				view.Offset = offset;
				view.Look(userScrolling: true);
			}

			Assert.True(view.Tracker.IsAnchored);
			Assert.Null(view.Look());
		}

		[Fact]
		public void TheBounceAtTheEndDoesNotDisengageWhenTheContentGrowsDuringIt()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);

			// Pulled past the end and springing back while a streamed answer grows the content: springing back from past
			// the end isn't moving away from it.
			foreach (var offset in new[] { 640, 680, 650 })
			{
				view.Offset = offset;
				view.Look(userScrolling: true);
			}

			view.Extent += 60;
			view.Offset = 630;
			Assert.Null(view.Look(userScrolling: true));
			Assert.True(view.Tracker.IsAnchored);

			// The bounce ends: catch up with the end, which moved on meanwhile.
			Assert.Equal(660, view.Look());
		}

		[Fact]
		public void AUserScrollFromPastTheEndCountsTheMoveShortOfTheEnd()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);
			view.Offset = 650;
			view.Look(userScrolling: true);

			view.Offset = 594;
			view.Look(userScrolling: true);
			Assert.True(view.Tracker.IsAnchored);

			view.Offset = 590;
			view.Look(userScrolling: true);
			Assert.False(view.Tracker.IsAnchored);
		}

		[Fact]
		public void ALayoutCorrectionMovingTheOffsetDoesNotDisengage()
		{
			var view = FakeScrollView.AtEnd(extent: 3000, viewport: 400);

			// A row above the viewport measured shorter than estimated, and the platform moved the offset with it.
			view.Extent -= 120;
			view.Offset -= 120;
			Assert.Null(view.Look());
			Assert.True(view.Tracker.IsAnchored);

			// Rows near the end measured taller than estimated while the offset moved back: still the layout's doing.
			view.Extent += 300;
			view.Offset -= 900;
			Assert.Equal(2780, view.Look());
			Assert.True(view.Tracker.IsAnchored);
		}

		[Fact]
		public void ALayoutThatResizesTheContentAndBackWhileMovingTheOffsetDoesNotDisengage()
		{
			// UICollectionView can re-estimate rows it measured before and measure them again in one layout pass: the
			// content ends up the size it was, but the offset moved on the way. The platform saw it move during layout.
			var view = FakeScrollView.AtEnd(extent: 967, viewport: 426.5);

			view.Offset -= 22;
			Assert.Equal(540.5, view.Look(sizeChanged: true));
			Assert.True(view.Tracker.IsAnchored);

			// The same move with no size change is a scroll.
			view.Offset -= 22;
			Assert.Null(view.Look());
			Assert.False(view.Tracker.IsAnchored);
		}

		[Fact]
		public void ALayoutMovingTheOffsetWhileTheUserScrollsDoesNotCountAsTheUserMovingAway()
		{
			// The user holds the view at its end while UICollectionView re-estimates rows in a layout pass: the offset
			// moved, but not by the user.
			var view = FakeScrollView.AtEnd(extent: 967, viewport: 426.5);

			view.Offset -= 22;
			Assert.Null(view.Look(userScrolling: true, sizeChanged: true));
			Assert.True(view.Tracker.IsAnchored);

			// When the gesture ends, the anchor settles the view on the end.
			Assert.Equal(540.5, view.Look());
			Assert.True(view.Tracker.IsAnchored);

			// The same move with no layout is the user's.
			view.Offset -= 22;
			Assert.Null(view.Look(userScrolling: true));
			Assert.False(view.Tracker.IsAnchored);
		}

		[Fact]
		public void AScrollAwayFromTheEndInStepsSmallerThanAPixelStillDisengages()
		{
			// An animated scroll the app requested, a short way: a quarter of a unit a frame.
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);

			for (var step = 0; step < 40; step++)
			{
				view.Offset -= 0.25;
				Assert.Null(view.Look());
			}

			Assert.False(view.Tracker.IsAnchored);

			// It stays where it went.
			view.Extent += 50;
			Assert.Null(view.Look());
		}

		[Fact]
		public void AScrollAwayFromTheEndThatTheUserDidNotDragStillDisengages()
		{
			// The keyboard, accessibility, or tapping the status bar to scroll to the top.
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);

			view.Offset -= 40;
			Assert.Null(view.Look());
			Assert.False(view.Tracker.IsAnchored);
		}

		[Fact]
		public void AScrollTowardTheEndIsNeverFought()
		{
			// An animation stepping toward the end: no jumps ahead of it.
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);
			view.Offset = 594;
			view.Look();

			view.Offset = 597;
			Assert.Null(view.Look());
			Assert.True(view.Tracker.IsAnchored);
		}

		[Fact]
		public void ScrollingBackToTheEndEngagesAgain()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);
			view.Offset = 200;
			view.Look(userScrolling: true);
			Assert.False(view.Tracker.IsAnchored);

			view.Offset = 450;
			view.Look(userScrolling: true);
			Assert.False(view.Tracker.IsAnchored);

			view.Offset = 594;
			view.Look(userScrolling: true);
			Assert.True(view.Tracker.IsAnchored);

			// The gesture ends a few points short: settle on the end, then follow it.
			Assert.Equal(600, view.Look());
			view.Extent += 100;
			Assert.Equal(700, view.Look());
		}

		[Fact]
		public void TheEndComingIntoViewWithoutAScrollEngagesAgain()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);
			view.Offset = 300;
			view.Look(userScrolling: true);
			Assert.False(view.Tracker.IsAnchored);

			// The log is cleared: everything fits, so there's nothing to scroll back through.
			view.Extent = 0;
			view.Offset = 0;
			Assert.Null(view.Look());
			Assert.True(view.Tracker.IsAnchored);

			view.Extent = 520;
			Assert.Equal(120, view.Look());
		}

		[Theory]
		[InlineData(0)]
		[InlineData(8)]
		[InlineData(20)]
		public void TheThresholdIsHowCloseToTheEndStillCounts(double threshold)
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400, threshold);

			view.Offset = 600 - threshold;
			view.Look(userScrolling: true);
			Assert.True(view.Tracker.IsAnchored);

			view.Offset -= 1;
			view.Look(userScrolling: true);
			Assert.False(view.Tracker.IsAnchored);

			view.Offset += 1;
			view.Look(userScrolling: true);
			Assert.True(view.Tracker.IsAnchored);
		}

		[Fact]
		public void ARequestedScrollAwayFromTheEndDisengagesWithoutBeingScrolledBack()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);

			view.Tracker.RequestScroll();
			// Where it landed, after a layout jump that also re-estimated the content's size (as on Android).
			view.Offset = 100;
			view.Extent = 1100;
			Assert.Null(view.Look());
			Assert.False(view.Tracker.IsAnchored);

			view.Extent += 100;
			Assert.Null(view.Look());
		}

		[Fact]
		public void ARequestedScrollToTheEndEngages()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);
			view.Offset = 100;
			view.Look(userScrolling: true);
			Assert.False(view.Tracker.IsAnchored);

			view.Tracker.RequestScroll();
			view.Offset = 600;
			view.Look();
			Assert.True(view.Tracker.IsAnchored);

			view.Extent += 50;
			Assert.Equal(650, view.Look());
		}

		[Fact]
		public void AnAnimatedRequestedScrollIsJudgedLikeAFling()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);

			view.Tracker.RequestScroll();
			view.Offset -= 30;
			view.Look(userScrolling: true);
			Assert.False(view.Tracker.IsAnchored);
		}

		[Fact]
		public void AnAnimatedRequestedScrollToTheEndEngagesWhenItArrives()
		{
			// An animation the platform doesn't report as the user's (iOS): judged on its first step, then step by step.
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);
			view.Offset = 100;
			view.Look(userScrolling: true);

			view.Tracker.RequestScroll();
			foreach (var offset in new[] { 250, 420, 540 })
			{
				view.Offset = offset;
				Assert.Null(view.Look());
				Assert.False(view.Tracker.IsAnchored);
			}

			view.Offset = 600;
			Assert.Null(view.Look());
			Assert.True(view.Tracker.IsAnchored);
		}

		[Fact]
		public void ResetEngagesAgainForNewItems()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);
			view.Offset = 0;
			view.Look(userScrolling: true);
			Assert.False(view.Tracker.IsAnchored);

			view.Tracker.Reset();
			Assert.True(view.Tracker.IsAnchored);
			Assert.Equal(600, view.Look());
		}

		[Fact]
		public void ResetForgetsAPendingRequestedScroll()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);

			view.Tracker.RequestScroll();
			view.Tracker.Reset();
			view.Offset = 0;
			view.Extent = 2000;
			Assert.Equal(1600, view.Look());
			Assert.True(view.Tracker.IsAnchored);
		}

		[Fact]
		public void EngageFollowsTheEndAgainWithoutJumpingAnAnimationToIt()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);
			view.Offset = 100;
			view.Look(userScrolling: true);
			Assert.False(view.Tracker.IsAnchored);

			// A "jump to latest" that animates: anchored from the start, and the animation runs its course.
			view.Tracker.Engage(ScrollAnchorMode.End);
			Assert.True(view.Tracker.IsAnchored);
			foreach (var offset in new[] { 250, 420, 600 })
			{
				view.Offset = offset;
				Assert.Null(view.Look());
				Assert.True(view.Tracker.IsAnchored);
			}

			view.Extent += 40;
			Assert.Equal(640, view.Look());
		}

		[Fact]
		public void EngageScrollsToTheEndWhenTheContentChangesSizeOnTheWay()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);
			view.Offset = 100;
			view.Look(userScrolling: true);

			view.Tracker.Engage(ScrollAnchorMode.End);
			view.Offset = 250;
			Assert.Null(view.Look());

			// Rows measured as the animation reaches them move the end: following it never animates.
			view.Extent = 1100;
			Assert.Equal(700, view.Look());
			Assert.True(view.Tracker.IsAnchored);
		}

		[Fact]
		public void EngageOverridesAPendingRequestedScroll()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);
			view.Offset = 100;
			view.Look(userScrolling: true);

			view.Tracker.RequestScroll();
			view.Tracker.Engage(ScrollAnchorMode.End);
			view.Offset = 250;
			Assert.Null(view.Look());
			Assert.True(view.Tracker.IsAnchored);
		}

		[Fact]
		public void IgnoresMetricsThatArentNumbers()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);

			Assert.Null(view.Tracker.Update(new ScrollAnchorMetrics(double.NaN, 1000, 400), isUserScrolling: false));
			Assert.Null(view.Tracker.Update(new ScrollAnchorMetrics(0, double.PositiveInfinity, 400), isUserScrolling: false));
			Assert.Null(view.Tracker.Update(new ScrollAnchorMetrics(0, 1000, -1), isUserScrolling: false));
			Assert.True(view.Tracker.IsAnchored);

			view.Extent += 10;
			Assert.Equal(610, view.Look());
		}

		[Fact]
		public void DoesNotScrollAViewThatHasNotBeenLaidOut()
		{
			var view = new FakeScrollView(extent: 1000, viewport: 0);

			Assert.Null(view.Look());

			view.Viewport = 400;
			Assert.Equal(600, view.Look());
		}

		[Theory]
		[InlineData(-1)]
		[InlineData(double.NaN)]
		public void RejectsAThresholdThatIsNegativeOrNotANumber(double threshold) =>
			Assert.Throws<ArgumentOutOfRangeException>(() => new ScrollAnchorTracker(ScrollAnchorMode.End, threshold));

		[Fact]
		public void MetricsMeasureTheDistanceToTheEnd()
		{
			var metrics = new ScrollAnchorMetrics(offset: 250, extent: 1000, viewport: 400);

			Assert.Equal(350, metrics.DistanceToEnd);
			Assert.Equal(600, metrics.EndOffset);
			Assert.Equal(0, new ScrollAnchorMetrics(0, 300, 400).EndOffset);
			Assert.Equal(600, metrics.WithOffset(600).Offset);
			Assert.Equal(0, metrics.WithOffset(600).DistanceToEnd);
		}

		[Fact]
		public void StartFollowsTheStartFromTheFirstLook()
		{
			var view = new FakeScrollView(extent: 1000, viewport: 400, edges: ScrollAnchorMode.Start);

			Assert.Equal(ScrollAnchorMode.Start, view.Tracker.AnchoredEdge);
			Assert.Null(view.Look());
			Assert.Equal(0, view.Offset);
			Assert.True(view.Tracker.IsAnchored);
		}

		[Fact]
		public void StartScrollsToTheStartOnTheFirstLookWhereTheViewIsNotThere()
		{
			// New items, where the platform kept the offset it had.
			var view = new FakeScrollView(extent: 1000, viewport: 400, edges: ScrollAnchorMode.Start) { Offset = 300 };

			Assert.Equal(0, view.Look());
			Assert.True(view.Tracker.IsAnchored);
		}

		[Fact]
		public void StartFollowsItemsAddedBeforeTheFirstItem()
		{
			var view = FakeScrollView.AtStart(extent: 1000, viewport: 400);

			// The platform kept the first row where it was, so the new row went in above the viewport (as on Android).
			view.Extent += 80;
			view.Offset += 80;
			Assert.Equal(0, view.Look());
			Assert.True(view.Tracker.IsAnchored);

			// The platform kept the offset, so the new row is in view already (as on iOS).
			view.Extent += 80;
			Assert.Null(view.Look());
			Assert.Equal(0, view.Offset);
			Assert.True(view.Tracker.IsAnchored);
		}

		[Fact]
		public void StartDoesNotFollowItemsAddedAfterTheLastItem()
		{
			var view = FakeScrollView.AtStart(extent: 1000, viewport: 400);

			view.Extent += 500;
			Assert.Null(view.Look());
			Assert.Equal(0, view.Offset);
			Assert.True(view.Tracker.IsAnchored);
		}

		[Fact]
		public void AUserScrollAwayFromTheStartDisengages()
		{
			var view = FakeScrollView.AtStart(extent: 1000, viewport: 400);

			view.Offset += 4;
			Assert.Null(view.Look(userScrolling: true));
			Assert.True(view.Tracker.IsAnchored);

			view.Offset += 6;
			Assert.Null(view.Look(userScrolling: true));
			Assert.False(view.Tracker.IsAnchored);

			// Once the user has left, nothing pulls the view back: not the end of the gesture, not items added before the
			// first one.
			Assert.Null(view.Look());
			view.Extent += 80;
			view.Offset += 80;
			Assert.Null(view.Look());
			Assert.Equal(90, view.Offset);
		}

		[Fact]
		public void TheBounceAtTheStartDoesNotDisengage()
		{
			var view = FakeScrollView.AtStart(extent: 1000, viewport: 400);

			// Pulled past the start, then springing back to it.
			foreach (var offset in new[] { -40, -70, -60, -30, -10, 0 })
			{
				view.Offset = offset;
				view.Look(userScrolling: true);
			}

			Assert.True(view.Tracker.IsAnchored);
			Assert.Null(view.Look());
		}

		[Fact]
		public void AUserScrollFromPastTheStartCountsTheMovePastTheStart()
		{
			var view = FakeScrollView.AtStart(extent: 1000, viewport: 400);
			view.Offset = -50;
			view.Look(userScrolling: true);

			view.Offset = 6;
			view.Look(userScrolling: true);
			Assert.True(view.Tracker.IsAnchored);

			view.Offset = 10;
			view.Look(userScrolling: true);
			Assert.False(view.Tracker.IsAnchored);
		}

		[Fact]
		public void ALayoutMovingTheOffsetAwayFromTheStartDoesNotDisengage()
		{
			var view = FakeScrollView.AtStart(extent: 967, viewport: 426.5);

			view.Offset += 22;
			Assert.Equal(0, view.Look(sizeChanged: true));
			Assert.True(view.Tracker.IsAnchored);

			// The same move with no size change is a scroll.
			view.Offset += 22;
			Assert.Null(view.Look());
			Assert.False(view.Tracker.IsAnchored);
		}

		[Fact]
		public void AScrollAwayFromTheStartInStepsSmallerThanAPixelStillDisengages()
		{
			var view = FakeScrollView.AtStart(extent: 1000, viewport: 400);

			for (var step = 0; step < 40; step++)
			{
				view.Offset += 0.25;
				Assert.Null(view.Look());
			}

			Assert.False(view.Tracker.IsAnchored);

			// It stays where it went.
			view.Extent += 50;
			view.Offset += 50;
			Assert.Null(view.Look());
		}

		[Fact]
		public void ScrollingBackToTheStartEngagesAgain()
		{
			var view = FakeScrollView.AtStart(extent: 1000, viewport: 400);
			view.Offset = 400;
			view.Look(userScrolling: true);
			Assert.False(view.Tracker.IsAnchored);

			view.Offset = 150;
			view.Look(userScrolling: true);
			Assert.False(view.Tracker.IsAnchored);

			view.Offset = 6;
			view.Look(userScrolling: true);
			Assert.True(view.Tracker.IsAnchored);

			// The gesture ends a few points short: settle on the start, then follow it.
			Assert.Equal(0, view.Look());
			view.Extent += 100;
			view.Offset += 100;
			Assert.Equal(0, view.Look());
		}

		[Fact]
		public void ARequestedScrollIsJudgedByWhereItLandsForTheStart()
		{
			var view = FakeScrollView.AtStart(extent: 1000, viewport: 400);

			view.Tracker.RequestScroll();
			view.Offset = 300;
			Assert.Null(view.Look());
			Assert.False(view.Tracker.IsAnchored);

			view.Tracker.RequestScroll();
			view.Offset = 0;
			Assert.Null(view.Look());
			Assert.Equal(ScrollAnchorMode.Start, view.Tracker.AnchoredEdge);
		}

		[Fact]
		public void EngagingTheStartFollowsItWithoutJumpingAnAnimationToIt()
		{
			var view = FakeScrollView.AtStart(extent: 1000, viewport: 400);
			view.Offset = 500;
			view.Look(userScrolling: true);
			Assert.False(view.Tracker.IsAnchored);

			view.Tracker.Engage(ScrollAnchorMode.Start);
			Assert.Equal(ScrollAnchorMode.Start, view.Tracker.AnchoredEdge);
			foreach (var offset in new[] { 350, 180, 0 })
			{
				view.Offset = offset;
				Assert.Null(view.Look());
				Assert.True(view.Tracker.IsAnchored);
			}

			view.Extent += 40;
			view.Offset += 40;
			Assert.Equal(0, view.Look());
		}

		[Fact]
		public void AViewThatFollowsOnlyItsStartDoesNotFollowItsEnd()
		{
			var view = FakeScrollView.AtStart(extent: 1000, viewport: 400);
			view.Offset = 600;
			view.Look(userScrolling: true);
			Assert.False(view.Tracker.IsAnchored);

			view.Extent += 100;
			Assert.Null(view.Look());
			Assert.Equal(600, view.Offset);
		}

		[Fact]
		public void ResetFollowsTheStartAgainWhereTheViewDoesNotFollowItsEnd()
		{
			var view = FakeScrollView.AtStart(extent: 1000, viewport: 400);
			view.Offset = 300;
			view.Look(userScrolling: true);

			view.Tracker.Reset();
			Assert.Equal(ScrollAnchorMode.Start, view.Tracker.AnchoredEdge);
			Assert.Equal(0, view.Look());
		}

		[Fact]
		public void BothEdgesStartAtTheEnd()
		{
			var view = new FakeScrollView(extent: 1000, viewport: 400, edges: ScrollAnchorMode.Start | ScrollAnchorMode.End);

			Assert.Equal(ScrollAnchorMode.End, view.Tracker.AnchoredEdge);
			Assert.Equal(600, view.Look());
		}

		[Fact]
		public void BothEdgesFollowTheEndOfContentThatFits()
		{
			var view = new FakeScrollView(extent: 300, viewport: 400, edges: ScrollAnchorMode.Start | ScrollAnchorMode.End);

			// Asked to follow the start, a view at both edges follows the end.
			view.Tracker.Reset(ScrollAnchorMode.Start);
			Assert.Null(view.Look());
			Assert.Equal(ScrollAnchorMode.End, view.Tracker.AnchoredEdge);

			view.Extent = 500;
			Assert.Equal(100, view.Look());
		}

		[Fact]
		public void BothEdgesFollowWhicheverEdgeTheUserScrollsTo()
		{
			var view = new FakeScrollView(extent: 1000, viewport: 400, edges: ScrollAnchorMode.Start | ScrollAnchorMode.End);
			view.Look();

			foreach (var offset in new[] { 450, 200, 4 })
			{
				view.Offset = offset;
				view.Look(userScrolling: true);
			}

			Assert.Equal(ScrollAnchorMode.Start, view.Tracker.AnchoredEdge);

			// At the start, items added before the first come into view, and items added after the last don't move it.
			Assert.Equal(0, view.Look());
			view.Extent += 80;
			view.Offset += 80;
			Assert.Equal(0, view.Look());
			view.Extent += 120;
			Assert.Null(view.Look());

			foreach (var offset in new[] { 300, 795 })
			{
				view.Offset = offset;
				view.Look(userScrolling: true);
			}

			Assert.Equal(ScrollAnchorMode.End, view.Tracker.AnchoredEdge);
			Assert.Equal(800, view.Look());
			view.Extent += 50;
			Assert.Equal(850, view.Look());
		}

		[Fact]
		public void BothEdgesFollowTheEndAfterAScrollFromTheStartToTheEndThatTheUserDidNotDrag()
		{
			var view = new FakeScrollView(extent: 1000, viewport: 400, edges: ScrollAnchorMode.Start | ScrollAnchorMode.End);
			view.Tracker.Reset(ScrollAnchorMode.Start);
			Assert.Null(view.Look());
			Assert.Equal(ScrollAnchorMode.Start, view.Tracker.AnchoredEdge);

			view.Offset = 600;
			Assert.Null(view.Look());
			Assert.Equal(ScrollAnchorMode.End, view.Tracker.AnchoredEdge);
		}

		[Fact]
		public void BothEdgesJudgeARequestedScrollByTheEdgeItLandsOn()
		{
			var view = new FakeScrollView(extent: 1000, viewport: 400, edges: ScrollAnchorMode.Start | ScrollAnchorMode.End);
			view.Look();

			view.Tracker.RequestScroll();
			view.Offset = 0;
			Assert.Null(view.Look());
			Assert.Equal(ScrollAnchorMode.Start, view.Tracker.AnchoredEdge);

			view.Tracker.RequestScroll();
			view.Offset = 300;
			Assert.Null(view.Look());
			Assert.Equal(ScrollAnchorMode.None, view.Tracker.AnchoredEdge);

			view.Tracker.RequestScroll();
			view.Offset = 600;
			Assert.Null(view.Look());
			Assert.Equal(ScrollAnchorMode.End, view.Tracker.AnchoredEdge);
		}

		[Fact]
		public void BothEdgesFollowTheStartForAnAnimationToItFromTheEnd()
		{
			// An animated ScrollToStart starts from the end: the view isn't at both edges, so it follows the start.
			var view = new FakeScrollView(extent: 1000, viewport: 400, edges: ScrollAnchorMode.Start | ScrollAnchorMode.End);
			view.Look();

			view.Tracker.Engage(ScrollAnchorMode.Start);
			Assert.Null(view.Look());
			Assert.Equal(ScrollAnchorMode.Start, view.Tracker.AnchoredEdge);

			foreach (var offset in new[] { 450, 200, 0 })
			{
				view.Offset = offset;
				Assert.Null(view.Look());
				Assert.Equal(ScrollAnchorMode.Start, view.Tracker.AnchoredEdge);
			}
		}

		[Theory]
		[InlineData(ScrollAnchorMode.Start)]
		[InlineData(ScrollAnchorMode.End)]
		public void AScrollThatBringsRowsOfAnotherSizeIntoViewStillMovesAway(ScrollAnchorMode edge)
		{
			// The platform measured the rows the scroll brought into view as it scrolled: the content changed size too.
			var view = new FakeScrollView(extent: 1000, viewport: 400, edges: edge);
			view.Look();

			view.Offset += edge == ScrollAnchorMode.Start ? 200 : -200;
			view.Extent += 20;
			Assert.Null(view.Look(scrolled: true));
			Assert.False(view.Tracker.IsAnchored);
		}

		[Fact]
		public void AScrollWithinTheToleranceThatBringsRowsOfAnotherSizeIntoViewKeepsFollowing()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);

			view.Offset -= 0.25;
			view.Extent += 50;
			Assert.Equal(650, view.Look(scrolled: true));
			Assert.True(view.Tracker.IsAnchored);
		}

		[Fact]
		public void ChangingTheEdgesStartsOver()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);

			view.Tracker.SetEdges(ScrollAnchorMode.Start);
			Assert.Equal(ScrollAnchorMode.Start, view.Tracker.AnchoredEdge);
			Assert.Equal(0, view.Look());

			view.Tracker.SetEdges(ScrollAnchorMode.Start | ScrollAnchorMode.End);
			Assert.Equal(ScrollAnchorMode.End, view.Tracker.AnchoredEdge);
			Assert.Equal(600, view.Look());

			view.Tracker.SetEdges(ScrollAnchorMode.None);
			Assert.False(view.Tracker.IsAnchored);
			view.Extent += 100;
			Assert.Null(view.Look());
		}

		[Fact]
		public void OnlyTheStartAndTheEndAreEdges()
		{
			var tracker = new ScrollAnchorTracker(ScrollAnchorMode.End | (ScrollAnchorMode)4);

			Assert.Equal(ScrollAnchorMode.End, tracker.Edges);
			Assert.True(tracker.CanFollow(ScrollAnchorMode.End));
			Assert.False(tracker.CanFollow(ScrollAnchorMode.Start));
			Assert.False(tracker.CanFollow(ScrollAnchorMode.None));
			Assert.False(tracker.CanFollow(ScrollAnchorMode.Start | ScrollAnchorMode.End));
		}

		[Fact]
		public void AnEdgeTheViewCannotFollowIsNotFollowed()
		{
			var view = FakeScrollView.AtEnd(extent: 1000, viewport: 400);

			view.Tracker.Engage(ScrollAnchorMode.Start);
			Assert.False(view.Tracker.IsAnchored);

			view.Tracker.Reset(ScrollAnchorMode.Start);
			Assert.False(view.Tracker.IsAnchored);
		}

		sealed class FakeScrollView
		{
			public FakeScrollView(
				double extent,
				double viewport,
				double threshold = ScrollAnchorTracker.DefaultThreshold,
				ScrollAnchorMode edges = ScrollAnchorMode.End)
			{
				Tracker = new ScrollAnchorTracker(edges, threshold);
				Extent = extent;
				Viewport = viewport;
			}

			public ScrollAnchorTracker Tracker { get; }

			public double Offset { get; set; }

			public double Extent { get; set; }

			public double Viewport { get; set; }

			public ScrollAnchorMetrics Metrics => new(Offset, Extent, Viewport);

			public static FakeScrollView AtEnd(double extent, double viewport, double threshold = ScrollAnchorTracker.DefaultThreshold)
			{
				var view = new FakeScrollView(extent, viewport, threshold);
				view.Look();
				Assert.Equal(0, view.Metrics.DistanceToEnd);
				return view;
			}

			/// <summary>A view that follows only its start, at its start.</summary>
			public static FakeScrollView AtStart(double extent, double viewport)
			{
				var view = new FakeScrollView(extent, viewport, edges: ScrollAnchorMode.Start);
				Assert.Null(view.Look());
				Assert.Equal(0, view.Offset);
				Assert.Equal(ScrollAnchorMode.Start, view.Tracker.AnchoredEdge);
				return view;
			}

			/// <summary>Reports the position; scrolls to the edge, as the platform does, when told to.</summary>
			public double? Look(bool userScrolling = false, bool sizeChanged = false, bool scrolled = false)
			{
				var target = Tracker.Update(Metrics, userScrolling, sizeChanged, scrolled);
				if (target is double edge)
				{
					Offset = edge;
				}

				return target;
			}
		}
	}
}
