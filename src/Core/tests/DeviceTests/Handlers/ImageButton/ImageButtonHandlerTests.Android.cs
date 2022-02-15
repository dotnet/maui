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
		Google.Android.Material.ImageView.ShapeableImageView GetNativeImageButton(ImageButtonHandler buttonHandler) =>
			buttonHandler.NativeView;

		Task PerformClick(IButton button)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				GetNativeImageButton(CreateHandler(button)).PerformClick();
			});
		}
	}
}