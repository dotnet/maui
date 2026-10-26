using System;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageButtonHandlerTests
	{
		[Fact]
		public async Task GradientCornerRadiusUpdatesWhenResized()
		{
			var imageButton = new ImageButtonStub
			{
				Background = new LinearGradientPaintStub(Colors.Red, Colors.Blue),
				CornerRadius = 100,
				Width = 50,
				Height = 50,
				DesiredSize = new Microsoft.Maui.Graphics.Size(50, 50)
			};

			var radii = await GetValueAsync(imageButton, handler =>
			{
				double GetGradientCornerRadius()
				{
					var backgroundLayer = handler.PlatformView.Layer.Sublayers?.FirstOrDefault(layer => layer.Name == "MauiBackgroundLayer");
					Assert.NotNull(backgroundLayer);
					return backgroundLayer.CornerRadius;
				}

				handler.PlatformArrange(new Microsoft.Maui.Graphics.Rect(0, 0, 50, 50));
				var initialRadius = GetGradientCornerRadius();

				var resizedBounds = new Microsoft.Maui.Graphics.Rect(0, 0, 40, 40);
				handler.PlatformArrange(resizedBounds);
				var resizedRadius = GetGradientCornerRadius();
				handler.PlatformArrange(resizedBounds);

				return (initialRadius, resizedRadius, sameSizeRadius: GetGradientCornerRadius());
			});

			Assert.Equal(25, radii.initialRadius, Precision);
			Assert.Equal(20, radii.resizedRadius, Precision);
			Assert.Equal(20, radii.sameSizeRadius, Precision);
		}

		[Fact]
		public async Task GradientCornerRadiusUpdatesWhenPropertyChanges()
		{
			var imageButton = new ImageButtonStub
			{
				Background = new LinearGradientPaintStub(Colors.Red, Colors.Blue),
				CornerRadius = 10,
				Width = 50,
				Height = 50,
				DesiredSize = new Microsoft.Maui.Graphics.Size(50, 50)
			};

			var radii = await GetValueAsync(imageButton, handler =>
			{
				var backgroundLayer = handler.PlatformView.Layer.Sublayers?.FirstOrDefault(layer => layer.Name == "MauiBackgroundLayer");
				Assert.NotNull(backgroundLayer);
				var initialRadius = backgroundLayer.CornerRadius;

				imageButton.CornerRadius = 20;
				handler.UpdateValue(nameof(imageButton.CornerRadius));

				var updatedBackgroundLayer = handler.PlatformView.Layer.Sublayers?.FirstOrDefault(layer => layer.Name == "MauiBackgroundLayer");
				Assert.Same(backgroundLayer, updatedBackgroundLayer);

				return (initialRadius, updatedRadius: updatedBackgroundLayer.CornerRadius);
			});

			Assert.Equal(10, radii.initialRadius, Precision);
			Assert.Equal(20, radii.updatedRadius, Precision);
		}

		[Fact]
		public async Task CornerRadiusUpdatesWhenResized()
		{
			var imageButton = new ImageButtonStub
			{
				CornerRadius = 100,
				Width = 50,
				Height = 50,
				DesiredSize = new Microsoft.Maui.Graphics.Size(50, 50)
			};

			var radii = await GetValueAsync(imageButton, handler =>
			{
				handler.PlatformArrange(new Microsoft.Maui.Graphics.Rect(0, 0, 50, 50));
				var initialRadius = (double)handler.PlatformView.Layer.CornerRadius;

				var resizedBounds = new Microsoft.Maui.Graphics.Rect(0, 0, 40, 40);
				handler.PlatformArrange(resizedBounds);
				var resizedRadius = (double)handler.PlatformView.Layer.CornerRadius;
				handler.PlatformArrange(resizedBounds);

				return (initialRadius, resizedRadius, sameSizeRadius: (double)handler.PlatformView.Layer.CornerRadius);
			});

			Assert.Equal(25, radii.initialRadius, Precision);
			Assert.Equal(20, radii.resizedRadius, Precision);
			Assert.Equal(20, radii.sameSizeRadius, Precision);
		}

		[Theory]
		[InlineData(50)]
		[InlineData(40)]
		public async Task OversizedCornerRadiusStillRendersBackground(int size)
		{
			var imageButton = new ImageButtonStub
			{
				Background = new SolidPaintStub(Colors.Red),
				CornerRadius = 100,
				Width = size,
				Height = size,
				DesiredSize = new Microsoft.Maui.Graphics.Size(size, size)
			};

			var actualRadius = await GetValueAsync(imageButton, handler =>
			{
				handler.PlatformArrange(new Microsoft.Maui.Graphics.Rect(0, 0, size, size));
				return (double)handler.PlatformView.Layer.CornerRadius;
			});

			Assert.Equal(size / 2d, actualRadius, Precision);
			await ValidateHasColor(imageButton, Colors.Red);
		}

		[Fact(DisplayName = "Stroke Color Initializes Correctly")]
		public async Task StrokeColorInitializesCorrectly()
		{
			var xplatStrokeColor = Colors.CadetBlue;

			var imageButton = new ImageButtonStub
			{
				Height = 50,
				Width = 100,
				StrokeThickness = 2,
				StrokeColor = xplatStrokeColor
			};

			var expectedValue = xplatStrokeColor.ToPlatform();

			var values = await GetValueAsync(imageButton, (handler) =>
			{
				return new
				{
					ViewValue = imageButton.StrokeColor,
					PlatformViewValue = GetNativeStrokeColor(handler)
				};
			});

			Assert.Equal(xplatStrokeColor, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue);
		}

		//src/Compatibility/Core/tests/iOS/ImageButtonTests.cs
		[Fact]
		[Trait("Category", "ImageButton")]
		public async Task CreatedWithCorrectButtonType()
		{
			var imageButton = new ImageButton();
			var handler = await CreateHandlerAsync(imageButton);

			var buttonType = await InvokeOnMainThreadAsync(() =>
			{
				var uiButton = GetPlatformImageButton(handler);
				return uiButton.ButtonType;
			});

			Assert.NotEqual(UIButtonType.Custom, buttonType);
		}
		UIButton GetPlatformImageButton(ImageButtonHandler imageButtonHandler) =>
			imageButtonHandler.PlatformView;

		UIColor GetNativeStrokeColor(ImageButtonHandler imageButtonHandler)
		{
			var platformButton = GetPlatformImageButton(imageButtonHandler);

			if (platformButton.Layer != null)
				return new UIColor(platformButton.Layer.BorderColor);

			return UIColor.Clear;
		}

		Task PerformClick(IImageButton button)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				GetPlatformImageButton(CreateHandler(button)).SendActionForControlEvents(UIControlEvent.TouchUpInside);
			});
		}

#pragma warning disable CA1416, CA1422
		UIEdgeInsets GetNativePadding(ImageButtonHandler imageButtonHandler) =>
			GetPlatformImageButton(imageButtonHandler).ContentEdgeInsets;
#pragma warning restore CA1416, CA1422

		bool ImageSourceLoaded(ImageButtonHandler imageButtonHandler) =>
			imageButtonHandler.PlatformView.ImageView.Image != null;

		public partial class ImageButtonImageHandlerTests
		{
			protected override bool UsesAnimatedImages => false;
		}
	}
}