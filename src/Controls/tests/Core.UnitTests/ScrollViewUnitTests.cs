using System;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;


	public class ScrollViewUnitTests : BaseTestFixture
	{
		[Fact]
		public void TestConstructor()
		{
			ScrollView scrollView = new ScrollView();

			Assert.Null(scrollView.Content);

			View view = new View();
			scrollView = new ScrollView { Content = view };

			Assert.Equal(view, scrollView.Content);
		}

		[Theory]
		[InlineData(ScrollOrientation.Horizontal)]
		[InlineData(ScrollOrientation.Both)]
		public void GetsCorrectSizeRequestWithWrappingContent(ScrollOrientation orientation)
		{
			var scrollView = new ScrollView
			{
				IsPlatformEnabled = true,
				Orientation = orientation,
			};

			var hLayout = new StackLayout
			{
				IsPlatformEnabled = true,
				Orientation = StackOrientation.Horizontal,
				Children = {
					MockPlatformSizeService.Sub<Label>(text: "THIS IS A REALLY LONG STRING", useRealisticLabelMeasure: true),
					MockPlatformSizeService.Sub<Label>(text: "THIS IS A REALLY LONG STRING", useRealisticLabelMeasure: true),
					MockPlatformSizeService.Sub<Label>(text: "THIS IS A REALLY LONG STRING", useRealisticLabelMeasure: true),
					MockPlatformSizeService.Sub<Label>(text: "THIS IS A REALLY LONG STRING", useRealisticLabelMeasure: true),
					MockPlatformSizeService.Sub<Label>(text: "THIS IS A REALLY LONG STRING", useRealisticLabelMeasure: true),
				}
			};

			scrollView.Content = hLayout;
			var view = ((ICrossPlatformLayout)scrollView);

			view.CrossPlatformMeasure(100, 100);
			var r = view.CrossPlatformArrange(new Graphics.Rect(0, 0, 100, 100));

			Assert.Equal(100, r.Height);
		}

		[Fact]
		public void TestChildChanged()
		{
			ScrollView scrollView = new ScrollView();

			bool changed = false;
			scrollView.PropertyChanged += (sender, e) =>
			{
				switch (e.PropertyName)
				{
					case "Content":
						changed = true;
						break;
				}
			};
			View view = new View();
			scrollView.Content = view;

			Assert.True(changed);
		}

		[Fact]
		public void TestChildDoubleSet()
		{
			var scrollView = new ScrollView();

			bool changed = false;
			scrollView.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "Content")
					changed = true;
			};

			var child = new View();
			scrollView.Content = child;

			Assert.True(changed);
			Assert.Equal(child, scrollView.Content);
			Assert.Equal(child.Parent, scrollView);

			changed = false;

			scrollView.Content = child;

			Assert.False(changed);

			scrollView.Content = null;

			Assert.True(changed);
			Assert.Null(scrollView.Content);
			Assert.Null(child.Parent);
		}

		[Fact]
		public void TestOrientation()
		{
			var scrollView = new ScrollView();

			Assert.Equal(ScrollOrientation.Vertical, scrollView.Orientation);

			bool signaled = false;
			scrollView.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "Orientation")
					signaled = true;
			};

			scrollView.Orientation = ScrollOrientation.Horizontal;

			Assert.Equal(ScrollOrientation.Horizontal, scrollView.Orientation);
			Assert.True(signaled);

			scrollView.Orientation = ScrollOrientation.Both;
			Assert.Equal(ScrollOrientation.Both, scrollView.Orientation);
			Assert.True(signaled);

			scrollView.Orientation = ScrollOrientation.Neither;
			Assert.Equal(ScrollOrientation.Neither, scrollView.Orientation);
			Assert.True(signaled);
		}

		[Fact]
		public void TestOrientationDoubleSet()
		{
			var scrollView = new ScrollView();

			bool signaled = false;
			scrollView.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "Orientation")
					signaled = true;
			};

			scrollView.Orientation = scrollView.Orientation;

			Assert.False(signaled);
		}


		[Fact]
		public void TestScrollTo()
		{
			var scrollView = new ScrollView();

			var item = new View { };
			scrollView.Content = new StackLayout { Children = { item } };

			bool requested = false;
			((IScrollViewController)scrollView).ScrollToRequested += (sender, args) =>
			{
				requested = true;
				Assert.Equal(100, args.ScrollY);
				Assert.Equal(0, args.ScrollX);
				Assert.Null(args.Item);
				Assert.True(args.ShouldAnimate);
			};

			scrollView.ScrollToAsync(0, 100, true);
			Assert.True(requested);
		}

		[Fact]
		public void TestScrollWasNotFiredOnNeither()
		{
			var scrollView = new ScrollView
			{
				Orientation = ScrollOrientation.Neither
			};

			var item = new View { };
			scrollView.Content = new StackLayout { Children = { item } };

			bool requested = false;
			((IScrollViewController)scrollView).ScrollToRequested += (sender, args) =>
			{
				requested = true;
			};

			scrollView.ScrollToAsync(0, 100, true);
			Assert.False(requested);
		}

		[Fact]
		public void TestScrollToNotAnimated()
		{
			var scrollView = new ScrollView();

			var item = new View { };
			scrollView.Content = new StackLayout { Children = { item } };

			bool requested = false;
			((IScrollViewController)scrollView).ScrollToRequested += (sender, args) =>
			{
				requested = true;
				Assert.Equal(100, args.ScrollY);
				Assert.Equal(0, args.ScrollX);
				Assert.Null(args.Item);
				Assert.False(args.ShouldAnimate);
			};

			scrollView.ScrollToAsync(0, 100, false);
			Assert.True(requested);
		}

		[Fact]
		public void TestScrollToElement()
		{
			var scrollView = new ScrollView();

			var item = new Label { Text = "Test" };
			scrollView.Content = new StackLayout { Children = { item } };

			bool requested = false;
			((IScrollViewController)scrollView).ScrollToRequested += (sender, args) =>
			{
				requested = true;

				Assert.Same(args.Element, item);
				Assert.Equal(ScrollToPosition.Center, args.Position);
				Assert.True(args.ShouldAnimate);
			};

			scrollView.ScrollToAsync(item, ScrollToPosition.Center, true);
			Assert.True(requested);
		}

		[Fact]
		public void TestScrollToElementNotAnimated()
		{
			var scrollView = new ScrollView();

			var item = new Label { Text = "Test" };
			scrollView.Content = new StackLayout { Children = { item } };

			bool requested = false;
			((IScrollViewController)scrollView).ScrollToRequested += (sender, args) =>
			{
				requested = true;

				Assert.Same(args.Element, item);
				Assert.Equal(ScrollToPosition.Center, args.Position);
				Assert.False(args.ShouldAnimate);
			};

			scrollView.ScrollToAsync(item, ScrollToPosition.Center, false);
			Assert.True(requested);
		}

		[Fact]
		public async Task TestScrollToInvalid()
		{
			var scrollView = new ScrollView();

			await Assert.ThrowsAsync<ArgumentException>(() => scrollView.ScrollToAsync(new VisualElement(), ScrollToPosition.Center, true));
			await Assert.ThrowsAsync<ArgumentException>(() => scrollView.ScrollToAsync(null, (ScrollToPosition)500, true));
		}

		[Fact]
		public void SetScrollPosition()
		{
			var scroll = new ScrollView();
			IScrollViewController controller = scroll;
			controller.SetScrolledPosition(100, 100);

			Assert.Equal(100, scroll.ScrollX);
			Assert.Equal(100, scroll.ScrollY);
		}

		[Fact]
		public void TestBackToBackBiDirectionalScroll()
		{
			var scrollView = new ScrollView
			{
				Orientation = ScrollOrientation.Both,
				Content = new Grid
				{
					WidthRequest = 1000,
					HeightRequest = 1000
				}
			};

			var y100Count = 0;

			((IScrollViewController)scrollView).ScrollToRequested += (sender, args) =>
			{
				if (args.ScrollY == 100)
				{
					++y100Count;
				}
			};

			scrollView.ScrollToAsync(100, 100, true);
			Assert.Equal(1, y100Count);

			scrollView.ScrollToAsync(0, 100, true);
			Assert.Equal(2, y100Count);
		}

		[Fact]
		public async Task DeferredElementScrollCompletesWhenTheHandlerGoesAway()
		{
			var item = new View();
			var scrollView = new ScrollView { Content = new StackLayout { Children = { item } } };

			// No handler yet, so the request is held. An element target also cannot be resolved
			// until layout has run, so it stays held even once a handler attaches.
			var task = scrollView.ScrollToAsync(item, ScrollToPosition.Center, false);
			Assert.False(task.IsCompleted);

			scrollView.Handler = Substitute.For<IViewHandler>();
			Assert.False(task.IsCompleted);

			// The handler goes away before layout ever happens, so nothing will dispatch the
			// request; the awaiting caller must still be released rather than hanging forever.
			scrollView.Handler = null;

			await task.WaitAsync(TimeSpan.FromSeconds(5));
			Assert.True(task.IsCompleted);
		}

		[Fact]
		public void DeferredElementScrollDispatchesOnceContentIsArranged()
		{
			var item = new View();
			var layout = new StackLayout { Children = { item } };
			var scrollView = new ScrollView { Content = layout };

			var task = scrollView.ScrollToAsync(item, ScrollToPosition.Start, false);

			var handler = new ViewportProviderHandlerStub();
			scrollView.Handler = handler;

			// No geometry yet, so the request must still be held
			Assert.Empty(handler.ScrollToRequests);

			item.Layout(new Graphics.Rect(0, 450, 100, 50));
			layout.Layout(new Graphics.Rect(0, 0, 100, 1000));
			scrollView.Layout(new Graphics.Rect(0, 0, 100, 100));

			// The element target resolves against the arranged position, not the zeros it had
			// when the request was queued
			var request = Assert.Single(handler.ScrollToRequests);
			Assert.Equal(450, request.VerticalOffset);

			// The replay must complete the task the original caller is still awaiting, not a
			// fresh completion source
			Assert.False(task.IsCompleted);
			scrollView.SendScrollFinished();
			Assert.True(task.IsCompleted);
		}

		[Fact]
		public void DirectRequestSupersedesDeferredElementRequest()
		{
			var item = new View();
			var layout = new StackLayout { Children = { item } };
			var scrollView = new ScrollView { Content = layout };

			_ = scrollView.ScrollToAsync(item, ScrollToPosition.Start, false);

			var handler = new ViewportProviderHandlerStub();
			scrollView.Handler = handler;

			// A direct request while the element request is still held must win
			var task = scrollView.ScrollToAsync(0, 100, false);
			var request = Assert.Single(handler.ScrollToRequests);
			Assert.Equal(100, request.VerticalOffset);

			// Layout completing later must not dispatch the stale element target on top of it
			item.Layout(new Graphics.Rect(0, 450, 100, 50));
			layout.Layout(new Graphics.Rect(0, 0, 100, 1000));
			scrollView.Layout(new Graphics.Rect(0, 0, 100, 100));

			Assert.Single(handler.ScrollToRequests);
			scrollView.SendScrollFinished();
			Assert.True(task.IsCompleted);
		}

		[Fact]
		public void DeferredElementScrollCompletesWhenContentArrangesToZero()
		{
			var item = new View();
			var layout = new StackLayout { Children = { item } };
			var scrollView = new ScrollView { Content = layout };

			var task = scrollView.ScrollToAsync(item, ScrollToPosition.Start, false);

			var handler = new ViewportProviderHandlerStub();
			scrollView.Handler = handler;

			// Everything arranges to zero (a collapsed container). "Arranged to nothing" is a
			// settled state: the request must dispatch and clamp instead of waiting for a
			// content size that will never become non-zero.
			item.Layout(new Graphics.Rect(0, 0, 0, 0));
			layout.Layout(new Graphics.Rect(0, 0, 0, 0));
			scrollView.Layout(new Graphics.Rect(0, 0, 0, 0));

			Assert.Single(handler.ScrollToRequests);
			scrollView.SendScrollFinished();
			Assert.True(task.IsCompleted);
		}

		[Fact]
		public void ElementTargetsAccountForViewportAndContentCoordinateInsets()
		{
			var item = new View();
			var layout = new StackLayout { Children = { item } };
			var scrollView = new ScrollView { Content = layout };

			// Total obscured insets shrink the viewport; the content-coordinate part is padding
			// the platform baked into the content, which element positions already include
			var handler = new ViewportProviderHandlerStub
			{
				ViewportInsets = new Thickness(0, 70, 0, 50),
				ContentCoordinateInsets = new Thickness(0, 10, 0, 10),
			};
			scrollView.Handler = handler;

			item.Layout(new Graphics.Rect(0, 450, 100, 50));
			layout.Layout(new Graphics.Rect(0, 0, 100, 1000));
			scrollView.Layout(new Graphics.Rect(0, 0, 100, 300));

			// Start aligns the element with the visible viewport top: the 10 of baked padding in
			// its coordinates must be shifted back out
			Assert.Equal(440, scrollView.GetScrollPositionForElement(item, ScrollToPosition.Start).Y);

			// End: 450 - (300 - 70 - 50) + 50 = 320, shifted by the baked 10
			Assert.Equal(310, scrollView.GetScrollPositionForElement(item, ScrollToPosition.End).Y);

			// Center: 450 - 180 / 2 + 25 = 385, shifted by the baked 10
			Assert.Equal(375, scrollView.GetScrollPositionForElement(item, ScrollToPosition.Center).Y);

			// The element is below the visible window, so MakeVisible resolves to End
			Assert.Equal(310, scrollView.GetScrollPositionForElement(item, ScrollToPosition.MakeVisible).Y);
		}

		[Fact]
		public void ElementRequestWithHandlerAttachedWaitsForArrange()
		{
			var item = new View();
			var layout = new StackLayout { Children = { item } };
			var scrollView = new ScrollView { Content = layout };

			var handler = new ViewportProviderHandlerStub();
			scrollView.Handler = handler;

			var eventCount = 0;
			((IScrollViewController)scrollView).ScrollToRequested += (_, _) => eventCount++;

			// The handler exists but nothing is arranged yet (Width/Height are the -1
			// never-arranged sentinels): resolving the element target now would compute
			// against garbage geometry, so the request must wait for layout
			var task = scrollView.ScrollToAsync(item, ScrollToPosition.Center, false);
			Assert.Empty(handler.ScrollToRequests);
			Assert.Equal(1, eventCount);

			item.Layout(new Graphics.Rect(0, 450, 100, 50));
			layout.Layout(new Graphics.Rect(0, 0, 100, 1000));
			scrollView.Layout(new Graphics.Rect(0, 0, 100, 100));

			// Dispatched once with a target computed from real geometry: 450 - 100/2 + 50/2.
			// The public event must not be raised a second time on the replay — subscribers
			// were already notified when the request was made.
			var request = Assert.Single(handler.ScrollToRequests);
			Assert.Equal(425, request.VerticalOffset);
			Assert.Equal(1, eventCount);

			scrollView.SendScrollFinished();
			Assert.True(task.IsCompleted);
		}

		[Fact]
		public void ElementRequestOnHiddenScrollViewWaitsAndScrollsOnceShown()
		{
			var item = new View();
			var layout = new StackLayout { Children = { item } };
			var scrollView = new ScrollView { Content = layout, IsVisible = false };

			var handler = new ViewportProviderHandlerStub();
			scrollView.Handler = handler;

			// A hidden view is skipped by layout, so no arrange is coming yet. The request
			// stays parked and the task stays pending: completing it here would either
			// scroll to a target computed from unarranged geometry or leave a request alive
			// after its await returned — the task means "the scroll happened or the view is
			// gone", nothing in between.
			var task = scrollView.ScrollToAsync(item, ScrollToPosition.Center, false);
			Assert.False(task.IsCompleted);
			Assert.Empty(handler.ScrollToRequests);

			// Showing the view arranges it; the first arrange replays the request against
			// real geometry so the scroll lands on the element: Center = 450 - 100/2 + 50/2
			scrollView.IsVisible = true;
			item.Layout(new Graphics.Rect(0, 450, 100, 50));
			layout.Layout(new Graphics.Rect(0, 0, 100, 1000));
			scrollView.Layout(new Graphics.Rect(0, 0, 100, 100));

			var request = Assert.Single(handler.ScrollToRequests);
			Assert.Equal(425, request.VerticalOffset);

			scrollView.SendScrollFinished();
			Assert.True(task.IsCompleted);
		}

		[Fact]
		public void ElementRequestIsDroppedAndCompletedWhenTheViewLeavesTheTree()
		{
			var item = new View();
			var layout = new StackLayout { Children = { item } };
			var scrollView = new ScrollView { Content = layout };
			var parent = new StackLayout { Children = { scrollView } };

			var handler = new ViewportProviderHandlerStub();
			scrollView.Handler = handler;

			// Parked with the handler attached, waiting for the arrange
			var task = scrollView.ScrollToAsync(item, ScrollToPosition.End, false);
			Assert.False(task.IsCompleted);

			// Removing the view from the tree ends its lifecycle for layout purposes: no
			// arrange will ever come from a parent it no longer has, and the removal does
			// not disconnect its handler — so the request must be dropped and the caller
			// released here, at the moment of removal, rather than left pending forever
			parent.Children.Remove(scrollView);
			Assert.True(task.IsCompleted);
			Assert.Empty(handler.ScrollToRequests);

			// And dropped means dropped: re-attaching and arranging later must not resurrect
			// the stale request into a scroll the caller no longer expects
			parent.Children.Add(scrollView);
			item.Layout(new Graphics.Rect(0, 450, 100, 50));
			layout.Layout(new Graphics.Rect(0, 0, 100, 1000));
			scrollView.Layout(new Graphics.Rect(0, 0, 100, 100));
			Assert.Empty(handler.ScrollToRequests);
		}

		[Fact]
		public void DropCompletesTheDroppedTaskAndOnlyThatTask()
		{
			var item = new View();
			var layout = new StackLayout { Children = { item } };
			var scrollView = new ScrollView { Content = layout };
			var parent = new StackLayout { Children = { scrollView } };
			scrollView.Handler = new ViewportProviderHandlerStub();

			// T1 parks (handler attached, geometry not ready)
			var task1 = scrollView.ScrollToAsync(item, ScrollToPosition.Start, false);
			Assert.False(task1.IsCompleted);

			// The view leaves the tree: T1's request is dropped and T1 completes at that
			// moment — bound to the request being dropped, with no window in which anything
			// else could be released in its place
			parent.Children.Remove(scrollView);
			Assert.True(task1.IsCompleted);

			// Re-attached, a new request T2 parks. It is a different request with its own
			// task: nothing about the earlier drop may touch it
			parent.Children.Add(scrollView);
			var task2 = scrollView.ScrollToAsync(item, ScrollToPosition.End, false);
			Assert.False(task2.IsCompleted);

			// T2 stays pending until its own scroll: the arrange replays it and lands on the
			// element (End = 450 - 100 + 50), and only then does its task complete
			item.Layout(new Graphics.Rect(0, 450, 100, 50));
			layout.Layout(new Graphics.Rect(0, 0, 100, 1000));
			scrollView.Layout(new Graphics.Rect(0, 0, 100, 100));

			var handler = (ViewportProviderHandlerStub)scrollView.Handler;
			var request = Assert.Single(handler.ScrollToRequests);
			Assert.Equal(400, request.VerticalOffset);
			Assert.False(task2.IsCompleted);

			scrollView.SendScrollFinished();
			Assert.True(task2.IsCompleted);
		}

		[Fact]
		public void ConsecutiveDropsEachCompleteTheirOwnTask()
		{
			var item = new View();
			var scrollView = new ScrollView { Content = new StackLayout { Children = { item } } };
			var parent = new StackLayout { Children = { scrollView } };
			scrollView.Handler = new ViewportProviderHandlerStub();

			var task1 = scrollView.ScrollToAsync(item, ScrollToPosition.Start, false);
			parent.Children.Remove(scrollView);
			Assert.True(task1.IsCompleted);

			// A second park-and-drop cycle on the same view: the second drop must complete
			// the second task, and the first drop must have had no effect on it
			parent.Children.Add(scrollView);
			var task2 = scrollView.ScrollToAsync(item, ScrollToPosition.Start, false);
			Assert.False(task2.IsCompleted);

			parent.Children.Remove(scrollView);
			Assert.True(task2.IsCompleted);
		}

		[Fact]
		public void NewerRequestSupersedesAGeometryParkedElementRequest()
		{
			var item = new View();
			var layout = new StackLayout { Children = { item } };
			var scrollView = new ScrollView { Content = layout };
			var handler = new ViewportProviderHandlerStub();
			scrollView.Handler = handler;

			// T1 parks for geometry with the handler attached
			var task1 = scrollView.ScrollToAsync(item, ScrollToPosition.Start, false);
			Assert.Empty(handler.ScrollToRequests);

			// A direct request while T1 is parked wins: it is sent now, and T1's request is
			// cleared so the arrange cannot replay the stale target on top of it
			var task2 = scrollView.ScrollToAsync(0, 100, false);
			var direct = Assert.Single(handler.ScrollToRequests);
			Assert.Equal(100, direct.VerticalOffset);

			item.Layout(new Graphics.Rect(0, 450, 100, 50));
			layout.Layout(new Graphics.Rect(0, 0, 100, 1000));
			scrollView.Layout(new Graphics.Rect(0, 0, 100, 100));
			Assert.Single(handler.ScrollToRequests);

			// The scroll completes the current (latest) request's task
			scrollView.SendScrollFinished();
			Assert.True(task2.IsCompleted);
		}

		[Fact]
		public void HandlerDetachDropCompletesOnlyTheDroppedTaskAndDoesNotResurrect()
		{
			var item = new View();
			var layout = new StackLayout { Children = { item } };
			var scrollView = new ScrollView { Content = layout };
			scrollView.Handler = new ViewportProviderHandlerStub();

			var task1 = scrollView.ScrollToAsync(item, ScrollToPosition.Start, false);
			Assert.False(task1.IsCompleted);

			// Handler detach is the other lifecycle end: T1 completes at that moment
			scrollView.Handler = null;
			Assert.True(task1.IsCompleted);

			// A new handler and a new request: T2 is its own request, unaffected by the drop
			var handler = new ViewportProviderHandlerStub();
			scrollView.Handler = handler;
			var task2 = scrollView.ScrollToAsync(item, ScrollToPosition.End, false);
			Assert.False(task2.IsCompleted);
			Assert.Empty(handler.ScrollToRequests);

			// The arrange replays exactly one request — T2's — and the dropped T1 request is
			// not resurrected alongside it
			item.Layout(new Graphics.Rect(0, 450, 100, 50));
			layout.Layout(new Graphics.Rect(0, 0, 100, 1000));
			scrollView.Layout(new Graphics.Rect(0, 0, 100, 100));
			var request = Assert.Single(handler.ScrollToRequests);
			Assert.Equal(400, request.VerticalOffset);
			Assert.False(task2.IsCompleted);

			scrollView.SendScrollFinished();
			Assert.True(task2.IsCompleted);
		}

		[Fact]
		public void DoubleLifecycleEndOnOneRequestCompletesItOnceAndLeavesLaterRequestsAlone()
		{
			var item = new View();
			var scrollView = new ScrollView { Content = new StackLayout { Children = { item } } };
			var parent = new StackLayout { Children = { scrollView } };
			scrollView.Handler = new ViewportProviderHandlerStub();

			var task1 = scrollView.ScrollToAsync(item, ScrollToPosition.Start, false);

			// Both lifecycle ends fire for the same parked request: the first drops and
			// completes it, the second must be a no-op (nothing left to drop)
			parent.Children.Remove(scrollView);
			Assert.True(task1.IsCompleted);
			scrollView.Handler = null;

			// A later request on the revived view is untouched by either earlier end
			parent.Children.Add(scrollView);
			scrollView.Handler = new ViewportProviderHandlerStub();
			var task2 = scrollView.ScrollToAsync(item, ScrollToPosition.Start, false);
			Assert.False(task2.IsCompleted);
		}

		[Fact]
		public void PreHandlerParkedRequestIsDroppedWhenTheViewLeavesTheTree()
		{
			var item = new View();
			var scrollView = new ScrollView { Content = new StackLayout { Children = { item } } };
			var parent = new StackLayout { Children = { scrollView } };

			// Parked because there is no handler yet
			var task = scrollView.ScrollToAsync(item, ScrollToPosition.Start, false);
			Assert.False(task.IsCompleted);

			// Leaving the tree is a lifecycle end whether or not a handler ever attached
			parent.Children.Remove(scrollView);
			Assert.True(task.IsCompleted);

			// And a handler attaching afterwards must find nothing to replay
			var handler = new ViewportProviderHandlerStub();
			scrollView.Handler = handler;
			Assert.Empty(handler.ScrollToRequests);
		}

		[Fact]
		public void PreHandlerParkedOffsetRequestIsDroppedWhenTheViewLeavesTheTree()
		{
			var scrollView = new ScrollView { Content = new StackLayout() };
			var parent = new StackLayout { Children = { scrollView } };

			// The contract is about parked requests of any mode, not only element mode
			var task = scrollView.ScrollToAsync(0, 100, false);
			Assert.False(task.IsCompleted);

			parent.Children.Remove(scrollView);
			Assert.True(task.IsCompleted);

			var handler = new ViewportProviderHandlerStub();
			scrollView.Handler = handler;
			Assert.Empty(handler.ScrollToRequests);
		}

		[Fact]
		public void ParkedElementRequestSurvivesContentReplacementAndDrainsOnTheNewContent()
		{
			var item = new View();
			var scrollView = new ScrollView { Content = new StackLayout { Children = { item } } };
			var handler = new ViewportProviderHandlerStub();
			scrollView.Handler = handler;

			var task = scrollView.ScrollToAsync(item, ScrollToPosition.Start, false);
			Assert.False(task.IsCompleted);

			// The content is swapped while the request is parked. The retry is hooked to
			// the content's SizeChanged, so it must be re-hooked to the new content — the
			// element still belongs to the tree via the old layout, but the ScrollView's
			// geometry callbacks now come from the new one
			var newLayout = new StackLayout { Children = { item } };
			scrollView.Content = newLayout;
			Assert.False(task.IsCompleted);
			Assert.Empty(handler.ScrollToRequests);

			item.Layout(new Graphics.Rect(0, 450, 100, 50));
			newLayout.Layout(new Graphics.Rect(0, 0, 100, 1000));
			scrollView.Layout(new Graphics.Rect(0, 0, 100, 100));

			var request = Assert.Single(handler.ScrollToRequests);
			Assert.Equal(450, request.VerticalOffset);
		}

		[Fact]
		public void ParkedElementRequestWhoseTargetIsOrphanedByContentRemovalIsDroppedAndCompleted()
		{
			var item = new View();
			var scrollView = new ScrollView { Content = new StackLayout { Children = { item } } };
			var handler = new ViewportProviderHandlerStub();
			scrollView.Handler = handler;

			var task = scrollView.ScrollToAsync(item, ScrollToPosition.Start, false);
			Assert.False(task.IsCompleted);

			// Removing the content orphans the target: it no longer hangs off this ScrollView,
			// so no arrange of this ScrollView can ever give it a position. There is nothing
			// left to wait for — the request is dropped and the caller released at that
			// moment (not at some later arrange that may never come), and no target computed
			// against nothing is dispatched
			scrollView.Content = null;
			Assert.True(task.IsCompleted);
			Assert.Empty(handler.ScrollToRequests);

			// A later arrange has nothing to replay
			scrollView.Layout(new Graphics.Rect(0, 0, 100, 100));
			Assert.Empty(handler.ScrollToRequests);
		}

		[Fact]
		public void ParkedElementRequestWhoseTargetIsOrphanedByContentReplacementIsDroppedAndCompleted()
		{
			var item = new View();
			var scrollView = new ScrollView { Content = new StackLayout { Children = { item } } };
			var handler = new ViewportProviderHandlerStub();
			scrollView.Handler = handler;

			var task = scrollView.ScrollToAsync(item, ScrollToPosition.Start, false);
			Assert.False(task.IsCompleted);

			// Replacing the content with a layout that does not contain the target orphans it
			// just as removal does: dropped and completed at the replacement. (Replacement
			// with content that still contains the target instead keeps waiting for that
			// content's arrange — see ParkedElementRequestSurvivesContentReplacement...)
			var unrelated = new StackLayout();
			scrollView.Content = unrelated;
			Assert.True(task.IsCompleted);
			Assert.Empty(handler.ScrollToRequests);

			unrelated.Layout(new Graphics.Rect(0, 0, 100, 1000));
			scrollView.Layout(new Graphics.Rect(0, 0, 100, 100));
			Assert.Empty(handler.ScrollToRequests);
		}

		[Fact]
		public void SelfTargetOnAContentlessScrollViewNeedsOnlyItsOwnGeometry()
		{
			// ScrollToAsync(scrollView, ...) is a valid request that resolves to the origin
			// without touching Content, so a ScrollView with no content must not park it
			// forever waiting for content that may never come
			var scrollView = new ScrollView();
			var handler = new ViewportProviderHandlerStub();
			scrollView.Handler = handler;

			var task = scrollView.ScrollToAsync(scrollView, ScrollToPosition.Start, false);

			// Waits only for the ScrollView's own arrange...
			Assert.False(task.IsCompleted);
			Assert.Empty(handler.ScrollToRequests);

			// ...then dispatches against its own geometry
			scrollView.Layout(new Graphics.Rect(0, 0, 100, 100));
			var request = Assert.Single(handler.ScrollToRequests);
			Assert.Equal(0, request.VerticalOffset);

			scrollView.SendScrollFinished();
			Assert.True(task.IsCompleted);
		}

		[Fact]
		public void ReparentingCancelsAPendingElementScroll()
		{
			var item = new View();
			var layout = new StackLayout { Children = { item } };
			var scrollView = new ScrollView { Content = layout };
			var parent1 = new StackLayout { Children = { scrollView } };
			var parent2 = new StackLayout();
			var handler = new ViewportProviderHandlerStub();
			scrollView.Handler = handler;

			var task = scrollView.ScrollToAsync(item, ScrollToPosition.Start, false);
			Assert.False(task.IsCompleted);

			// Moving the ScrollView passes through a removal. A request made against a tree
			// position that no longer exists is cancelled at that removal: the task completes
			// (without a scroll) and the request is not carried into the new parent — a
			// caller that moves a ScrollView with a pending scroll re-requests it there
			parent1.Children.Remove(scrollView);
			Assert.True(task.IsCompleted);
			parent2.Children.Add(scrollView);

			item.Layout(new Graphics.Rect(0, 450, 100, 50));
			layout.Layout(new Graphics.Rect(0, 0, 100, 1000));
			scrollView.Layout(new Graphics.Rect(0, 0, 100, 100));
			Assert.Empty(handler.ScrollToRequests);
		}

		[Fact]
		public void ReentrantRequestFromTheDropContinuationParksCleanly()
		{
			var item = new View();
			var layout = new StackLayout { Children = { item } };
			var scrollView = new ScrollView { Content = layout };
			var parent = new StackLayout { Children = { scrollView } };
			var handler = new ViewportProviderHandlerStub();
			scrollView.Handler = handler;

			// The caller re-requests from its own continuation, which the inline drop runs
			// synchronously in the middle of the removal. The mechanism must already have
			// cleared its state by then, so the re-entrant request parks as a fresh T2
			// instead of being dropped by the same removal, double-completed, or lost.
			Task task2 = null;
			var task1 = scrollView.ScrollToAsync(item, ScrollToPosition.Start, false);
			task1.ContinueWith(_ => task2 = scrollView.ScrollToAsync(item, ScrollToPosition.End, false),
				TaskContinuationOptions.ExecuteSynchronously);

			parent.Children.Remove(scrollView);
			Assert.True(task1.IsCompleted);
			Assert.NotNull(task2);
			Assert.False(task2.IsCompleted);
			Assert.Empty(handler.ScrollToRequests);

			// T2 belongs to the new lifecycle: re-attached and arranged, it scrolls
			parent.Children.Add(scrollView);
			item.Layout(new Graphics.Rect(0, 450, 100, 50));
			layout.Layout(new Graphics.Rect(0, 0, 100, 1000));
			scrollView.Layout(new Graphics.Rect(0, 0, 100, 100));
			var request = Assert.Single(handler.ScrollToRequests);
			Assert.Equal(400, request.VerticalOffset);
		}

		[Fact]
		public void ReentrantRequestFromTheReplayedEventSupersedesTheReplay()
		{
			var item = new View();
			var layout = new StackLayout { Children = { item } };
			var scrollView = new ScrollView { Content = layout };

			// Parked before the handler exists, so the attach replays ScrollToRequested
			_ = scrollView.ScrollToAsync(0, 100, false);

			// A subscriber (as a compatibility renderer would be) that issues a new request
			// from inside the replayed event. The replay has already cleared the pending
			// request before raising, so the re-entrant request is not clobbered — and it
			// must win: the replay must not send the stale request to the handler on top of it.
			Task reentrant = null;
			var reentered = false;
			((IScrollViewController)scrollView).ScrollToRequested += (_, _) =>
			{
				if (reentered) return;
				reentered = true;
				reentrant = scrollView.ScrollToAsync(0, 250, false);
			};

			var handler = new ViewportProviderHandlerStub();
			scrollView.Handler = handler;

			Assert.NotNull(reentrant);
			// Latest request wins; exactly what was sent, in order: the re-entrant one first
			// (sent immediately, handler present) — the stale replay must not follow it
			Assert.Equal(new[] { 250d }, handler.ScrollToRequests.ConvertAll(r => r.VerticalOffset));
		}

		[Fact]
		public void DeferredRequestReplaysEventForSubscribersAttachedWithTheHandler()
		{
			var scrollView = new ScrollView();
			var task = scrollView.ScrollToAsync(10, 20, false);

			// Compatibility renderers subscribe to ScrollToRequested when they attach —
			// after the request above was parked — and perform the scroll from the event,
			// so the replay must re-raise it for them
			var eventCount = 0;
			((IScrollViewController)scrollView).ScrollToRequested += (_, _) => eventCount++;

			var handler = new ViewportProviderHandlerStub();
			scrollView.Handler = handler;

			Assert.Equal(1, eventCount);
			var request = Assert.Single(handler.ScrollToRequests);
			Assert.Equal(20, request.VerticalOffset);

			scrollView.SendScrollFinished();
			Assert.True(task.IsCompleted);
		}

		[Fact]
		public void InsetRefreshUpdatesOffsetsWithoutRaisingScrolled()
		{
			var scrollView = new ScrollView();
			var scrolledCount = 0;
			scrollView.Scrolled += (_, _) => scrolledCount++;

			// An inset-only change moves the derived offsets without any scroll: the values
			// (and their bindings) must refresh, but no Scrolled event may be manufactured
			((IScrollOffsetReceiver)scrollView).UpdateScrollOffsets(5, 40);

			Assert.Equal(5, scrollView.ScrollX);
			Assert.Equal(40, scrollView.ScrollY);
			Assert.Equal(0, scrolledCount);

			// An actual scroll notification still raises Scrolled
			((IScrollView)scrollView).VerticalOffset = 60;

			Assert.Equal(60, scrollView.ScrollY);
			Assert.Equal(1, scrolledCount);
		}

		// IScrollViewportProvider is internal to Core, which NSubstitute cannot proxy, so the
		// viewport contract is stubbed by hand here.
		class ViewportProviderHandlerStub : IViewHandler, Microsoft.Maui.Handlers.IScrollViewportProvider
		{
			public System.Collections.Generic.List<ScrollToRequest> ScrollToRequests { get; } = new();

			public Thickness ViewportInsets { get; set; }
			public Thickness ContentCoordinateInsets { get; set; }
			public void NotifyInsetsChanged() { }

			public bool HasContainer { get; set; }
			public object ContainerView => null;
			public IView VirtualView { get; private set; }
			IElement IElementHandler.VirtualView => VirtualView;
			public object PlatformView => null;
			public IMauiContext MauiContext => null;

			public Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint) => Graphics.Size.Zero;
			public void PlatformArrange(Graphics.Rect frame) { }
			public void SetMauiContext(IMauiContext mauiContext) { }
			public void SetVirtualView(IElement view) => VirtualView = (IView)view;
			public void UpdateValue(string property) { }
			public void DisconnectHandler() { }

			public void Invoke(string command, object args = null)
			{
				if (command == nameof(IScrollView.RequestScrollTo) && args is ScrollToRequest request)
					ScrollToRequests.Add(request);
			}
		}

		void AssertInvalidated(IViewHandler handler)
		{
			handler.Received().Invoke(Arg.Is(nameof(IView.InvalidateMeasure)), Arg.Any<object>());
			handler.ClearReceivedCalls();
		}
	}
}
