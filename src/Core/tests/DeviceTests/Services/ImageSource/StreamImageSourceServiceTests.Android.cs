using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
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
		public async Task LoadDrawableAsyncDoesNotShowAStreamThatArrivesAfterCancellation()
		{
			var service = new StreamImageSourceService();
			using var cts = new CancellationTokenSource();
			var imageSource = new StreamImageSourceStub(_ =>
			{
				// The stream ignores cancellation and arrives after the load was cancelled (for example replaced by a newer load).
				cts.Cancel();
				return Task.FromResult(CreateBitmapStream(100, 100, Microsoft.Maui.Graphics.Colors.Red));
			});

			await InvokeOnMainThreadAsync(async () =>
			{
				var imageView = new ImageView(MauiProgram.DefaultContext)
				{
					LayoutParameters = new ViewGroup.LayoutParams(100, 100),
				};

				var load = service.LoadDrawableAsync(imageSource, imageView, cts.Token);
				await Task.WhenAny(load, Task.Delay(5_000));

				Assert.True(load.IsCanceled);
				Assert.Null(imageView.Drawable);
			});
		}
	}
}
