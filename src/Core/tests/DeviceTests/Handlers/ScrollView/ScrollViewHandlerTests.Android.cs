using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.Widget;
using Google.Android.Material.AppBar;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using static Android.Views.View;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ScrollViewHandlerTests : CoreHandlerTestBase<ScrollViewHandler, ScrollViewStub>
	{
		// Regression test for https://github.com/dotnet/maui/issues/35180
		// On Material3, the AppBarLayout was auto-detecting the scroll target as the outer
		// FragmentContainerView, causing a flicker on every layout pass triggered by CheckBox /
		// Switch animations after scrolling.  The fix pins LiftOnScrollTargetViewId to the
		// real MauiScrollView so AppBarLayout correctly evaluates the scroll position.
		[Fact]
		[Category(TestCategory.ScrollView)]
		public async Task AppBarLiftTargetSetToScrollViewOnAttach()
		{
			if (!Microsoft.Maui.RuntimeFeature.IsMaterial3Enabled)
				return;

			await InvokeOnMainThreadAsync(async () =>
			{
				var context = MauiContext.Context!;

				// Replicate the NavigationPage CoordinatorLayout structure:
				//   CoordinatorLayout
				//     ├─ AppBarLayout  (sibling — the lift-on-scroll host)
				//     └─ FrameLayout   (content container)
				//          └─ MauiScrollView
				var coordinator = new CoordinatorLayout(context);
				var appBarLayout = new AppBarLayout(context);
				appBarLayout.SetLiftable(true);

				var contentFrame = new FrameLayout(context);
				var scrollView = new Microsoft.Maui.Platform.MauiScrollView(context);

				contentFrame.AddView(scrollView, new ViewGroup.LayoutParams(
					ViewGroup.LayoutParams.MatchParent,
					ViewGroup.LayoutParams.MatchParent));

				coordinator.AddView(appBarLayout, new CoordinatorLayout.LayoutParams(
					ViewGroup.LayoutParams.MatchParent,
					ViewGroup.LayoutParams.WrapContent));

				coordinator.AddView(contentFrame, new CoordinatorLayout.LayoutParams(
					ViewGroup.LayoutParams.MatchParent,
					ViewGroup.LayoutParams.MatchParent));

				// Attach the whole tree to the window so that OnAttachedToWindow fires.
				await coordinator.AttachAndRun(async () =>
				{
					// Post() schedules the lift-target assignment on the next looper tick.
					// Await a task continuation that runs after that tick has been processed.
					var tcs = new TaskCompletionSource<bool>();
					scrollView.Post(new Java.Lang.Runnable(() => tcs.SetResult(true)));
					await tcs.Task;

					Assert.Equal(scrollView.Id, appBarLayout.LiftOnScrollTargetViewId);
				});

				// After detach the lift target must be released to avoid stale references.
				Assert.NotEqual(scrollView.Id, appBarLayout.LiftOnScrollTargetViewId);
			});
		}

		[Fact]
		[Category(TestCategory.ScrollView)]
		public async Task AppBarLiftTargetClearedOnVisibilityGone()
		{
			if (!Microsoft.Maui.RuntimeFeature.IsMaterial3Enabled)
				return;

			await InvokeOnMainThreadAsync(async () =>
			{
				var context = MauiContext.Context!;

				var coordinator = new CoordinatorLayout(context);
				var appBarLayout = new AppBarLayout(context);
				appBarLayout.SetLiftable(true);

				var contentFrame = new FrameLayout(context);
				var scrollView = new Microsoft.Maui.Platform.MauiScrollView(context);

				contentFrame.AddView(scrollView, new ViewGroup.LayoutParams(
					ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

				coordinator.AddView(appBarLayout, new CoordinatorLayout.LayoutParams(
					ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

				coordinator.AddView(contentFrame, new CoordinatorLayout.LayoutParams(
					ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

				await coordinator.AttachAndRun(async () =>
				{
					// Wait for the initial Post() to settle before checking.
					var tcs = new TaskCompletionSource<bool>();
					scrollView.Post(new Java.Lang.Runnable(() => tcs.SetResult(true)));
					await tcs.Task;

					Assert.Equal(scrollView.Id, appBarLayout.LiftOnScrollTargetViewId);

					// Hiding the scroll view should synchronously clear the lift target.
					scrollView.Visibility = ViewStates.Gone;
					Assert.NotEqual(scrollView.Id, appBarLayout.LiftOnScrollTargetViewId);

					// Restoring visibility should re-establish the lift target.
					scrollView.Visibility = ViewStates.Visible;
					var tcs2 = new TaskCompletionSource<bool>();
					scrollView.Post(new Java.Lang.Runnable(() => tcs2.SetResult(true)));
					await tcs2.Task;

					Assert.Equal(scrollView.Id, appBarLayout.LiftOnScrollTargetViewId);
				});
			});
		}

		[Fact]
		[Category(TestCategory.ScrollView)]
		public async Task AppBarLiftTargetNotSetWhenNoAppBarLayout()
		{
			if (!Microsoft.Maui.RuntimeFeature.IsMaterial3Enabled)
				return;

			await InvokeOnMainThreadAsync(async () =>
			{
				var context = MauiContext.Context!;

				// Plain FrameLayout with no CoordinatorLayout / AppBarLayout ancestor.
				var frame = new FrameLayout(context);
				var scrollView = new Microsoft.Maui.Platform.MauiScrollView(context);

				frame.AddView(scrollView, new ViewGroup.LayoutParams(
					ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

				await frame.AttachAndRun(async () =>
				{
					// Allow the Post()-deferred work to settle.
					var tcs = new TaskCompletionSource<bool>();
					scrollView.Post(new Java.Lang.Runnable(() => tcs.SetResult(true)));
					await tcs.Task;

					// Without an AppBarLayout in the hierarchy, no view ID should be generated
					// (SetAppBarLiftTarget only assigns an ID when it actually claims the target).
					Assert.Equal(View.NoId, scrollView.Id);
				});
			});
		}

		[Fact]
		public async Task ContentInitializesCorrectly()
		{
			bool result = await InvokeOnMainThreadAsync(() =>
			{

				var entry = new EntryStub() { Text = "In a ScrollView" };
				var entryHandler = Activator.CreateInstance<EntryHandler>();
				entryHandler.SetMauiContext(MauiContext);
				entryHandler.SetVirtualView(entry);
				entry.Handler = entryHandler;

				var scrollView = new ScrollViewStub()
				{
					Content = entry
				};

				var scrollViewHandler = CreateHandler(scrollView);

				for (int n = 0; n < scrollViewHandler.PlatformView.ChildCount; n++)
				{
					var platformView = scrollViewHandler.PlatformView.GetChildAt(n);

					// ScrollView on Android uses an intermediate ContentViewGroup to handle measurement/arrangement/padding
					if (platformView is ContentViewGroup contentViewGroup)
					{
						for (int i = 0; i < contentViewGroup.ChildCount; i++)
						{
							if (contentViewGroup.GetChildAt(i) is AppCompatEditText)
							{
								return true;
							}
						}
					}
				}

				return false; // No AppCompatEditText
			});

			Assert.True(result, $"Expected (but did not find) a {nameof(AppCompatEditText)} child of the {nameof(NestedScrollView)}.");
		}

		[Theory]
		[InlineData(ScrollBarVisibility.Always, true)]
		[InlineData(ScrollBarVisibility.Default, true)]
		[InlineData(ScrollBarVisibility.Never, false)]
		public async Task HorizontalVisibilityInitializesCorrectly(ScrollBarVisibility visibility, bool expected)
		{
			bool result = await InvokeOnMainThreadAsync(() =>
			{
				var scrollView = new ScrollViewStub()
				{
					Orientation = ScrollOrientation.Horizontal,
					HorizontalScrollBarVisibility = visibility
				};

				var scrollViewHandler = CreateHandler(scrollView);


				return ((MauiHorizontalScrollView)scrollViewHandler.PlatformView.GetChildAt(0)).HorizontalScrollBarEnabled;
			});

			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData(ScrollBarVisibility.Always, true)]
		[InlineData(ScrollBarVisibility.Default, true)]
		[InlineData(ScrollBarVisibility.Never, false)]
		public async Task VerticalVisibilityInitializesCorrectly(ScrollBarVisibility visibility, bool expected)
		{
			bool result = await InvokeOnMainThreadAsync(() =>
			{
				var scrollView = new ScrollViewStub()
				{
					Orientation = ScrollOrientation.Vertical,
					VerticalScrollBarVisibility = visibility
				};

				var scrollViewHandler = CreateHandler(scrollView);


				return ((MauiScrollView)scrollViewHandler.PlatformView).VerticalScrollBarEnabled;
			});

			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData(ScrollBarVisibility.Always, false)]
		[InlineData(ScrollBarVisibility.Default, true)]
		[InlineData(ScrollBarVisibility.Never, true)]
		public async Task VerticalScrollbarFadingInitializesCorrectly(ScrollBarVisibility visibility, bool expected)
		{
			bool result = await InvokeOnMainThreadAsync(() =>
			{
				var scrollView = new ScrollViewStub()
				{
					Orientation = ScrollOrientation.Vertical,
					VerticalScrollBarVisibility = visibility
				};

				var scrollViewHandler = CreateHandler(scrollView);

				return ((MauiScrollView)scrollViewHandler.PlatformView).ScrollbarFadingEnabled;
			});

			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData(ScrollBarVisibility.Always, false)]
		[InlineData(ScrollBarVisibility.Default, true)]
		[InlineData(ScrollBarVisibility.Never, true)]
		public async Task HorizontalScrollbarFadingInitializesCorrectly(ScrollBarVisibility visibility, bool expected)
		{
			bool result = await InvokeOnMainThreadAsync(() =>
			{
				var scrollView = new ScrollViewStub()
				{
					Orientation = ScrollOrientation.Horizontal,
					HorizontalScrollBarVisibility = visibility
				};

				var scrollViewHandler = CreateHandler(scrollView);

				return ((MauiHorizontalScrollView)scrollViewHandler.PlatformView.GetChildAt(0)).ScrollbarFadingEnabled;
			});

			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData(ScrollBarVisibility.Always, false)]
		[InlineData(ScrollBarVisibility.Default, true)]
		[InlineData(ScrollBarVisibility.Never, true)]
		public async Task VerticalandHorizontalScrollbarFadingInitializesCorrectlyOnBothOrientation(ScrollBarVisibility visibility, bool expected)
		{
			bool result = await InvokeOnMainThreadAsync(() =>
			{
				var scrollView = new ScrollViewStub()
				{
					Orientation = ScrollOrientation.Both,
					HorizontalScrollBarVisibility = visibility,
					VerticalScrollBarVisibility = visibility
				};

				var scrollViewHandler = CreateHandler(scrollView);

				var horizontalScrollView = (MauiHorizontalScrollView)scrollViewHandler.PlatformView.GetChildAt(0);
				var verticalScrollView = (MauiScrollView)scrollViewHandler.PlatformView;

				return horizontalScrollView.ScrollbarFadingEnabled && verticalScrollView.ScrollbarFadingEnabled;
			});

			Assert.Equal(expected, result);
		}

		// Mirrors ScrollViewHandler.LayoutWaitTimeoutMillis (private there). The fallback cannot fire
		// before its own bound, so a request completing inside it was served by the layout event and
		// one completing at/after it can only have been served by the fallback - which is how the two
		// tests below tell the two waiter outcomes apart instead of both accepting "it eventually ran".
		// Keep in sync with the handler.
		static readonly TimeSpan LayoutWaitBound = TimeSpan.FromSeconds(4);

		// Regression test for the ScrollToAsync layout wait (MapRequestScrollTo).
		// Two waiter outcomes have to be covered, because deferring is only correct if it still
		// terminates and still serves exactly once:
		//   * the view is not laid out yet when the request arrives, and its first layout must
		//     serve the pending request - once, inside the bound (so: by the event, not the timer),
		//     and not again when the fallback later fires;
		//   * ordinary requests arriving while the view sits in IsLaidOut && IsLayoutRequested -
		//     the state the flag-polled retry could never leave - are served instead of parked.
		// The latched state is armed with platform APIs only: a GlobalLayout callback issues its
		// RequestLayout() inside the same performTraversals call stack, where ViewRootImpl's
		// mHandlingLayoutInLayoutRequest guard swallows the follow-up traversal scheduling.
		[Fact]
		[Category(TestCategory.ScrollView)]
		public async Task ScrollToRequestDeferredBeforeFirstLayoutIsServedByThatLayout()
		{
			var scrollView = new ScrollFinishedCountingScrollViewStub();
			var handler = await InvokeOnMainThreadAsync(() => CreateHandler(scrollView));
			var platformView = handler.PlatformView;

			ViewGroup windowRoot = null!;
			FrameLayout holder = null!;
			var observer = (global::Android.Views.ViewTreeObserver?)null;
			global::System.EventHandler? layoutHook = null;
			var armHook = false;

			try
			{
				// Not laid out yet, but attached to a live window: posts and the ViewTreeObserver
				// work, while no layout of this view can resolve the wait without our say-so.
				await InvokeOnMainThreadAsync(() =>
				{
					windowRoot = (ViewGroup)MauiContext.Context!.GetActivity()!
						.FindViewById(global::Android.Resource.Id.Content)!;

					holder = new FrameLayout(MauiContext.Context);
					windowRoot.AddView(holder, new ViewGroup.LayoutParams(100, 100));
					holder.AddView(platformView, new FrameLayout.LayoutParams(100, 100));
					platformView.Visibility = ViewStates.Gone;
				});

				await InvokeOnMainThreadAsync(() =>
				{
					Assert.False(platformView.IsLaidOut);

					ScrollViewHandler.MapRequestScrollTo(handler, scrollView, new ScrollToRequest(0, 30, true));

					// Deferred, not silently dropped, and not served against geometry that is not there yet.
					Assert.Equal(0, scrollView.ScrollFinishedCount);
				});

				// Make the view real and let the first layout pass happen under the latched flag.
				await InvokeOnMainThreadAsync(() =>
				{
					observer = platformView.ViewTreeObserver;

					if (observer is { IsAlive: true })
					{
						layoutHook = (_, _) =>
						{
							if (armHook)
							{
								platformView.RequestLayout();
							}
						};

						observer.GlobalLayout += layoutHook;
					}

					platformView.Visibility = ViewStates.Visible;
					armHook = true;
					platformView.RequestLayout();
				});

				// The deferred request is served by that layout - the only thing that can serve it.
				// Timing matters here: the fallback cannot fire before its own bound, so completing
				// inside it proves the layout event served the request, not the timeout (which the
				// other test covers).
				var servedAt = DateTime.UtcNow;
				var deadline = servedAt.AddSeconds(6);

				while (DateTime.UtcNow < deadline && scrollView.ScrollFinishedCount == 0)
				{
					await Task.Delay(50);
				}

				var servedIn = DateTime.UtcNow - servedAt;

				Assert.True(await InvokeOnMainThreadAsync(() => platformView.IsLaidOut));
				Assert.Equal(1, scrollView.ScrollFinishedCount);
				Assert.True(servedIn < LayoutWaitBound, $"{nameof(ScrollToRequestDeferredBeforeFirstLayoutIsServedByThatLayout)}: served by the fallback timer ({servedIn.TotalMilliseconds:F0} ms), not by the layout event.");

				// And a request arriving in the latched state is served straight away: polling the
				// layout flags is what left the previous retry parked here indefinitely.
				await InvokeOnMainThreadAsync(() =>
				{
					Assert.True(platformView.IsLayoutRequested);

					ScrollViewHandler.MapRequestScrollTo(handler, scrollView, new ScrollToRequest(0, 60, true));
					Assert.Equal(2, scrollView.ScrollFinishedCount);
				});

				// A fallback timer armed while the request was waiting must never serve a second time.
				await Task.Delay(4500);
				Assert.Equal(2, scrollView.ScrollFinishedCount);
			}
			finally
			{
				await InvokeOnMainThreadAsync(() =>
				{
					armHook = false;

					if (observer is { IsAlive: true } && layoutHook is not null)
					{
						observer.GlobalLayout -= layoutHook;
					}

					windowRoot?.RemoveView(holder);
				});
			}
		}

		// The same request for a view that is never laid out still has to terminate: within the wait's bound,
		// exactly once, and without re-arming. A view that is GONE once attached is the state where no
		// layout of that view can ever resolve the wait, which is the state the retry loop it replaces could
		// never leave.
		[Fact]
		[Category(TestCategory.ScrollView)]
		public async Task ScrollToRequestForNeverLaidOutViewTerminatesOnceWithoutRetrying()
		{
			var scrollView = new ScrollFinishedCountingScrollViewStub();
			var handler = await InvokeOnMainThreadAsync(() => CreateHandler(scrollView));
			var platformView = handler.PlatformView;

			ViewGroup windowRoot = null!;
			FrameLayout holder = null!;
			DateTime requestedAt = default;

			try
			{
				await InvokeOnMainThreadAsync(async () =>
				{
					// Gone, but attached to a live window: posts run and the ViewTreeObserver is alive, yet
					// Android never lays this view out, so the wait can only end on its own terms.
					windowRoot = (ViewGroup)MauiContext.Context!.GetActivity()!
						.FindViewById(global::Android.Resource.Id.Content)!;

					holder = new FrameLayout(MauiContext.Context);
					windowRoot.AddView(holder, new ViewGroup.LayoutParams(10, 10));
					holder.AddView(platformView, new FrameLayout.LayoutParams(10, 10));
					platformView.Visibility = ViewStates.Gone;

					// Let the attach traversal settle so the wait is armed after it, not during it.
					var settled = new TaskCompletionSource<bool>();
					platformView.Post(new Java.Lang.Runnable(() => settled.TrySetResult(true)));
					await settled.Task;

					Assert.False(platformView.IsLaidOut);

					// Measured from here: this is when the handler would arm its fallback.
					requestedAt = DateTime.UtcNow;

					ScrollViewHandler.MapRequestScrollTo(handler, scrollView, new ScrollToRequest(0, 30, true));

					// Not served against geometry that does not exist yet.
					Assert.Equal(0, scrollView.ScrollFinishedCount);
				});

				// Off the UI thread, so the looper is free: 8 s covers the wait's own bound with margin.
				var deadline = DateTime.UtcNow.AddSeconds(8);

				while (DateTime.UtcNow < deadline && scrollView.ScrollFinishedCount == 0)
				{
					await Task.Delay(50);
				}

				var servedIn = DateTime.UtcNow - requestedAt;

				Assert.False(platformView.IsLaidOut);
				Assert.Equal(1, scrollView.ScrollFinishedCount);

				// And it can only have been the fallback that served it: the timer cannot fire before its
				// bound, and the view was never laid out, so no layout event existed to serve earlier.
				Assert.True(servedIn >= LayoutWaitBound - TimeSpan.FromMilliseconds(500),
					$"{nameof(ScrollToRequestForNeverLaidOutViewTerminatesOnceWithoutRetrying)}: served in {servedIn.TotalMilliseconds:F0} ms, before the fallback bound - a layout event served this view, so the timeout branch went untested.");

				// A second timeout period: the request must not be parked again, nor served twice.
				await Task.Delay(4500);
				Assert.Equal(1, scrollView.ScrollFinishedCount);
			}
			finally
			{
				if (windowRoot is not null && holder?.Parent is not null)
				{
					await InvokeOnMainThreadAsync(() => windowRoot.RemoveView(holder));
				}
			}
		}

		// ScrollViewStub.ScrollFinished() throws, and the public method cannot be overridden, so the
		// interface is re-implemented here to observe how many times a served request reported back.
		class ScrollFinishedCountingScrollViewStub : ScrollViewStub, IScrollView
		{
			int _scrollFinishedCount;

			public int ScrollFinishedCount => Volatile.Read(ref _scrollFinishedCount);

			void IScrollView.ScrollFinished() => Interlocked.Increment(ref _scrollFinishedCount);
		}

		[Fact]
		public async Task MauiScrollViewGetsFullHeightInHorizontalOrientation()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var sv = new MauiScrollView(MauiContext.Context);
				sv.SetOrientation(ScrollOrientation.Horizontal);
				var content = new Button(MauiContext.Context);
				sv.SetContent(content);

				var hsv = sv.FindViewWithTag("Microsoft.Maui.Android.HorizontalScrollView") as MauiHorizontalScrollView;

				Assert.NotNull(hsv);

				sv.Measure(
					MeasureSpec.MakeMeasureSpec(1000, global::Android.Views.MeasureSpecMode.Exactly),
					MeasureSpec.MakeMeasureSpec(1000, global::Android.Views.MeasureSpecMode.Exactly));

				sv.Layout(0, 0, 1000, 1000);

				var measuredWidth = hsv.MeasuredWidth;
				var measuredHeight = hsv.MeasuredHeight;

				Assert.Equal(1000, measuredWidth);
				Assert.Equal(1000, measuredHeight);
			});
		}
	}
}
