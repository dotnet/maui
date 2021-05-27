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
		public async Task ThrowsForIncorrectTypes(Type type)
		{
			var service = new StreamImageSourceService();

			var imageSource = (ImageSourceStub)Activator.CreateInstance(type);

			await Assert.ThrowsAsync<InvalidCastException>(() => service.GetDrawableAsync(imageSource, Platform.DefaultContext));
		}

		[Theory]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#000000")]
		public async Task GetDrawableAsync(string colorHex)
		{
			var expectedColor = Color.FromArgb(colorHex).ToNative();

			var service = new StreamImageSourceService();

			var imageSource = new StreamImageSourceStub(CreateBitmapStream(100, 100, expectedColor));

			using var drawable = await service.GetDrawableAsync(imageSource, Platform.DefaultContext);

			var bitmapDrawable = Assert.IsType<BitmapDrawable>(drawable.Value);

			var bitmap = bitmapDrawable.Bitmap;

			bitmap.AssertColorAtCenter(expectedColor);
		}
	}
}