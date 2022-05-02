using System;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageButtonHandlerTests
	{
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
	}
}