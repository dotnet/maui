using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageButtonHandlerTests
	{
		[Fact(DisplayName = "Clip ImageButton with Background works Correctly")]
		public async Task ClipImageButtonWithBackgroundWorks()
		{
			Color expected = Colors.Yellow;

			var brush = new LinearGradientPaintStub(Colors.Blue, expected);

			var imageButton = new ImageButtonStub
			{
				Background = brush,
				Clip = new EllipseShapeStub()
			};

			await ValidateHasColor(imageButton, expected);
		}

		Google.Android.Material.ImageView.ShapeableImageView GetPlatformImageButton(ImageButtonHandler buttonHandler) =>
			buttonHandler.PlatformView;

		Task PerformClick(IImageButton button)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				GetPlatformImageButton(CreateHandler(button)).PerformClick();
			});
		}

		Thickness GetNativePadding(ImageButtonHandler imageButtonHandler)
		{
			var shapeableImageView = GetPlatformImageButton(imageButtonHandler);

			return new Thickness(
				shapeableImageView.ContentPaddingLeft,
				shapeableImageView.ContentPaddingTop,
				shapeableImageView.ContentPaddingRight,
				shapeableImageView.ContentPaddingBottom);
		}

		Task ValidateHasColor(IImageButton imageButton, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var platformImageButton = GetPlatformImageButton(CreateHandler(imageButton));
				action?.Invoke();
				platformImageButton.AssertContainsColor(color);
			});
		}

		bool ImageSourceLoaded(ImageButtonHandler imageButtonHandler) =>
			imageButtonHandler.PlatformView.Drawable != null;
	}
}