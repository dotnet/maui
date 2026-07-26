using System;
using System.Collections.Generic;
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
