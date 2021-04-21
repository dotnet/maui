using System;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class FileImageSourceServiceTests
	{
		[Theory]
		[InlineData(typeof(FontImageSourceStub))]
		[InlineData(typeof(StreamImageSourceStub))]
		[InlineData(typeof(UriImageSourceStub))]
		public async Task GetDrawableAsyncReturnsNullForIncorrectTypes(Type type)
		{
			var service = new FileImageSourceService();

			var imageSource = (ImageSourceStub)Activator.CreateInstance(type);

			var drawable = await service.GetDrawableAsync(imageSource, Platform.DefaultContext);

			Assert.Null(drawable);
		}

		[Theory]
		[InlineData("red.png", "#FF0000")]
		[InlineData("green.png", "#00FF00")]
		[InlineData("black.png", "#000000")]
		public async Task GetDrawableAsyncWithResource(string filename, string colorHex)
		{
			var service = new FileImageSourceService();

			var imageSource = new FileImageSourceStub(filename);

			var drawable = await service.GetDrawableAsync(imageSource, Platform.DefaultContext);

			var bitmapDrawable = Assert.IsType<BitmapDrawable>(drawable.Value);

			var bitmap = bitmapDrawable.Bitmap;

			var expectedColor = Color.FromHex(colorHex);
			bitmap.AssertColorAtCenter(expectedColor.ToNative());
		}
	}
}