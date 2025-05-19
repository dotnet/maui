using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class StreamImageSourceServiceTests
	{
		[Theory]
		[InlineData(typeof(FileImageSourceStub))]
		[InlineData(typeof(FontImageSourceStub))]
		[InlineData(typeof(UriImageSourceStub))]
		public async Task ThrowsForIncorrectTypes(Type type)
		{
			var service = new StreamImageSourceService();

			var imageSource = (ImageSourceStub)Activator.CreateInstance(type);

			await Assert.ThrowsAsync<InvalidCastException>(() => service.GetImageAsync(imageSource));
		}

		[Theory]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#000000")]
		public async Task GetImageAsync(string colorHex)
		{
			await MainThread.InvokeOnMainThreadAsync(async () =>
		   {
			   var expectedColor = Color.FromArgb(colorHex).ToPlatform();

			   var service = new StreamImageSourceService();

			   var imageSource = new StreamImageSourceStub(CreateBitmapStream(100, 100, expectedColor));

			   using var drawable = await service.GetImageAsync(imageSource);

			   var image = Assert.IsType<UIImage>(drawable.Value);

			   image.AssertColorAtCenter(expectedColor);

		   });
		}
	}
}