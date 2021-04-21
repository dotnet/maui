using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using UIKit;
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
			
			UIViewContentMode expectedValue = UIViewContentMode.ScaleToFill;

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

		UIImageView GetNativeImage(ImageHandler imageHandler) =>
			(UIImageView)imageHandler.NativeView;

		UIViewContentMode GetNativeAspect(ImageHandler imageHandler) =>
			GetNativeImage(imageHandler).ContentMode;
	}
}