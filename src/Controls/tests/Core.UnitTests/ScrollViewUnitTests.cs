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
		public async Task ElementRequestIsDroppedAndCompletedWhenTheViewLeavesTheTree()
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
			// released here rather than left pending forever
			parent.Children.Remove(scrollView);

			await task.WaitAsync(TimeSpan.FromSeconds(5));
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
		public async Task ScrollCompletionNeverResumesTheCallerInlineOnTheMutationStack()
		{
			var item = new View();
			var scrollView = new ScrollView { Content = new StackLayout { Children = { item } } };
			var parent = new StackLayout { Children = { scrollView } };
			scrollView.Handler = new ViewportProviderHandlerStub();

			// The task is completed from inside a lifecycle mutation (child removal). The
			// caller's continuation must not run inline on that mutation's stack, where it
			// could re-enter a half-finished parenting operation. A continuation that asks
			// to run synchronously would do exactly that if the completion source allowed
			// it, so it must instead land on a different thread than the one mutating.
			var mutatingThread = Environment.CurrentManagedThreadId;
			var continuationThread = -1;
			var task = scrollView.ScrollToAsync(item, ScrollToPosition.Start, false);
			var continuation = task.ContinueWith(
				_ => continuationThread = Environment.CurrentManagedThreadId,
				TaskContinuationOptions.ExecuteSynchronously);

			parent.Children.Remove(scrollView);

			// The task itself completes synchronously with the drop...
			Assert.True(task.IsCompleted);

			// ...but the continuation was pushed off the mutating thread's stack
			await continuation.WaitAsync(TimeSpan.FromSeconds(5));
			Assert.NotEqual(mutatingThread, continuationThread);
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
