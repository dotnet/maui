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
		public async Task UpdatingSourceWorks()
		{
			var image = new TStub
			{
				Background = new SolidPaintStub(Colors.Black),
				Source = new FileImageSourceStub("red.png"),
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<CountedImageHandler>(image);
				await image.WaitUntilLoaded();
				await handler.PlatformView.AssertContainsColor(Colors.Red, MauiContext);

				image.Source = new FileImageSourceStub("blue.png");
				handler.UpdateValue(nameof(IImage.Source));
				await image.WaitUntilLoaded();
				await handler.PlatformView.AssertContainsColor(Colors.Blue, MauiContext);
			});
		}

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

				await handler.PlatformView.AttachAndRun(async () =>
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

					// now try without awaiting (Glide cache should return the same drawable immediately)
					var thirdResultTask = service.LoadDrawableAsync(imageSource, handler.PlatformView);
					var fourthResultTask = service.LoadDrawableAsync(imageSource, handler.PlatformView);

					// make sure we wait, but only for 5 seconds
					await Task.WhenAny(
						Task.WhenAll(thirdResultTask, fourthResultTask),
						Task.Delay(5_000));

					Assert.Equal(TaskStatus.RanToCompletion, thirdResultTask.Status);
					Assert.Equal(TaskStatus.RanToCompletion, fourthResultTask.Status);
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

				await handler.PlatformView.AttachAndRun(async () =>
				{
					var result = await service.LoadDrawableAsync(imageSource, handler.PlatformView);

					await handler.PlatformView.AssertColorAtCenterAsync(expectedColor.ToPlatform(), MauiContext);
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