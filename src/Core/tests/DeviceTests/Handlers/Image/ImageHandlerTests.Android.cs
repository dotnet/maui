using System.Threading.Tasks;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageHandlerTests
	{
		[Fact(DisplayName = "Aspect Initializes Correctly")]
		public async Task AspectInitializesCorrectly()
		{
			var xplatAspect = Aspect.Fill;
			var image = new ImageStub()
			{
				Aspect = xplatAspect
			};
			
			ImageView.ScaleType expectedValue = ImageView.ScaleType.FitXy;

			var values = await GetValueAsync(image, (handler) =>
			{
				return new
				{
					ViewValue = image.Aspect,
					NativeViewValue = GetNativeAspect(handler)
				};
			});
			
			Assert.Equal(xplatAspect, values.ViewValue);
			Assert.Equal(expectedValue, values.NativeViewValue);
		}

		AppCompatImageView GetNativeImage(ImageHandler imageHandler) =>
			(AppCompatImageView)imageHandler.NativeView;

		ImageView.ScaleType GetNativeAspect(ImageHandler imageHandler) =>
			GetNativeImage(imageHandler).GetScaleType();
	}
}