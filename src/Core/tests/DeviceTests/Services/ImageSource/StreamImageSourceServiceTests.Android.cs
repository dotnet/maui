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
		const int OversizePixelMargin = 500;

		[Theory]
		[InlineData(typeof(FileImageSourceStub))]
		[InlineData(typeof(FontImageSourceStub))]
		[InlineData(typeof(UriImageSourceStub))]
		public async Task ThrowsForIncorrectTypes(Type type)
		{
			var service = new StreamImageSourceService();

			var imageSource = (ImageSourceStub)Activator.CreateInstance(type);

			await Assert.ThrowsAsync<InvalidCastException>(() => service.GetDrawableAsync(imageSource, MauiProgram.DefaultContext));
		}

		[Theory]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#000000")]
		public async Task GetDrawableAsync(string colorHex)
		{
			var expectedColor = Color.FromArgb(colorHex).ToPlatform();

			var service = new StreamImageSourceService();

			var stream = CreateBitmapStream(100, 100, expectedColor);

			var imageSource = new StreamImageSourceStub(stream);

			using var result = await service.GetDrawableAsync(imageSource, MauiProgram.DefaultContext);

			var bitmapDrawable = Assert.IsType<BitmapDrawable>(result.Value);

			var bitmap = bitmapDrawable.Bitmap;

			await bitmap.AssertContainsColor(expectedColor).ConfigureAwait(false);
		}

		[Fact]
		public async Task GetDrawableAsyncLimitsLargeStreamToDisplaySize()
		{
			var metrics = MauiProgram.DefaultContext?.Resources?.DisplayMetrics;
			Assert.NotNull(metrics);

			var service = new StreamImageSourceService();
			var expectedColor = Color.FromArgb("#FF0000").ToPlatform();
			var sourceWidth = metrics.WidthPixels + OversizePixelMargin;
			var sourceHeight = metrics.HeightPixels + OversizePixelMargin;
			var imageSource = new StreamImageSourceStub(CreateBitmapStream(sourceWidth, sourceHeight, expectedColor));

			using var result = await service.GetDrawableAsync(imageSource, MauiProgram.DefaultContext);
			var bitmapDrawable = Assert.IsType<BitmapDrawable>(result.Value);

			Assert.True(bitmapDrawable.Bitmap.Width <= metrics.WidthPixels);
			Assert.True(bitmapDrawable.Bitmap.Height <= metrics.HeightPixels);
		}
	}
}
