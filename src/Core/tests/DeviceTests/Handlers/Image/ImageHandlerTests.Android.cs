using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Widget;
using Google.Android.Material.ImageView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageHandlerTests<TImageHandler, TStub>
	{
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task FailingLoadOnlyReportsFailureWhenNotCancelled(bool disconnectHandler)
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var image = new TStub();
				var handler = CreateHandler<CountedImageHandler>(image);
				var platformView = handler.PlatformView;
				var imageEvents = handler.ImageEvents;
				using var previousDrawable = new ColorDrawable(Colors.Red.ToPlatform());
				platformView.SetImageDrawable(previousDrawable);
				imageEvents.Clear();

				var provider = handler.Services.GetRequiredService<IImageSourceServiceProvider>();
				var service = Assert.IsType<CountedImageSourceServiceStub>(
					provider.GetRequiredImageSourceService<CountedImageSourceStub>());
				var failure = new InvalidOperationException("The image service failed.");
				service.LoadException = failure;

				var events = new List<string>();
				Exception reportedFailure = null;
				image.LoadingStarted += () => events.Add("LoadingStarted");
				image.LoadingCompleted += successful => events.Add($"LoadingCompleted({successful})");
				image.LoadingFailed += exception =>
				{
					reportedFailure = exception;
					events.Add("LoadingFailed");
				};

				image.Source = new CountedImageSourceStub(Colors.Blue, true);
				var loadTask = handler.SourceLoader.UpdateImageSourceAsync();

				try
				{
					Assert.True(await Task.Run(() => service.Starting.WaitOne(TimeSpan.FromSeconds(5))),
						"The image service did not start.");
					Assert.True(service.LoadCancellationToken.CanBeCanceled);
					Assert.False(service.LoadCancellationToken.IsCancellationRequested);
					Assert.True(image.IsLoading);

					if (disconnectHandler)
						((IElementHandler)handler).DisconnectHandler();

					Assert.Equal(disconnectHandler, service.LoadCancellationToken.IsCancellationRequested);
				}
				finally
				{
					service.DoWork.Set();
					await loadTask.WaitAsync(TimeSpan.FromSeconds(5));
				}

				Assert.False(image.IsLoading);
				if (disconnectHandler)
				{
					Assert.Equal(new[] { "LoadingStarted", "LoadingCompleted(False)" }, events);
					Assert.Null(reportedFailure);
					Assert.Empty(imageEvents);
					Assert.Same(previousDrawable, platformView.Drawable);
				}
				else
				{
					Assert.Equal(new[] { "LoadingStarted", "LoadingFailed" }, events);
					Assert.Same(failure, reportedFailure);
					var imageEvent = Assert.Single(imageEvents);
					Assert.Equal("SetImageDrawable", imageEvent.Member);
					Assert.Null(imageEvent.Value);
					Assert.Null(platformView.Drawable);
				}
			});
		}

		[Fact]
		public async Task CancelledLoadThatFailsDoesNotClearReplacementImage()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var image = new TStub();
				var handler = CreateHandler<CountedImageHandler>(image);
				var provider = handler.Services.GetRequiredService<IImageSourceServiceProvider>();
				var service = Assert.IsType<CountedImageSourceServiceStub>(
					provider.GetRequiredImageSourceService<CountedImageSourceStub>());
				service.LoadException = new InvalidOperationException("The old image service failed.");

				var events = new List<string>();
				image.LoadingStarted += () => events.Add("LoadingStarted");
				image.LoadingCompleted += successful => events.Add($"LoadingCompleted({successful})");
				image.LoadingFailed += exception => events.Add($"LoadingFailed({exception.Message})");

				image.Source = new CountedImageSourceStub(Colors.Blue, true);
				var loadTask = handler.SourceLoader.UpdateImageSourceAsync();
				Drawable replacementDrawable = null;

				try
				{
					Assert.True(await Task.Run(() => service.Starting.WaitOne(TimeSpan.FromSeconds(5))),
						"The image service did not start.");
					var cancelledToken = service.LoadCancellationToken;
					Assert.True(cancelledToken.CanBeCanceled);
					Assert.False(cancelledToken.IsCancellationRequested);

					// Finish the replacement before allowing the old load to fail.
					service.LoadException = null;
					image.Source = new CountedImageSourceStub(Colors.Red);
					await handler.SourceLoader.UpdateImageSourceAsync().WaitAsync(TimeSpan.FromSeconds(5));
					Assert.True(cancelledToken.IsCancellationRequested);
					var drawable = Assert.IsType<ColorDrawable>(handler.PlatformView.Drawable);
					Assert.Equal(Colors.Red.ToPlatform(), drawable.Color);
					replacementDrawable = drawable;
					handler.ImageEvents.Clear();
				}
				finally
				{
					service.DoWork.Set();
					await loadTask.WaitAsync(TimeSpan.FromSeconds(5));
				}

				Assert.Equal(new[]
				{
					"LoadingStarted", "LoadingStarted", "LoadingCompleted(True)", "LoadingCompleted(False)"
				}, events);
				Assert.False(image.IsLoading);
				Assert.Empty(handler.ImageEvents);
				Assert.Same(replacementDrawable, handler.PlatformView.Drawable);
			});
		}

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

		[Fact]
		public async Task ImageHandler2PlatformViewDoesNotUseHardwareLayer()
		{
			var image = new TStub();

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<ImageHandler2>(image);
				var platformView = Assert.IsType<ShapeableImageView>(handler.PlatformView);

				Assert.Equal(global::Android.Views.LayerType.None, platformView.LayerType);
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