using System;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class UriImageSourceServiceTests
	{
		const int ExtremeAspectRatioMultiplier = 4;

		[Theory]
		[InlineData(typeof(FileImageSourceStub))]
		[InlineData(typeof(FontImageSourceStub))]
		[InlineData(typeof(StreamImageSourceStub))]
		public async Task ThrowsForIncorrectTypes(Type type)
		{
			var service = new UriImageSourceService();

			var imageSource = (ImageSourceStub)Activator.CreateInstance(type);

			await Assert.ThrowsAsync<InvalidCastException>(() => service.GetDrawableAsync(imageSource, MauiProgram.DefaultContext));
		}

		[Theory]
		[InlineData(ExtremeAspectRatioMultiplier, 1)]
		[InlineData(1, ExtremeAspectRatioMultiplier)]
		public async Task GetDrawableAsyncLimitsExtremeAspectLargeFileUrisToDisplaySize(int widthMultiplier, int heightMultiplier)
		{
			var metrics = MauiProgram.DefaultContext?.Resources?.DisplayMetrics;
			Assert.NotNull(metrics);

			var expectedColor = Color.FromArgb("#FF0000").ToPlatform();
			var sourceWidth = Math.Max(1, metrics.WidthPixels * widthMultiplier);
			var sourceHeight = Math.Max(1, metrics.HeightPixels * heightMultiplier);
			var filename = BaseImageSourceServiceTests.CreateBitmapFile(sourceWidth, sourceHeight, expectedColor);
			var imageSource = new UriImageSourceStub(new Uri($"file://{filename}"));

			var service = new UriImageSourceService();
			using var result = await service.GetDrawableAsync(imageSource, MauiProgram.DefaultContext);
			var bitmapDrawable = Assert.IsType<BitmapDrawable>(result.Value);
			var bitmap = bitmapDrawable.Bitmap;

			Assert.True(bitmap.Width <= metrics.WidthPixels, $"Expected bitmap width {bitmap.Width} to be <= display width {metrics.WidthPixels}.");
			Assert.True(bitmap.Height <= metrics.HeightPixels, $"Expected bitmap height {bitmap.Height} to be <= display height {metrics.HeightPixels}.");
		}

		[Theory]
		[InlineData(ExtremeAspectRatioMultiplier, 1, "CenterCrop")]
		[InlineData(1, ExtremeAspectRatioMultiplier, "CenterCrop")]
		[InlineData(ExtremeAspectRatioMultiplier, 1, "FitXy")]
		[InlineData(1, ExtremeAspectRatioMultiplier, "FitXy")]
		[InlineData(ExtremeAspectRatioMultiplier, 1, "Center")]
		[InlineData(1, ExtremeAspectRatioMultiplier, "Center")]
		public async Task LoadDrawableAsyncLimitsExtremeAspectLargeFileUrisToDisplaySize(int widthMultiplier, int heightMultiplier, string scaleTypeName)
		{
			var metrics = MauiProgram.DefaultContext?.Resources?.DisplayMetrics;
			Assert.NotNull(metrics);

			var expectedColor = Color.FromArgb("#FF0000").ToPlatform();
			var sourceWidth = Math.Max(1, metrics.WidthPixels * widthMultiplier);
			var sourceHeight = Math.Max(1, metrics.HeightPixels * heightMultiplier);
			var filename = BaseImageSourceServiceTests.CreateBitmapFile(sourceWidth, sourceHeight, expectedColor);
			var imageSource = new UriImageSourceStub(new Uri($"file://{filename}"));
			var imageView = new ImageView(MauiProgram.DefaultContext);
			imageView.SetScaleType(scaleTypeName switch
			{
				"FitXy" => ImageView.ScaleType.FitXy,
				"Center" => ImageView.ScaleType.Center,
				_ => ImageView.ScaleType.CenterCrop,
			});

			var service = new UriImageSourceService();
			await imageView.AttachAndRun(async () =>
			{
				using var result = await service.LoadDrawableAsync(imageSource, imageView);
				var bitmapDrawable = Assert.IsType<BitmapDrawable>(imageView.Drawable);
				var bitmap = bitmapDrawable.Bitmap;

				Assert.NotNull(result);
				Assert.True(bitmap.Width <= metrics.WidthPixels, $"Expected bitmap width {bitmap.Width} to be <= display width {metrics.WidthPixels} for scale type {scaleTypeName}.");
				Assert.True(bitmap.Height <= metrics.HeightPixels, $"Expected bitmap height {bitmap.Height} to be <= display height {metrics.HeightPixels} for scale type {scaleTypeName}.");
			});
		}
	}
}
