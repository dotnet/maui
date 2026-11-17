using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.SwipeView)]
	public partial class SwipeViewTests : ControlsHandlerTestBase
	{
		MauiSwipeView GetPlatformControl(SwipeViewHandler handler) =>
			handler.PlatformView;

		Task<bool> HasChildren(SwipeViewHandler handler)
		{
			return InvokeOnMainThreadAsync(()
				=> GetPlatformControl(handler).Subviews.Length != 0);
		}

		[Fact(DisplayName = "Execute Mode Uses Native Item Size And Open Distance (Issue 37700)")]
		public async Task ExecuteModeUsesNativeItemSizeAndOpenDistance()
		{
			SetupBuilder();

			var (swipeView, content) = CreateSwipeView(
				SwipeMode.Execute,
				new SwipeItem
				{
					Text = "OK"
				});

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformView = GetPlatformControl(handler);

			await InvokeOnMainThreadAsync(async () =>
			{
				await platformView.AttachAndRun(async () =>
				{
					var actionView = await OpenLeftItemsAsync(swipeView, platformView);
					var nativeSwipeItem = Assert.IsType<SwipeItemButton>(Assert.Single(actionView.Subviews));
					var contentView = Assert.IsAssignableFrom<UIView>(content.Handler?.PlatformView);

					await AssertEventually(() => nativeSwipeItem.Frame.Width > 0 && contentView.Frame.X != 0);

					var desiredSize = nativeSwipeItem.SizeThatFits(contentView.Bounds.Size);

					Assert.Equal(desiredSize.Width, nativeSwipeItem.Frame.Width, 1d);
					Assert.Equal(contentView.Bounds.Height, nativeSwipeItem.Frame.Height, 1d);
					Assert.Equal(nativeSwipeItem.Frame.Width, actionView.Frame.Width, 1d);
					Assert.Equal(nativeSwipeItem.Frame.Width, Math.Abs(contentView.Frame.X), 1d);
				});
			});
		}

		[Fact(DisplayName = "Execute Mode Uses Average Native Item Width (Issue 37700)")]
		public async Task ExecuteModeUsesAverageNativeItemWidth()
		{
			SetupBuilder();

			var (swipeView, content) = CreateSwipeView(
				SwipeMode.Execute,
				new SwipeItem
				{
					Text = "OK"
				},
				new SwipeItem
				{
					Text = "A significantly longer action"
				});

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformView = GetPlatformControl(handler);

			await InvokeOnMainThreadAsync(async () =>
			{
				await platformView.AttachAndRun(async () =>
				{
					var actionView = await OpenLeftItemsAsync(swipeView, platformView);
					var nativeSwipeItems = actionView.Subviews.OfType<SwipeItemButton>().ToArray();
					var contentView = Assert.IsAssignableFrom<UIView>(content.Handler?.PlatformView);

					Assert.Equal(2, nativeSwipeItems.Length);
					await AssertEventually(() => nativeSwipeItems.All(item => item.Frame.Width > 0));

					var firstDesiredSize = nativeSwipeItems[0].SizeThatFits(contentView.Bounds.Size);
					var secondDesiredSize = nativeSwipeItems[1].SizeThatFits(contentView.Bounds.Size);
					var totalDesiredWidth = firstDesiredSize.Width + secondDesiredSize.Width;
					var expectedItemWidth = totalDesiredWidth > contentView.Bounds.Width
						? contentView.Bounds.Width / nativeSwipeItems.Length
						: totalDesiredWidth / nativeSwipeItems.Length;
					var totalWidth = nativeSwipeItems.Sum(item => item.Frame.Width);

					Assert.NotEqual(firstDesiredSize.Width, secondDesiredSize.Width);
					Assert.Equal(expectedItemWidth, nativeSwipeItems[0].Frame.Width, 1d);
					Assert.Equal(expectedItemWidth, nativeSwipeItems[1].Frame.Width, 1d);
					Assert.Equal(totalWidth, actionView.Frame.Width, 1d);
					Assert.Equal(totalWidth, Math.Abs(contentView.Frame.X), 1d);
				});
			});
		}

		[Fact(DisplayName = "Execute Mode Icon And Text Size Is Stable After Reopen (Issue 37700)")]
		public async Task ExecuteModeIconAndTextSizeIsStableAfterReopen()
		{
			SetupBuilder();

			var (swipeView, content) = CreateSwipeView(
				SwipeMode.Execute,
				new SwipeItem
				{
					Text = "Back",
					IconImageSource = "red.png"
				});

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformView = GetPlatformControl(handler);

			await InvokeOnMainThreadAsync(async () =>
			{
				await platformView.AttachAndRun(async () =>
				{
					var contentView = Assert.IsAssignableFrom<UIView>(content.Handler?.PlatformView);
					var firstActionView = await OpenLeftItemsAsync(swipeView, platformView);
					var firstButton = Assert.IsType<SwipeItemButton>(Assert.Single(firstActionView.Subviews));

					await AssertEventually(() =>
						firstButton.CurrentImage != null &&
						Math.Abs(firstButton.SizeThatFits(contentView.Bounds.Size).Width - firstButton.Frame.Width) <= 1d);

					var firstDesiredSize = firstButton.SizeThatFits(contentView.Bounds.Size);
					var firstWidth = firstButton.Frame.Width;
					Assert.Equal(firstDesiredSize.Width, firstWidth, 1d);

					swipeView.Close(false);
					await AssertEventually(() => !platformView.Subviews.OfType<UIStackView>().Any());

					var secondActionView = await OpenLeftItemsAsync(swipeView, platformView);
					var secondButton = Assert.IsType<SwipeItemButton>(Assert.Single(secondActionView.Subviews));

					await AssertEventually(() =>
						secondButton.CurrentImage != null &&
						Math.Abs(secondButton.SizeThatFits(contentView.Bounds.Size).Width - secondButton.Frame.Width) <= 1d);

					var secondDesiredSize = secondButton.SizeThatFits(contentView.Bounds.Size);
					Assert.Equal(secondDesiredSize.Width, secondButton.Frame.Width, 1d);
					Assert.Equal(firstWidth, secondButton.Frame.Width, 1d);
					Assert.Equal(secondButton.Frame.Width, Math.Abs(contentView.Frame.X), 1d);
				});
			});
		}

		[Fact(DisplayName = "Horizontal Execute SwipeItemView Uses Parent Height (Issue 37700)")]
		public async Task HorizontalExecuteSwipeItemViewUsesParentHeight()
		{
			SetupBuilder();

			var content = CreateContent();
			var swipeItemView = new SwipeItemView
			{
				Content = new Grid
				{
					WidthRequest = 40,
					HeightRequest = 20
				}
			};
			var swipeItems = new SwipeItems
			{
				Mode = SwipeMode.Execute
			};
			swipeItems.Add(swipeItemView);
			var swipeView = CreateSwipeView(swipeItems, content);

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformView = GetPlatformControl(handler);

			await InvokeOnMainThreadAsync(async () =>
			{
				await platformView.AttachAndRun(async () =>
				{
					var actionView = await OpenLeftItemsAsync(swipeView, platformView);
					var nativeSwipeItem = Assert.Single(actionView.Subviews);
					var contentView = Assert.IsAssignableFrom<UIView>(content.Handler?.PlatformView);

					await AssertEventually(() => nativeSwipeItem.Frame.Width > 0);

					Assert.Equal(40d, nativeSwipeItem.Frame.Width, 1d);
					Assert.Equal(contentView.Bounds.Height, nativeSwipeItem.Frame.Height, 1d);
				});
			});
		}

		[Fact(DisplayName = "Reveal Mode Retains Fixed SwipeItem Width (Issue 37700)")]
		public async Task RevealModeRetainsFixedSwipeItemWidth()
		{
			SetupBuilder();

			var (swipeView, content) = CreateSwipeView(
				SwipeMode.Reveal,
				new SwipeItem
				{
					Text = "A significantly longer action"
				});

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformView = GetPlatformControl(handler);

			await InvokeOnMainThreadAsync(async () =>
			{
				await platformView.AttachAndRun(async () =>
				{
					var actionView = await OpenLeftItemsAsync(swipeView, platformView);
					var nativeSwipeItem = Assert.Single(actionView.Subviews);
					var contentView = Assert.IsAssignableFrom<UIView>(content.Handler?.PlatformView);

					await AssertEventually(() => nativeSwipeItem.Frame.Width > 0 && contentView.Frame.X != 0);

					Assert.Equal(SwipeViewExtensions.SwipeItemWidth, nativeSwipeItem.Frame.Width, 1d);
					Assert.Equal(SwipeViewExtensions.SwipeItemWidth, Math.Abs(contentView.Frame.X), 1d);
				});
			});
		}

		static (SwipeView SwipeView, VerticalStackLayout Content) CreateSwipeView(SwipeMode mode, params SwipeItem[] swipeItems)
		{
			var content = CreateContent();
			var items = new SwipeItems
			{
				Mode = mode
			};

			foreach (var swipeItem in swipeItems)
				items.Add(swipeItem);

			return (CreateSwipeView(items, content), content);
		}

		static SwipeView CreateSwipeView(SwipeItems swipeItems, View content)
		{
			return new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = swipeItems,
				Content = content
			};
		}

		static VerticalStackLayout CreateContent()
		{
			return new VerticalStackLayout
			{
				HeightRequest = 60,
				Background = new SolidColorBrush(Colors.White)
			};
		}

		static async Task<UIStackView> OpenLeftItemsAsync(SwipeView swipeView, MauiSwipeView platformView)
		{
			swipeView.Open(OpenSwipeItem.LeftItems, false);

			await AssertEventually(() => platformView.Subviews.OfType<UIStackView>().Any());

			var actionView = platformView.Subviews.OfType<UIStackView>().FirstOrDefault();
			Assert.NotNull(actionView);
			await AssertEventually(() => actionView.Subviews.Length > 0);

			return actionView;
		}

		[Fact]
		[Description("The Opacity property of a SwipeView should match with native Opacity")]
		public async Task VerifySwipeViewOpacityProperty()
		{
			var swipeView = new SwipeView
			{
				Opacity = 0.35f
			};
			var expectedValue = swipeView.Opacity;

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var nativeView = GetPlatformControl(handler);
			await InvokeOnMainThreadAsync(() =>
   			{
				   var nativeOpacityValue = (float)nativeView.Alpha;
				   Assert.Equal(expectedValue, nativeOpacityValue);
			   });
		}

		[Fact]
		[Description("The IsVisible property of a SwipeView should match with native IsVisible")]
		public async Task VerifySwipeViewIsVisibleProperty()
		{
			var swipeView = new SwipeView
			{
				IsVisible = false
			};
			var expectedValue = swipeView.IsVisible;

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var nativeView = GetPlatformControl(handler);
			await InvokeOnMainThreadAsync(() =>
   			{
				   var isVisible = !nativeView.Hidden;
				   Assert.Equal(expectedValue, isVisible);
			   });
		}
	}
}
