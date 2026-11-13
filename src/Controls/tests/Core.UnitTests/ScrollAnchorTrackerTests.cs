using System;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	/// <summary>
	/// Drives <see cref="ScrollAnchorTracker"/> the way a platform does: report where the view is after each change, and
	/// scroll to the end without animating when it says so.
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
			view.Tracker.Engage();
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

			view.Tracker.Engage();
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
			view.Tracker.Engage();
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
			Assert.Throws<ArgumentOutOfRangeException>(() => new ScrollAnchorTracker(threshold));

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

		sealed class FakeScrollView
		{
			public FakeScrollView(double extent, double viewport, double threshold = ScrollAnchorTracker.DefaultThreshold)
			{
				Tracker = new ScrollAnchorTracker(threshold);
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

			/// <summary>Reports the position; scrolls to the end, as the platform does, when told to.</summary>
			public double? Look(bool userScrolling = false, bool sizeChanged = false)
			{
				var target = Tracker.Update(Metrics, userScrolling, sizeChanged);
				if (target is double end)
				{
					Offset = end;
				}

				return target;
			}
		}
	}
}
