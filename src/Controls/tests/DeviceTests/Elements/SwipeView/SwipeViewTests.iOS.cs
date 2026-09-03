using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
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

		[Fact(DisplayName = "Execute Mode SwipeItem Measures Native Content Width (Issue 37700)")]
		public async Task ExecuteModeSwipeItemMeasuresNativeContentWidth()
		{
			SetupBuilder();

			var content = new VerticalStackLayout
			{
				HeightRequest = 60,
				Background = new SolidColorBrush(Colors.White)
			};

			var swipeItem = new SwipeItem
			{
				Text = "OK"
			};

			var swipeItems = new SwipeItems
			{
				swipeItem
			};
			swipeItems.Mode = SwipeMode.Execute;

			var swipeView = new SwipeView()
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = swipeItems,
				Content = content
			};

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformView = GetPlatformControl(handler);

			await InvokeOnMainThreadAsync(async () =>
			{
				await platformView.AttachAndRun(async () =>
				{
					swipeView.Open(OpenSwipeItem.LeftItems, false);

					// The SwipeView adds subviews dynamically when opening it.
					await AssertEventually(() => platformView.Subviews.Length > 1);

					var actionView = platformView.Subviews.OfType<UIStackView>().FirstOrDefault();
					Assert.NotNull(actionView);

					await AssertEventually(() => actionView.Subviews.Length > 0);

					var nativeSwipeItem = actionView.Subviews.FirstOrDefault();
					Assert.NotNull(nativeSwipeItem);

					await AssertEventually(() => nativeSwipeItem.Frame.Width > 0);

					double contentWidth = platformView.Frame.Width;
					double swipeItemWidth = nativeSwipeItem.Frame.Width;

					// Prior to the fix, a single Execute-mode SwipeItem used a fixed
					// SwipeItemWidth (100pt), and Execute-mode threshold/measurement logic
					// did not measure the native UIButton's actual content size. The fix
					// measures the native menu button via SizeThatFits, so a single
					// short-text item should be sized noticeably smaller than the full
					// SwipeView content width.
					Assert.True(swipeItemWidth < contentWidth,
						$"Expected the Execute-mode SwipeItem width ({swipeItemWidth}pt) to be smaller " +
						$"than the full SwipeView content width ({contentWidth}pt), matching the native " +
						$"button's measured content size instead of the entire SwipeView width.");
				});
			});
		}

		[Fact(DisplayName = "Execute Mode Shared Width Fits Unequal Item Content (Issue 37700)")]
		public async Task ExecuteModeSharedWidthFitsUnequalItemContent()
		{
			SetupBuilder();

			var swipeItems = new SwipeItems
			{
				new SwipeItem { Text = "OK" },
				new SwipeItem { Text = "Long action" },
				new SwipeItem
				{
					Text = "This hidden action must not affect visible widths",
					IsVisible = false
				}
			};
			swipeItems.Mode = SwipeMode.Execute;

			var swipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = swipeItems,
				Content = new Grid()
			};

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformView = GetPlatformControl(handler);

			await InvokeOnMainThreadAsync(async () =>
			{
				await platformView.AttachAndRun(async () =>
				{
					swipeView.Open(OpenSwipeItem.LeftItems, false);

					await AssertEventually(() => platformView.Subviews.Length > 1);

					var actionView = platformView.Subviews.OfType<UIStackView>().FirstOrDefault();
					Assert.NotNull(actionView);

					await AssertEventually(() =>
						actionView.Subviews.Length == 3 &&
						actionView.Subviews[0].Frame.Width > 0 &&
						actionView.Subviews[1].Frame.Width > 0);

					var firstNativeSwipeItem = actionView.Subviews[0];
					var longerNativeSwipeItem = actionView.Subviews[1];
					var hiddenNativeSwipeItem = actionView.Subviews[2];
					Assert.True(hiddenNativeSwipeItem.Hidden);

					Assert.True(Math.Abs(firstNativeSwipeItem.Frame.Width - longerNativeSwipeItem.Frame.Width) < 0.5,
						"Expected Execute-mode SwipeItems to use a shared width.");

					double assignedWidth = longerNativeSwipeItem.Frame.Width;
					double measuredWidth = longerNativeSwipeItem.SizeThatFits(
						new CGSize(platformView.Frame.Width, platformView.Frame.Height)).Width;

					Assert.True(assignedWidth + 0.5 >= measuredWidth,
						$"Expected the shared width ({assignedWidth}pt) to fit the longer item's " +
						$"measured content width ({measuredWidth}pt).");

					double hiddenMeasuredWidth = hiddenNativeSwipeItem.SizeThatFits(
						new CGSize(platformView.Frame.Width, platformView.Frame.Height)).Width;

					Assert.True(assignedWidth < hiddenMeasuredWidth,
						"Expected hidden item content to be excluded from the shared width.");
				});
			});
		}

		[Fact(DisplayName = "Execute Mode Remeasures Item When Text Changes (Issue 37700)")]
		public async Task ExecuteModeRemeasuresItemWhenTextChanges()
		{
			SetupBuilder();

			var firstSwipeItem = new SwipeItem { Text = "OK" };
			var secondSwipeItem = new SwipeItem { Text = "Go" };
			var swipeItems = new SwipeItems
			{
				firstSwipeItem,
				secondSwipeItem
			};
			swipeItems.Mode = SwipeMode.Execute;

			var swipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = swipeItems,
				Content = new Grid()
			};

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformView = GetPlatformControl(handler);

			await InvokeOnMainThreadAsync(async () =>
			{
				await platformView.AttachAndRun(async () =>
				{
					swipeView.Open(OpenSwipeItem.LeftItems, false);

					await AssertEventually(() => platformView.Subviews.Length > 1);

					var actionView = platformView.Subviews.OfType<UIStackView>().FirstOrDefault();
					Assert.NotNull(actionView);

					await AssertEventually(() =>
						actionView.Subviews.Length == 2 &&
						actionView.Subviews.All(item => item.Frame.Width > 0));

					double initialWidth = actionView.Subviews[1].Frame.Width;
					secondSwipeItem.Text = "Long action";

					await AssertEventually(() =>
						actionView.Subviews[1].Frame.Width > initialWidth &&
						Math.Abs(actionView.Subviews[0].Frame.Width - actionView.Subviews[1].Frame.Width) < 0.5);

					double measuredWidth = actionView.Subviews[1].SizeThatFits(
						new CGSize(platformView.Frame.Width, platformView.Frame.Height)).Width;

					Assert.True(actionView.Subviews[1].Frame.Width + 0.5 >= measuredWidth);
				});
			});
		}

		[Fact(DisplayName = "Execute Mode Visibility Change Uses Consistent Item Widths (Issue 37700)")]
		public async Task ExecuteModeVisibilityChangeUsesConsistentItemWidths()
		{
			SetupBuilder();

			var firstSwipeItem = new SwipeItem { Text = "OK" };
			var secondSwipeItem = new SwipeItem
			{
				Text = "Long action",
				IsVisible = false
			};
			var swipeItems = new SwipeItems
			{
				firstSwipeItem,
				secondSwipeItem
			};
			swipeItems.Mode = SwipeMode.Execute;

			var swipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = swipeItems,
				Content = new Grid()
			};

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformView = GetPlatformControl(handler);

			await InvokeOnMainThreadAsync(async () =>
			{
				await platformView.AttachAndRun(async () =>
				{
					swipeView.Open(OpenSwipeItem.LeftItems, false);

					await AssertEventually(() => platformView.Subviews.Length > 1);

					var actionView = platformView.Subviews.OfType<UIStackView>().FirstOrDefault();
					Assert.NotNull(actionView);
					await AssertEventually(() => actionView.Subviews[0].Frame.Width > 0);

					secondSwipeItem.IsVisible = true;

					await AssertEventually(() =>
						!actionView.Subviews[1].Hidden &&
						actionView.Subviews[1].Frame.Width > 0 &&
						Math.Abs(actionView.Subviews[0].Frame.Width - actionView.Subviews[1].Frame.Width) < 0.5);
				});
			});
		}

		[Fact(DisplayName = "Execute Mode Item Width Remains Stable Across Reopen (Issue 37700)")]
		public async Task ExecuteModeItemWidthRemainsStableAcrossReopen()
		{
			SetupBuilder();

			var swipeItems = new SwipeItems
			{
				new SwipeItem
				{
					IconImageSource = new FontImageSource
					{
						Glyph = "A",
						FontFamily = "Arial",
						Size = 24
					}
				}
			};
			swipeItems.Mode = SwipeMode.Execute;

			var swipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = swipeItems,
				Content = new Grid()
			};

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformView = GetPlatformControl(handler);

			await InvokeOnMainThreadAsync(async () =>
			{
				await platformView.AttachAndRun(async () =>
				{
					double initialWidth = 0;

					for (int i = 0; i < 3; i++)
					{
						swipeView.Open(OpenSwipeItem.LeftItems, false);

						UIStackView actionView = null;
						await AssertEventually(() =>
						{
							actionView = platformView.Subviews.OfType<UIStackView>().FirstOrDefault();
							return actionView?.Subviews.FirstOrDefault()?.Frame.Width > 0;
						});

						double currentWidth = actionView!.Subviews[0].Frame.Width;
						if (i == 0)
							initialWidth = currentWidth;
						else
							Assert.True(Math.Abs(currentWidth - initialWidth) < 0.5);

						swipeView.Close(false);
						await AssertEventually(() => platformView.Subviews.OfType<UIStackView>().FirstOrDefault() == null);
					}
				});
			});
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
