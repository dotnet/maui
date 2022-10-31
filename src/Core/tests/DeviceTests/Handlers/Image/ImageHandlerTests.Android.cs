using System;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageHandlerTests<TImageHandler, TStub>
	{
		[Fact]
		public async Task LoadDrawableAsyncReturnsWithSameImageAndDoesNotHang()
		{
			var service = new FileImageSourceService();

			var filename = BaseImageSourceServiceTests.CreateBitmapFile(100, 100, Colors.Azure);
			var imageSource = new FileImageSourceStub(filename);

			var image = new TStub();

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<TImageHandler>(image);

				await handler.PlatformView.AttachAndRunAsync(async () =>
				{
					// get the file to load for the first time
					var firstResult = await service.LoadDrawableAsync(imageSource, handler.PlatformView);

					// now load and make sure the task completes
					var secondResultTask = service.LoadDrawableAsync(imageSource, handler.PlatformView);

					// make sure we wait, but only for 5 seconds
					await Task.WhenAny(
						secondResultTask,
						Task.Delay(5_000));

					Assert.Equal(TaskStatus.RanToCompletion, secondResultTask.Status);
				});
			});
		}

		[Theory]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#000000")]
		public async Task LoadDrawableAsyncWithFileLoadsFileInsteadOfResource(string colorHex)
		{
			var expectedColor = Color.FromArgb(colorHex);

			var service = new FileImageSourceService();

			var filename = BaseImageSourceServiceTests.CreateBitmapFile(100, 100, expectedColor, "blue.png");
			var imageSource = new FileImageSourceStub(filename);

			var image = new TStub();

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<TImageHandler>(image);

				await handler.PlatformView.AttachAndRunAsync(async () =>
				{
					var result = await service.LoadDrawableAsync(imageSource, handler.PlatformView);

					await handler.PlatformView.AssertColorAtCenterAsync(expectedColor.ToPlatform());
				});
			});
		}

		ImageView GetPlatformImageView(IImageHandler imageHandler) =>
			imageHandler.PlatformView;

		bool GetNativeIsAnimationPlaying(IImageHandler imageHandler) =>
			GetPlatformImageView(imageHandler).Drawable is IAnimatable animatable && animatable.IsRunning;

		Aspect GetNativeAspect(IImageHandler imageHandler)
		{
			var scaleType = GetPlatformImageView(imageHandler).GetScaleType();
			if (scaleType == ImageView.ScaleType.Center)
				return Aspect.Center;
			if (scaleType == ImageView.ScaleType.CenterCrop)
				return Aspect.AspectFill;
			if (scaleType == ImageView.ScaleType.FitCenter)
				return Aspect.AspectFit;
			if (scaleType == ImageView.ScaleType.FitXy)
				return Aspect.Fill;

			throw new ArgumentOutOfRangeException("Aspect");
		}
	}
}