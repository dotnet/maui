using System;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class StreamImageSourceServiceTests
	{
		[Theory]
		[InlineData(typeof(FileImageSourceStub))]
		[InlineData(typeof(FontImageSourceStub))]
		[InlineData(typeof(UriImageSourceStub))]
		public async Task GetDrawableAsyncReturnsNullForIncorrectTypes(Type type)
		{
			var service = new StreamImageSourceService();

			var imageSource = (ImageSourceStub)Activator.CreateInstance(type);

			var drawable = await service.GetDrawableAsync(imageSource, Platform.DefaultContext);

			Assert.Null(drawable);
		}

		[Theory]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#000000")]
		public async Task GetDrawableAsync(string colorHex)
		{
			var expectedColor = Color.FromHex(colorHex).ToNative();

			var service = new StreamImageSourceService();

			var imageSource = new StreamImageSourceStub(CreateBitmapStream(100, 100, expectedColor));

			var drawable = await service.GetDrawableAsync(imageSource, Platform.DefaultContext);

			var bitmapDrawable = Assert.IsType<BitmapDrawable>(drawable);

			var bitmap = bitmapDrawable.Bitmap;

			bitmap.AssertColorAtCenter(expectedColor);
		}
	}
}