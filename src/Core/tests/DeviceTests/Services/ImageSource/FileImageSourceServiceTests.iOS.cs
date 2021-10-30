using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class FileImageSourceServiceTests
	{
		[Theory]
		[InlineData(typeof(FontImageSourceStub))]
		[InlineData(typeof(StreamImageSourceStub))]
		[InlineData(typeof(UriImageSourceStub))]
		public async Task ThrowsForIncorrectTypes(Type type)
		{
			var service = new FileImageSourceService();

			var imageSource = (ImageSourceStub)Activator.CreateInstance(type);

			await Assert.ThrowsAsync<InvalidCastException>(() => service.GetImageAsync(imageSource));
		}

		[Theory]
		[InlineData("red.png", "#FF0000")]
		[InlineData("green.png", "#00FF00")]
		[InlineData("black.png", "#000000")]
		public async Task GetImageAsyncWithResource(string filename, string colorHex)
		{
			var service = new FileImageSourceService();

			var imageSource = new FileImageSourceStub(filename);

			using var result = await service.GetImageAsync(imageSource);

			var uiimage = Assert.IsType<UIImage>(result.Value);

			var expectedColor = Color.FromArgb(colorHex);
			uiimage.AssertColorAtCenter(expectedColor.ToNative());
		}

		[Theory]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#000000")]
		public async Task GetImageAsyncWithFile(string colorHex)
		{
			var expectedColor = Color.FromArgb(colorHex);

			var service = new FileImageSourceService();

			var filename = CreateBitmapFile(100, 100, expectedColor);
			var imageSource = new FileImageSourceStub(filename);

			using var drawable = await service.GetImageAsync(imageSource);

			var uiimage = Assert.IsType<UIImage>(drawable.Value);

			uiimage.AssertColorAtCenter(expectedColor.ToNative());
		}
	}
}