using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageButtonHandlerTests
	{
		[Fact(DisplayName = "ImageButton uses MauiShapeableImageView")]
		public Task ImageButtonUsesMauiShapeableImageView()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(new ImageButtonStub());

				var platformView = Assert.IsType<MauiShapeableImageView>(handler.PlatformView);
				Assert.IsType<MauiMaterialContextThemeWrapper>(platformView.Context);
			});
		}

		[Fact(DisplayName = "Clip ImageButton with Background works Correctly")]
		public async Task ClipImageButtonWithBackgroundWorks()
		{
			Color expected = Colors.Yellow;

			var brush = new SolidPaintStub(expected);

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

		bool ImageSourceLoaded(ImageButtonHandler imageButtonHandler) =>
			imageButtonHandler.PlatformView.Drawable != null;
	}
}