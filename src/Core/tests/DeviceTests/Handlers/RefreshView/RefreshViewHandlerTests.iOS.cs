using Microsoft.Maui.Handlers;
using System;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Hosting;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class RefreshViewHandlerTests
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task DisablingRefreshRestoresOriginalAlwaysBounceVertical(bool originalValue)
		{
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(
				handlers => handlers.AddHandler<ScrollViewStub, ScrollViewHandler>()));

			var scrollView = new ScrollViewStub();
			var refreshView = new RefreshViewStub
			{
				Content = scrollView,
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var scrollViewHandler = CreateHandler<ScrollViewHandler>(scrollView);
				scrollViewHandler.PlatformView.AlwaysBounceVertical = originalValue;
				CreateHandler(refreshView);

				Assert.True(scrollViewHandler.PlatformView.AlwaysBounceVertical);

				refreshView.IsRefreshEnabled = false;

				Assert.Equal(originalValue, scrollViewHandler.PlatformView.AlwaysBounceVertical);
			});
		}

		[Fact]
		public async Task DisablingRefreshPreservesNewAlwaysBounceVerticalValue()
		{
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(
				handlers => handlers.AddHandler<ScrollViewStub, ScrollViewHandler>()));

			var scrollView = new ScrollViewStub();
			var refreshView = new RefreshViewStub
			{
				Content = scrollView,
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var scrollViewHandler = CreateHandler<ScrollViewHandler>(scrollView);
				scrollViewHandler.PlatformView.AlwaysBounceVertical = true;
				CreateHandler(refreshView);

				scrollViewHandler.PlatformView.AlwaysBounceVertical = false;
				refreshView.IsRefreshEnabled = false;

				Assert.False(scrollViewHandler.PlatformView.AlwaysBounceVertical);
			});
		}

		[Fact]
		public async Task RepeatedlyTogglingRefreshRestoresAlwaysBounceVertical()
		{
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(
				handlers => handlers.AddHandler<ScrollViewStub, ScrollViewHandler>()));

			var scrollView = new ScrollViewStub();
			var refreshView = new RefreshViewStub
			{
				Content = scrollView,
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var scrollViewHandler = CreateHandler<ScrollViewHandler>(scrollView);
				CreateHandler(refreshView);

				for (int i = 0; i < 2; i++)
				{
					Assert.True(scrollViewHandler.PlatformView.AlwaysBounceVertical);

					refreshView.IsRefreshEnabled = false;
					Assert.False(scrollViewHandler.PlatformView.AlwaysBounceVertical);

					refreshView.IsRefreshEnabled = true;
				}
			});
		}

		[Fact]
		public async Task ReplacingContentRestoresEachScrollViewsAlwaysBounceVertical()
		{
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(
				handlers => handlers.AddHandler<ScrollViewStub, ScrollViewHandler>()));

			var originalScrollView = new ScrollViewStub();
			var replacementScrollView = new ScrollViewStub();
			var refreshView = new RefreshViewStub
			{
				Content = originalScrollView,
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var originalHandler = CreateHandler<ScrollViewHandler>(originalScrollView);
				var replacementHandler = CreateHandler<ScrollViewHandler>(replacementScrollView);
				replacementHandler.PlatformView.AlwaysBounceVertical = true;
				CreateHandler(refreshView);

				refreshView.Content = replacementScrollView;

				Assert.False(originalHandler.PlatformView.AlwaysBounceVertical);
				Assert.True(replacementHandler.PlatformView.AlwaysBounceVertical);

				refreshView.IsRefreshEnabled = false;

				Assert.True(replacementHandler.PlatformView.AlwaysBounceVertical);
			});
		}

		[Fact]
		public async Task EndingUserPullRefreshKeepsScrollViewAtInsetAdjustedTop()
		{
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(
				handlers => handlers.AddHandler<ScrollViewStub, ScrollViewHandler>()));

			var scrollView = new ScrollViewStub();
			var refreshView = new RefreshViewStub
			{
				Content = scrollView,
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var platformScrollView = CreateHandler<ScrollViewHandler>(scrollView).PlatformView;
				var handler = CreateHandler(refreshView);

				// UIRefreshControl only starts refreshing inside a window.
				await handler.PlatformView.AttachAndRun(async () =>
				{
					// Like a large title: the scroll view rests at -AdjustedContentInset.Top, not at 0.
					platformScrollView.ContentInset = new UIEdgeInsets(100, 0, 0, 0);
					var top = -platformScrollView.AdjustedContentInset.Top;
					platformScrollView.ContentOffset = new CGPoint(0, top);

					// A user pull: UIKit starts the refresh control, then ValueChanged sets IsRefreshing.
					var refreshControl = handler.PlatformView.RefreshControl;
					refreshControl.BeginRefreshing();
					Assert.True(refreshControl.Refreshing);
					refreshControl.SendActionForControlEvents(UIControlEvent.ValueChanged);
					Assert.True(refreshView.IsRefreshing);
					handler.UpdateValue(nameof(IRefreshView.IsRefreshing));

					refreshView.IsRefreshing = false;
					handler.UpdateValue(nameof(IRefreshView.IsRefreshing));

					await AssertHelpers.AssertEventually(
						() => Math.Abs(platformScrollView.ContentOffset.Y - top) < 1,
						timeout: 2000,
						message: $"ContentOffset.Y was {platformScrollView.ContentOffset.Y} right after the refresh ended and did not settle at {top}");
				});
			});
		}

		MauiRefreshView GetNativeRefreshView(RefreshViewHandler RefreshViewHandler) =>
			(MauiRefreshView)RefreshViewHandler.PlatformView;

		bool GetPlatformIsRefreshing(RefreshViewHandler RefreshViewHandler) =>
			GetNativeRefreshView(RefreshViewHandler).IsRefreshing;
	}
}