using System;
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