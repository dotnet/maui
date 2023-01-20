using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageButtonHandlerTests
	{
		[Fact(DisplayName = "Clip ImageButton with Background works Correctly",
			Skip = "This test is currently invalid https://github.com/dotnet/maui/issues/11948")]
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

		bool ImageSourceLoaded(ImageButtonHandler imageButtonHandler) =>
			imageButtonHandler.PlatformView.Drawable != null;
	}
}