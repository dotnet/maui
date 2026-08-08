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
		public async Task IconColorChangeDoesNotReloadImageSource()
		{
			var imageService = new CountingFileImageSourceService();
			EnsureHandlerCreated(builder => builder.ConfigureImageSources(
				services => services.AddService<IFileImageSource>(_ => imageService)));

			await InvokeOnMainThreadAsync(async () =>
			{
				var item = new SwipeItemMenuItemStub();
				var handler = CreateHandler<SwipeItemMenuItemHandler>(item);
				handler.PlatformView.Frame = new CGRect(0, 0, 100, 100);
				item.Source = new FileImageSourceStub("custom.png");

				await SwipeItemMenuItemHandler.MapSourceAsync(handler, item);
				Assert.Equal(1, imageService.LoadCount);
				var originalSize = handler.PlatformView.ImageForState(UIControlState.Normal).Size;

				item.IconColor = Colors.Red;
				handler.UpdateValue(nameof(ISwipeItemMenuItemIconColor.IconColor));

				Assert.Equal(1, imageService.LoadCount);
				Assert.Equal(Colors.Red, handler.PlatformView.TintColor.ToColor());
				Assert.Equal(originalSize, handler.PlatformView.ImageForState(UIControlState.Normal).Size);
			});
		}

		sealed class CountingFileImageSourceService : IImageSourceService<IFileImageSource>
		{
			readonly UIImage _image = UIImage.GetSystemImage("trash");

			public int LoadCount { get; private set; }

			public Task<IImageSourceServiceResult<UIImage>> GetImageAsync(
				IImageSource imageSource,
				float scale = 1,
				CancellationToken cancellationToken = default)
			{
				LoadCount++;
				return Task.FromResult<IImageSourceServiceResult<UIImage>>(
					new ImageSourceServiceResult(_image));
			}
		}
	}
}
