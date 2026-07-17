using System;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class StreamImageSourceServiceTests
	{
		const int ExtremeAspectRatioMultiplier = 4;

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

		[Theory]
		[InlineData(ExtremeAspectRatioMultiplier, 1)]
		[InlineData(1, ExtremeAspectRatioMultiplier)]
		public async Task GetDrawableAsyncLimitsExtremeAspectLargeStreamsToDisplaySize(int widthMultiplier, int heightMultiplier)
		{
			var metrics = MauiProgram.DefaultContext?.Resources?.DisplayMetrics;
			Assert.NotNull(metrics);

			var service = new StreamImageSourceService();
			var expectedColor = Color.FromArgb("#FF0000").ToPlatform();
			var sourceWidth = Math.Max(1, metrics.WidthPixels * widthMultiplier);
			var sourceHeight = Math.Max(1, metrics.HeightPixels * heightMultiplier);
			var imageSource = new StreamImageSourceStub(CreateBitmapStream(sourceWidth, sourceHeight, expectedColor));

			using var result = await service.GetDrawableAsync(imageSource, MauiProgram.DefaultContext);
			var bitmapDrawable = Assert.IsType<BitmapDrawable>(result.Value);
			var bitmap = bitmapDrawable.Bitmap;

			Assert.True(bitmap.Width <= metrics.WidthPixels, $"Expected bitmap width {bitmap.Width} to be <= display width {metrics.WidthPixels}.");
			Assert.True(bitmap.Height <= metrics.HeightPixels, $"Expected bitmap height {bitmap.Height} to be <= display height {metrics.HeightPixels}.");
		}

		[Theory]
		[InlineData(ExtremeAspectRatioMultiplier, 1)]
		[InlineData(1, ExtremeAspectRatioMultiplier)]
		public async Task LoadDrawableAsyncLimitsExtremeAspectLargeStreamsToDisplaySize(int widthMultiplier, int heightMultiplier)
		{
			var metrics = MauiProgram.DefaultContext?.Resources?.DisplayMetrics;
			Assert.NotNull(metrics);

			var expectedColor = Color.FromArgb("#FF0000").ToPlatform();
			var sourceWidth = Math.Max(1, metrics.WidthPixels * widthMultiplier);
			var sourceHeight = Math.Max(1, metrics.HeightPixels * heightMultiplier);
			var imageSource = new StreamImageSourceStub(CreateBitmapStream(sourceWidth, sourceHeight, expectedColor));
			var imageView = new ImageView(MauiProgram.DefaultContext);
			imageView.SetScaleType(ImageView.ScaleType.CenterCrop);

			var service = new StreamImageSourceService();
			await imageView.AttachAndRun(async () =>
			{
				using var result = await service.LoadDrawableAsync(imageSource, imageView);
				var bitmapDrawable = Assert.IsType<BitmapDrawable>(imageView.Drawable);
				var bitmap = bitmapDrawable.Bitmap;

				Assert.NotNull(result);
				Assert.True(bitmap.Width <= metrics.WidthPixels, $"Expected bitmap width {bitmap.Width} to be <= display width {metrics.WidthPixels}.");
				Assert.True(bitmap.Height <= metrics.HeightPixels, $"Expected bitmap height {bitmap.Height} to be <= display height {metrics.HeightPixels}.");
			});
		}
	}
}
