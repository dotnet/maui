using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Xunit;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.SwipeView)]
	public class SwipeItemMenuItemHandlerTests : CoreHandlerTestBase
	{
		[Fact]
		public Task TextColorCanBeCleared()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var item = new SwipeItemMenuItemStub
				{
					Text = "Delete",
					TextColor = Colors.Red
				};
				var handler = CreateHandler<SwipeItemMenuItemHandler>(item);

				Assert.NotSame(
					DependencyProperty.UnsetValue,
					handler.PlatformView.ReadLocalValue(SwipeItem.ForegroundProperty));

				item.TextColor = null;
				handler.UpdateValue(nameof(ITextStyle.TextColor));

				Assert.Same(
					DependencyProperty.UnsetValue,
					handler.PlatformView.ReadLocalValue(SwipeItem.ForegroundProperty));
			});
		}

		[Fact]
		public Task TintedPackagedFileUsesFlattenedMauiAssetName()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var source = new FileImageSourceStub("Resources/Images/delete.png");
				var service = MauiContext.Services
					.GetRequiredService<IImageSourceServiceProvider>()
					.GetRequiredImageSourceService(source);
				var icon = Assert.IsType<BitmapIconSource>(
					SwipeItemMenuItemHandler.CreateTintedIconSource(
						source,
						service,
						MauiContext));

				Assert.Equal(new Uri("ms-appx:///delete.png"), icon.UriSource);
			});
		}

		[Fact]
		public Task TintedRootedFileFallsBackToUntintedLoader()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var source = new FileImageSourceStub(@"C:\images\delete.png");
				var service = MauiContext.Services
					.GetRequiredService<IImageSourceServiceProvider>()
					.GetRequiredImageSourceService(source);
				var icon = SwipeItemMenuItemHandler.CreateTintedIconSource(
					source,
					service,
					MauiContext);

				Assert.Null(icon);
			});
		}

		[Fact]
		public async Task CustomFileImageServiceKeepsIconWhenTintIsRequested()
		{
			var imageService = new DelayedFileImageSourceService();
			EnsureHandlerCreated(builder => builder.ConfigureImageSources(
				services => services.AddService<IFileImageSource>(_ => imageService)));

			await InvokeOnMainThreadAsync(async () =>
			{
				var item = new SwipeItemMenuItemStub
				{
					IconColor = Colors.Red
				};
				var handler = CreateHandler<SwipeItemMenuItemHandler>(item);
				item.Source = new FileImageSourceStub("custom-relative-path.png");

				var load = SwipeItemMenuItemHandler.MapSourceAsync(handler, item);
				var request = imageService.Requests.Dequeue();
				var image = new BitmapImage();
				request.SetResult(new ImageSourceServiceResult(image));

				await load;

				var icon = Assert.IsType<ImageIconSource>(handler.PlatformView.IconSource);
				Assert.Same(image, icon.ImageSource);
			});
		}

		[Fact]
		public async Task StaleSameSourceLoadCannotOverwriteNewerTintChange()
		{
			var imageService = new DelayedFileImageSourceService();
			EnsureHandlerCreated(builder => builder.ConfigureImageSources(
				services => services.AddService<IFileImageSource>(_ => imageService)));

			await InvokeOnMainThreadAsync(async () =>
			{
				var source = new FileImageSourceStub("custom-relative-path.png");
				var item = new SwipeItemMenuItemStub();
				var handler = CreateHandler<SwipeItemMenuItemHandler>(item);
				item.Source = source;

				var staleLoad = SwipeItemMenuItemHandler.MapSourceAsync(handler, item);
				var initialRequest = imageService.Requests.Dequeue();

				item.IconColor = Colors.Blue;
				var newerLoad = SwipeItemMenuItemHandler.MapSourceAsync(handler, item);
				var newerRequest = imageService.Requests.Dequeue();
				var newerImage = new BitmapImage();
				newerRequest.SetResult(new ImageSourceServiceResult(newerImage));
				await newerLoad;

				var currentIcon = Assert.IsType<ImageIconSource>(handler.PlatformView.IconSource);
				Assert.Same(newerImage, currentIcon.ImageSource);

				var staleImage = new BitmapImage();
				initialRequest.SetResult(new ImageSourceServiceResult(staleImage));
				await staleLoad;

				currentIcon = Assert.IsType<ImageIconSource>(handler.PlatformView.IconSource);
				Assert.Same(newerImage, currentIcon.ImageSource);
			});
		}

		sealed class DelayedFileImageSourceService : IImageSourceService<IFileImageSource>
		{
			public Queue<TaskCompletionSource<IImageSourceServiceResult<WImageSource>?>> Requests { get; } = new();

			public Task<IImageSourceServiceResult<WImageSource>?> GetImageSourceAsync(
				IImageSource imageSource,
				float scale = 1,
				CancellationToken cancellationToken = default)
			{
				var completion = new TaskCompletionSource<IImageSourceServiceResult<WImageSource>?>(
					TaskCreationOptions.RunContinuationsAsynchronously);
				Requests.Enqueue(completion);
				return completion.Task;
			}
		}
	}
}
