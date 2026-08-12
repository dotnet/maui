using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

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
					Text = "Delete"
				};
				var handler = CreateHandler<SwipeItemMenuItemHandler>(item);
				var defaultTitleColor = handler.PlatformView.CurrentTitleColor.ToColor();

				item.TextColor = Colors.Red;
				handler.UpdateValue(nameof(ITextStyle.TextColor));

				Assert.Equal(Colors.Red, handler.PlatformView.CurrentTitleColor.ToColor());

				item.TextColor = null;
				handler.UpdateValue(nameof(ITextStyle.TextColor));

				Assert.Equal(defaultTitleColor, handler.PlatformView.CurrentTitleColor.ToColor());
			});
		}

		[Fact]
		public async Task ColorlessFontIconUsesTemplateRenderingAndTracksTextColor()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var item = new SwipeItemMenuItemStub();
				var handler = CreateHandler<SwipeItemMenuItemHandler>(item);
				handler.PlatformView.Frame = new CGRect(0, 0, 100, 100);

				item.Source = new FontImageSourceStub
				{
					Glyph = "X",
					Font = Font.Default.WithSize(30)
				};

				await SwipeItemMenuItemHandler.MapSourceAsync(handler, item);

				var image = handler.PlatformView.ImageForState(UIControlState.Normal);
				Assert.NotNull(image);
				Assert.Equal(UIImageRenderingMode.AlwaysTemplate, image.RenderingMode);

				item.TextColor = Colors.Red;
				handler.UpdateValue(nameof(ITextStyle.TextColor));

				await AssertEventually(() => handler.PlatformView.TintColor?.ToColor() == Colors.Red);
			});
		}

		[Fact]
		public async Task IconTintCanBeChangedAndCleared()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var item = new SwipeItemMenuItemStub
				{
					IconColor = Colors.Blue
				};
				var handler = CreateHandler<SwipeItemMenuItemHandler>(item);
				handler.PlatformView.Frame = new CGRect(0, 0, 100, 100);
				item.Source = new FileImageSourceStub("red.png");

				await SwipeItemMenuItemHandler.MapSourceAsync(handler, item);
				await AssertEventually(() =>
					handler.PlatformView.ImageForState(UIControlState.Normal)?.RenderingMode == UIImageRenderingMode.AlwaysTemplate &&
					handler.PlatformView.TintColor?.ToColor() == Colors.Blue);

				item.IconColor = Colors.Red;
				handler.UpdateValue(nameof(ISwipeItemMenuItemIconColor.IconColor));
				await AssertEventually(() => handler.PlatformView.TintColor?.ToColor() == Colors.Red);

				item.IconColor = null;
				handler.UpdateValue(nameof(ISwipeItemMenuItemIconColor.IconColor));
				await AssertEventually(() =>
					handler.PlatformView.ImageForState(UIControlState.Normal)?.RenderingMode == UIImageRenderingMode.AlwaysOriginal);
			});
		}

		[Fact]
		public async Task IconColorLoadsSourceWhenPlatformImageIsMissing()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var item = new SwipeItemMenuItemStub();
				var handler = CreateHandler<SwipeItemMenuItemHandler>(item);
				handler.PlatformView.Frame = new CGRect(0, 0, 100, 100);

				item.Source = new FileImageSourceStub("red.png");
				item.IconColor = Colors.Blue;
				handler.UpdateValue(nameof(ISwipeItemMenuItemIconColor.IconColor));

				await AssertEventually(() =>
					handler.PlatformView.ImageForState(UIControlState.Normal) is not null &&
					handler.PlatformView.TintColor?.ToColor() == Colors.Blue);
			});
		}

		[Fact]
		public async Task IconColorChangeDoesNotReloadOrRedrawImage()
		{
			var imageService = new CountingFileImageSourceService();
			EnsureHandlerCreated(builder => builder.ConfigureImageSources(
				services => services.AddService<IFileImageSource>(_ => imageService)));

			await InvokeOnMainThreadAsync(async () =>
			{
				var item = new SwipeItemMenuItemStub();
				var handler = CreateHandler<SwipeItemMenuItemHandler>(item);
				handler.PlatformView.Frame = new CGRect(0, 0, 100.2, 100.2);
				item.Source = new FileImageSourceStub("custom.png");

				await SwipeItemMenuItemHandler.MapSourceAsync(handler, item);
				Assert.Equal(1, imageService.LoadCount);
				var originalImage = handler.PlatformView.ImageForState(UIControlState.Normal);
				var originalSize = originalImage.Size;
				var originalImageHandle = originalImage.CGImage.Handle;

				item.IconColor = Colors.Red;
				handler.UpdateValue(nameof(ISwipeItemMenuItemIconColor.IconColor));

				Assert.Equal(1, imageService.LoadCount);
				Assert.Equal(Colors.Red, handler.PlatformView.TintColor.ToColor());
				Assert.Equal(originalSize, handler.PlatformView.ImageForState(UIControlState.Normal).Size);
				Assert.Equal(originalImageHandle, handler.PlatformView.ImageForState(UIControlState.Normal).CGImage.Handle);
			});
		}

		[Fact]
		public async Task ScaleOneImageJustOverLimitIsResized()
		{
			var imageService = new CountingFileImageSourceService(
				CountingFileImageSourceService.CreateImage(51, 1));
			EnsureHandlerCreated(builder => builder.ConfigureImageSources(
				services => services.AddService<IFileImageSource>(_ => imageService)));

			await InvokeOnMainThreadAsync(async () =>
			{
				var item = new SwipeItemMenuItemStub();
				var handler = CreateHandler<SwipeItemMenuItemHandler>(item);
				handler.PlatformView.Frame = new CGRect(0, 0, 100, 100);
				item.Source = new FileImageSourceStub("custom.png");

				await SwipeItemMenuItemHandler.MapSourceAsync(handler, item);

				var resizedImage = handler.PlatformView.ImageForState(UIControlState.Normal);
				Assert.NotNull(resizedImage);
				Assert.True(resizedImage.Size.Width <= 50);
				Assert.True(resizedImage.Size.Height <= 50);
				Assert.NotEqual(imageService.SourceImage.CGImage.Handle, resizedImage.CGImage.Handle);
			});
		}

		sealed class CountingFileImageSourceService : IImageSourceService<IFileImageSource>
		{
			public CountingFileImageSourceService()
				: this(CreateImage(200, UIScreen.MainScreen.Scale))
			{
			}

			public CountingFileImageSourceService(UIImage image)
			{
				SourceImage = image;
			}

			public int LoadCount { get; private set; }

			public UIImage SourceImage { get; }

			public Task<IImageSourceServiceResult<UIImage>> GetImageAsync(
				IImageSource imageSource,
				float scale = 1,
				CancellationToken cancellationToken = default)
			{
				LoadCount++;
				return Task.FromResult<IImageSourceServiceResult<UIImage>>(
					new ImageSourceServiceResult(SourceImage));
			}

			public static UIImage CreateImage(nfloat size, nfloat scale)
			{
				var bounds = new CGRect(0, 0, size, size);
				var format = new UIGraphicsImageRendererFormat
				{
					Opaque = false,
					Scale = scale
				};
				using var renderer = new UIGraphicsImageRenderer(bounds.Size, format);

				return renderer.CreateImage(context =>
				{
					UIColor.Red.SetFill();
					context.FillRect(bounds);
				});
			}
		}
	}
}
