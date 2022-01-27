using System;
using System.Threading.Tasks;
using CoreGraphics;
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

			var expectedValue = xplatStrokeColor.ToNative();

			var values = await GetValueAsync(imageButton, (handler) =>
			{
				return new
				{
					ViewValue = imageButton.StrokeColor,
					NativeViewValue = GetNativeStrokeColor(handler)
				};
			});

			Assert.Equal(xplatStrokeColor, values.ViewValue);
			Assert.Equal(expectedValue, values.NativeViewValue);
		}

		UIButton GetNativeImageButton(ImageButtonHandler imageButtonHandler) =>
			   imageButtonHandler.NativeView;

		UIColor GetNativeStrokeColor(ImageButtonHandler imageButtonHandler)
		{
			var nativeButton = GetNativeImageButton(imageButtonHandler);

			if (nativeButton.Layer != null)
				return new UIColor(nativeButton.Layer.BorderColor);

			return UIColor.Clear;
		}

		Task PerformClick(IButton button)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				GetNativeImageButton(CreateHandler(button)).SendActionForControlEvents(UIControlEvent.TouchUpInside);
			});
		}
	}
}