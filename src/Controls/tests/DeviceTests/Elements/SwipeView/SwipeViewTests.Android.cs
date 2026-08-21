using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
using ATextView = Android.Widget.TextView;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SwipeViewTests : ControlsHandlerTestBase
	{
		[Fact(DisplayName = "SwipeItem Size Initializes Correctly")]
		public async Task SwipeItemSizeInitializesCorrectly()
		{
			SetupBuilder();

			var expectedColor = Colors.Red;

			var content = new VerticalStackLayout
			{
				HeightRequest = 60,
				Background = new SolidColorBrush(Colors.White)
			};

			var swipeItemContent = new Grid
			{
				BackgroundColor = expectedColor,
				WidthRequest = 60,
			};

			var swipeItem = new SwipeItemView
			{
				Content = swipeItemContent
			};

			var swipeItems = new SwipeItems
			{
				swipeItem
			};

			var swipeView = new SwipeView()
			{
				HeightRequest = 60,
				LeftItems = swipeItems,
				Content = content
			};

			await AttachAndRun(swipeView, async (handler) =>
			{
				var platformView = ((SwipeViewHandler)handler).PlatformView;
				var openRequest = new SwipeViewOpenRequest(OpenSwipeItem.LeftItems, false);
				swipeView.Open(OpenSwipeItem.LeftItems, false);

				// The SwipeView add children dynamically opening it.
				await AssertEventually(() => platformView.ChildCount > 1);

				var actionView = platformView.GetChildAt(1) as ViewGroup;
				Assert.NotNull(actionView);

				await AssertEventually(() => actionView.ChildCount > 0);

				var swipeItem = actionView.GetChildAt(0);
				Assert.NotNull(swipeItem);

				await AssertEventually(() => swipeItem.Width > 0);
				Assert.NotEqual(0, swipeItem.Width);
			});
		}

		[Fact]
		[Description("The ScaleX property of a SwipeView should match with native ScaleX")]
		public async Task ScaleXConsistent()
		{
			var swipeView = new SwipeView() { ScaleX = 0.45f };
			var expected = swipeView.ScaleX;
			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformSwipeView = GetPlatformControl(handler);
			var platformScaleX = await InvokeOnMainThreadAsync(() => platformSwipeView.ScaleX);
			Assert.Equal(expected, platformScaleX);
		}

		[Fact]
		[Description("The ScaleY property of a SwipeView should match with native ScaleY")]
		public async Task ScaleYConsistent()
		{
			var swipeView = new SwipeView() { ScaleY = 1.23f };
			var expected = swipeView.ScaleY;
			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformSwipeView = GetPlatformControl(handler);
			var platformScaleY = await InvokeOnMainThreadAsync(() => platformSwipeView.ScaleY);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The Scale property of a SwipeView should match with native Scale")]
		public async Task ScaleConsistent()
		{
			var swipeView = new SwipeView() { Scale = 2.0f };
			var expected = swipeView.Scale;
			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformSwipeView = GetPlatformControl(handler);
			var platformScaleX = await InvokeOnMainThreadAsync(() => platformSwipeView.ScaleX);
			var platformScaleY = await InvokeOnMainThreadAsync(() => platformSwipeView.ScaleY);
			Assert.Equal(expected, platformScaleX);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The RotationX property of a SwipeView should match with native RotationX")]
		public async Task RotationXConsistent()
		{
			var swipeView = new SwipeView() { RotationX = 33.0 };
			var expected = swipeView.RotationX;
			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformSwipeView = GetPlatformControl(handler);
			var platformRotationX = await InvokeOnMainThreadAsync(() => platformSwipeView.RotationX);
			Assert.Equal(expected, platformRotationX);
		}

		[Fact]
		[Description("The RotationY property of a SwipeView should match with native RotationY")]
		public async Task RotationYConsistent()
		{
			var swipeView = new SwipeView() { RotationY = 87.0 };
			var expected = swipeView.RotationY;
			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformSwipeView = GetPlatformControl(handler);
			var platformRotationY = await InvokeOnMainThreadAsync(() => platformSwipeView.RotationY);
			Assert.Equal(expected, platformRotationY);
		}

		[Fact]
		[Description("The Rotation property of a SwipeView should match with native Rotation")]
		public async Task RotationConsistent()
		{
			var swipeView = new SwipeView() { Rotation = 23.0 };
			var expected = swipeView.Rotation;
			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformSwipeView = GetPlatformControl(handler);
			var platformRotation = await InvokeOnMainThreadAsync(() => platformSwipeView.Rotation);
			Assert.Equal(expected, platformRotation);
		}
		MauiSwipeView GetPlatformControl(SwipeViewHandler handler) =>
			handler.PlatformView;

		Task<bool> HasChildren(SwipeViewHandler handler)
		{
			return InvokeOnMainThreadAsync(()
				=> GetPlatformControl(handler).ChildCount != 0);
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
				   var isVisible = nativeView.Visibility == global::Android.Views.ViewStates.Visible;
				   Assert.Equal(expectedValue, isVisible);
			   });
		}

		//src/Compatibility/Core/tests/Android/TranslationTests.cs
		[Fact]
		[Description("The Translation property of a SwipeView should match with native Translation")]
		public async Task SwipeViewTranslationConsistent()
		{
			var swipeView = new SwipeView()
			{
				TranslationX = 50,
				TranslationY = -20
			};

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var nativeView = GetPlatformControl(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				AssertTranslationMatches(nativeView, swipeView.TranslationX, swipeView.TranslationY);
			});
		}

		[Fact]
		[Description("The IsEnabled of a SwipeView should match with native IsEnabled")]
		public async Task VerifySwipeViewIsEnabledProperty()
		{
			var swipeView = new SwipeView
			{
				IsEnabled = false
			};
			var expectedValue = swipeView.IsEnabled;

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var nativeView = GetPlatformControl(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				var isEnabled = nativeView.Enabled;
				Assert.Equal(expectedValue, isEnabled);
			});
		}

		[Theory]
		[InlineData(1)]
		[InlineData(5)]
		[InlineData(20)]
		[Description("SwipeItem icon and text should remain centered together when wrapping a CollectionView")]
		public async Task SwipeItemIconAndTextRemainAlignedWithCollectionView(int itemCount)
		{
			SetupBuilder();

			var collectionView = new CollectionView
			{
				ItemsSource = Enumerable.Range(1, itemCount).Select(index => $"{index} record").ToArray(),
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label { Padding = 10 };
					label.SetBinding(Label.TextProperty, ".");
					return label;
				})
			};

			var swipeItem = new SwipeItem
			{
				Text = "Back",
				BackgroundColor = Colors.White,
				IconImageSource = new FileImageSource { File = "red.png" }
			};

			var swipeItems = new SwipeItems
			{
				swipeItem
			};

			var swipeView = new SwipeView()
			{
				LeftItems = swipeItems,
				Content = collectionView
			};

			var root = new Grid
			{
				HeightRequest = 500,
				WidthRequest = 300,
				RowDefinitions =
				{
					new RowDefinition { Height = 40 },
					new RowDefinition { Height = GridLength.Star }
				}
			};
			root.Add(new Label { Text = "Records" });
			root.Add(swipeView, row: 1);

			await AttachAndRun(root, async (_) =>
			{
				var platformView = Assert.IsType<SwipeViewHandler>(swipeView.Handler).PlatformView;
				swipeView.Open(OpenSwipeItem.LeftItems, false);

				await AssertEventually(() => platformView.ChildCount > 1);

				var actionView = platformView.GetChildAt(1) as ViewGroup;
				Assert.NotNull(actionView);

				await AssertEventually(() => actionView.ChildCount > 0);

				var swipeButton = Assert.IsAssignableFrom<ATextView>(actionView.GetChildAt(0));

				await AssertEventually(() =>
					swipeButton.Height > 0 &&
					swipeButton.Baseline > 0 &&
					swipeButton.GetCompoundDrawables()[1] is not null);

				var icon = swipeButton.GetCompoundDrawables()[1];
				var fontMetrics = new global::Android.Graphics.Paint.FontMetricsInt();
				swipeButton.Paint.GetFontMetricsInt(fontMetrics);
				var iconTop = swipeButton.PaddingTop;
				var iconBottom = iconTop + icon.Bounds.Height();
				var textTop = swipeButton.Baseline + fontMetrics.Top;
				var density = swipeButton.Context.GetDisplayDensity();
				var tolerance = (int)Math.Ceiling(density);
				var maxAlignmentGap = Math.Max(
					swipeButton.CompoundDrawablePadding + tolerance,
					swipeButton.LineHeight / 2);

				Assert.InRange(textTop - iconBottom, 0, maxAlignmentGap);
			});
		}
	}
}