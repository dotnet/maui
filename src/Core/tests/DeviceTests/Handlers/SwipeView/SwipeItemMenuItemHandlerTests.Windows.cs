#nullable enable
using System;
using System.Threading;
using System.Threading.Channels;
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
using WSwipeItem = Microsoft.UI.Xaml.Controls.SwipeItem;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.SwipeView)]
	public class SwipeItemMenuItemHandlerTests : CoreHandlerTestBase
	{
		static readonly TimeSpan ImageLoadTimeout = TimeSpan.FromSeconds(5);

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
						service));

				Assert.Equal(new Uri("ms-appx:///delete.png"), icon.UriSource);
				Assert.True(icon.ShowAsMonochrome);
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
					service);

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
				var request = await imageService.Requests.DequeueAsync();
				var image = new BitmapImage();
				request.SetResult(new ImageSourceServiceResult(image));

				await load.WaitAsync(ImageLoadTimeout);

				var icon = Assert.IsType<ImageIconSource>(handler.PlatformView.IconSource);
				Assert.Same(image, icon.ImageSource);

				item.IconColor = Colors.Blue;
				handler.UpdateValue(nameof(ISwipeItemMenuItemIconColor.IconColor));

				Assert.True(imageService.Requests.IsEmpty);
				Assert.Same(icon, handler.PlatformView.IconSource);
			});
		}

		[Fact]
		public async Task ImageServiceResultsAreDisposedWhenReplacedAndDisconnected()
		{
			var imageService = new DelayedFileImageSourceService();
			EnsureHandlerCreated(builder => builder.ConfigureImageSources(
				services => services.AddService<IFileImageSource>(_ => imageService)));

			await InvokeOnMainThreadAsync(async () =>
			{
				var item = new SwipeItemMenuItemStub();
				var handler = CreateHandler<SwipeItemMenuItemHandler>(item);

				item.Source = new FileImageSourceStub("first.png");
				var firstLoad = SwipeItemMenuItemHandler.MapSourceAsync(handler, item);
				var firstRequest = await imageService.Requests.DequeueAsync();
				var firstDisposeCount = 0;
				firstRequest.SetResult(new ImageSourceServiceResult(
					new BitmapImage(),
					() => firstDisposeCount++));
				await firstLoad.WaitAsync(ImageLoadTimeout);

				Assert.Equal(0, firstDisposeCount);

				item.Source = new FileImageSourceStub("second.png");
				var secondLoad = SwipeItemMenuItemHandler.MapSourceAsync(handler, item);
				var secondRequest = await imageService.Requests.DequeueAsync();
				var secondDisposeCount = 0;

				Assert.Equal(1, firstDisposeCount);

				secondRequest.SetResult(new ImageSourceServiceResult(
					new BitmapImage(),
					() => secondDisposeCount++));
				await secondLoad.WaitAsync(ImageLoadTimeout);

				Assert.Equal(0, secondDisposeCount);

				((IElementHandler)handler).DisconnectHandler();

				Assert.Equal(1, secondDisposeCount);
			});
		}

		[Fact]
		public async Task UriImageUsesRegisteredServiceWhenTintIsRequested()
		{
			var imageService = new DelayedUriImageSourceService();
			EnsureHandlerCreated(builder => builder.ConfigureImageSources(
				services => services.AddService<IUriImageSource>(_ => imageService)));

			await InvokeOnMainThreadAsync(async () =>
			{
				var item = new SwipeItemMenuItemStub
				{
					IconColor = Colors.Red
				};
				var handler = CreateHandler<SwipeItemMenuItemHandler>(item);
				// Keep the explicit map below as the only pending delayed load.
				item.Source = new UriImageSourceStub("https://example.com/delete.png");

				var load = SwipeItemMenuItemHandler.MapSourceAsync(handler, item);
				var request = await imageService.Requests.DequeueAsync();
				var image = new BitmapImage();
				request.SetResult(new ImageSourceServiceResult(image));

				await load.WaitAsync(ImageLoadTimeout);

				var icon = Assert.IsType<ImageIconSource>(handler.PlatformView.IconSource);
				Assert.Same(image, icon.ImageSource);

				item.IconColor = Colors.Blue;
				handler.UpdateValue(nameof(ISwipeItemMenuItemIconColor.IconColor));

				Assert.True(imageService.Requests.IsEmpty);
				Assert.Same(icon, handler.PlatformView.IconSource);
			});
		}

		[Fact]
		public async Task TintedFontImageUsesRegisteredService()
		{
			var imageService = new CapturingFontImageSourceService();
			EnsureHandlerCreated(builder => builder.ConfigureImageSources(
				services => services.AddService<IFontImageSource>(_ => imageService)));

			await InvokeOnMainThreadAsync(async () =>
			{
				var source = new FontImageSourceStub
				{
					Color = Colors.Green,
					Font = Font.Default,
					Glyph = "A"
				};
				var item = new SwipeItemMenuItemStub
				{
					IconColor = Colors.Red,
					Source = source
				};
				var handler = CreateHandler<SwipeItemMenuItemHandler>(item);

				await SwipeItemMenuItemHandler.MapSourceAsync(handler, item);

				var loadedSource = Assert.IsAssignableFrom<IFontImageSource>(imageService.LastSource);
				Assert.NotSame(source, loadedSource);
				Assert.Equal(Colors.Red, loadedSource.Color);

				var icon = Assert.IsType<ImageIconSource>(handler.PlatformView.IconSource);
				Assert.Same(imageService.Image, icon.ImageSource);
			});
		}

		[Fact]
		public async Task FontImageUsesOriginalSourceWhenResolvedTintMatchesSourceColor()
		{
			var imageService = new CapturingFontImageSourceService();
			EnsureHandlerCreated(builder => builder.ConfigureImageSources(
				services => services.AddService<IFontImageSource>(_ => imageService)));

			await InvokeOnMainThreadAsync(async () =>
			{
				var sourceColor = new Color(0.2f, 0.4f, 0.6f);
				var requestedColor = new Color(0.2f, 0.4f, 0.6f);
				var source = new FontImageSourceStub
				{
					Color = sourceColor,
					Font = Font.Default,
					Glyph = "A"
				};
				var item = new SwipeItemMenuItemStub
				{
					IconColor = requestedColor,
					Source = source
				};
				var handler = CreateHandler<SwipeItemMenuItemHandler>(item);

				await SwipeItemMenuItemHandler.MapSourceAsync(handler, item);

				Assert.NotSame(sourceColor, requestedColor);
				Assert.Equal(sourceColor, requestedColor);
				Assert.Same(source, imageService.LastSource);
				var icon = Assert.IsType<ImageIconSource>(handler.PlatformView.IconSource);
				Assert.Same(imageService.Image, icon.ImageSource);
			});
		}

		[Fact]
		public async Task ConcreteCustomFontImageUsesRegisteredServiceWhenTintIsRequested()
		{
			var imageService = new CapturingCustomFontImageSourceService();
			EnsureHandlerCreated(builder => builder.ConfigureImageSources(
				services => services.AddService<CustomFontImageSourceStub>(_ => imageService)));

			await InvokeOnMainThreadAsync(async () =>
			{
				var source = new CustomFontImageSourceStub
				{
					Font = Font.Default,
					Glyph = "A"
				};
				var item = new SwipeItemMenuItemStub
				{
					IconColor = Colors.Red,
					Source = source
				};
				var handler = CreateHandler<SwipeItemMenuItemHandler>(item);

				await SwipeItemMenuItemHandler.MapSourceAsync(handler, item);

				Assert.Same(source, imageService.LastSource);
				var icon = Assert.IsType<ImageIconSource>(handler.PlatformView.IconSource);
				Assert.Same(imageService.Image, icon.ImageSource);
			});
		}

		[Fact]
		public async Task DisconnectClearsFontImageLoadState()
		{
			var imageService = new CapturingFontImageSourceService();
			EnsureHandlerCreated(builder => builder.ConfigureImageSources(
				services => services.AddService<IFontImageSource>(_ => imageService)));

			await InvokeOnMainThreadAsync(async () =>
			{
				var source = new FontImageSourceStub
				{
					Color = Colors.Green,
					Font = Font.Default,
					Glyph = "A"
				};
				var item = new SwipeItemMenuItemStub
				{
					IconColor = Colors.Red,
					Source = source
				};
				var handler = CreateHandler<SwipeItemMenuItemHandler>(item);

				await SwipeItemMenuItemHandler.MapSourceAsync(handler, item);
				Assert.True(handler.IsFontIconLoadCurrent(source, Colors.Red));

				((IElementHandler)handler).DisconnectHandler();

				Assert.False(handler.IsFontIconLoadCurrent(source, Colors.Red));
			});
		}

		[Fact]
		public Task ExternalHandlerUsesGenerationTracking()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var item = new SwipeItemMenuItemStub();
				var handler = new ExternalSwipeItemMenuItemHandler();
				handler.SetMauiContext(MauiContext);
				handler.SetVirtualView(item);

				var staleGeneration = SwipeItemMenuItemHandler.BeginIconLoad(handler);
				var currentGeneration = SwipeItemMenuItemHandler.BeginIconLoad(handler);

				Assert.False(SwipeItemMenuItemHandler.IsIconLoadCurrent(
					handler,
					item,
					handler.PlatformView,
					staleGeneration));
				Assert.True(SwipeItemMenuItemHandler.IsIconLoadCurrent(
					handler,
					item,
					handler.PlatformView,
					currentGeneration));

				((IElementHandler)handler).DisconnectHandler();
			});
		}

		[Fact]
		public async Task FontImageReloadsOnlyWhenResolvedTintChanges()
		{
			var imageService = new CapturingFontImageSourceService();
			EnsureHandlerCreated(builder => builder.ConfigureImageSources(
				services => services.AddService<IFontImageSource>(_ => imageService)));

			await InvokeOnMainThreadAsync(async () =>
			{
				var item = new SwipeItemMenuItemStub
				{
					TextColor = Colors.Blue
				};
				var handler = CreateHandler<SwipeItemMenuItemHandler>(item);
				item.Source = new FontImageSourceStub
				{
					Font = Font.Default,
					Glyph = "A"
				};

				await SwipeItemMenuItemHandler.MapSourceAsync(handler, item);
				Assert.Equal(1, imageService.LoadCount);

				handler.UpdateValue(nameof(ITextStyle.TextColor));
				Assert.Equal(1, imageService.LoadCount);

				item.TextColor = Colors.Red;
				handler.UpdateValue(nameof(ITextStyle.TextColor));

				Assert.Equal(2, imageService.LoadCount);
				Assert.Equal(Colors.Red, Assert.IsAssignableFrom<IFontImageSource>(imageService.LastSource).Color);
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
				var initialRequest = await imageService.Requests.DequeueAsync();

				item.IconColor = Colors.Blue;
				var newerLoad = SwipeItemMenuItemHandler.MapSourceAsync(handler, item);
				var newerRequest = await imageService.Requests.DequeueAsync();
				var newerImage = new BitmapImage();
				var newerDisposeCount = 0;
				newerRequest.SetResult(new ImageSourceServiceResult(
					newerImage,
					() => newerDisposeCount++));
				await newerLoad.WaitAsync(ImageLoadTimeout);

				var currentIcon = Assert.IsType<ImageIconSource>(handler.PlatformView.IconSource);
				Assert.Same(newerImage, currentIcon.ImageSource);
				Assert.Equal(0, newerDisposeCount);

				var staleImage = new BitmapImage();
				var staleDisposeCount = 0;
				initialRequest.SetResult(new ImageSourceServiceResult(
					staleImage,
					() => staleDisposeCount++));
				await staleLoad.WaitAsync(ImageLoadTimeout);

				currentIcon = Assert.IsType<ImageIconSource>(handler.PlatformView.IconSource);
				Assert.Same(newerImage, currentIcon.ImageSource);
				Assert.Equal(1, staleDisposeCount);
				Assert.Equal(0, newerDisposeCount);

				((IElementHandler)handler).DisconnectHandler();

				Assert.Equal(1, newerDisposeCount);
			});
		}

		sealed class DelayedFileImageSourceService : IImageSourceService<IFileImageSource>
		{
			public ImageLoadRequests Requests { get; } = new();

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

		sealed class DelayedUriImageSourceService : IImageSourceService<IUriImageSource>
		{
			public ImageLoadRequests Requests { get; } = new();

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

		sealed class ImageLoadRequests
		{
			readonly Channel<TaskCompletionSource<IImageSourceServiceResult<WImageSource>?>> _requests =
				Channel.CreateUnbounded<TaskCompletionSource<IImageSourceServiceResult<WImageSource>?>>();

			public bool IsEmpty => !_requests.Reader.TryPeek(out _);

			public void Enqueue(TaskCompletionSource<IImageSourceServiceResult<WImageSource>?> request)
			{
				if (!_requests.Writer.TryWrite(request))
					throw new InvalidOperationException("Unable to enqueue the pending image load.");
			}

			public Task<TaskCompletionSource<IImageSourceServiceResult<WImageSource>?>> DequeueAsync() =>
				_requests.Reader.ReadAsync().AsTask().WaitAsync(ImageLoadTimeout);
		}

		sealed class CustomFontImageSourceStub : FontImageSourceStub
		{
		}

		sealed class CapturingCustomFontImageSourceService : IImageSourceService<CustomFontImageSourceStub>
		{
			public WImageSource Image { get; } = new BitmapImage();

			public IImageSource? LastSource { get; private set; }

			public Task<IImageSourceServiceResult<WImageSource>?> GetImageSourceAsync(
				IImageSource imageSource,
				float scale = 1,
				CancellationToken cancellationToken = default)
			{
				LastSource = imageSource;
				return Task.FromResult<IImageSourceServiceResult<WImageSource>?>(
					new ImageSourceServiceResult(Image));
			}
		}

		sealed class CapturingFontImageSourceService : IImageSourceService<IFontImageSource>
		{
			public WImageSource Image { get; } = new BitmapImage();

			public IImageSource? LastSource { get; private set; }

			public int LoadCount { get; private set; }

			public Task<IImageSourceServiceResult<WImageSource>?> GetImageSourceAsync(
				IImageSource imageSource,
				float scale = 1,
				CancellationToken cancellationToken = default)
			{
				LoadCount++;
				LastSource = imageSource;
				return Task.FromResult<IImageSourceServiceResult<WImageSource>?>(
					new ImageSourceServiceResult(Image));
			}
		}

		sealed class ExternalSwipeItemMenuItemHandler : ElementHandler<ISwipeItemMenuItem, WSwipeItem>, ISwipeItemMenuItemHandler
		{
			public ExternalSwipeItemMenuItemHandler()
				: base(new PropertyMapper<ISwipeItemMenuItem, ISwipeItemMenuItemHandler>())
			{
			}

			protected override WSwipeItem CreatePlatformElement() => new();
		}
	}
}
