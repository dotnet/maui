using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageLoadingTests
	{
		const int LargeWidth = 1200;
		const int LargeHeight = 800;

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task LargeImagesAreDecodedAtTheSizeTheyAreShownAt(bool remote)
		{
			SetupBuilder();
			var png = CreateLargePng();
			_server.Respond(_ => LargePng(png));

			var image = CreateSizedImage(remote ? new UriImageSource { Uri = RedUri } : CreateLargeFile(png), 60, 40, Aspect.AspectFit);

			await RunWithImages(async () =>
			{
				await WaitFor(() => image.LoadingState == ImageLoadingState.Loaded);
				var uiImage = GetUIImage(image);

				// Decoded for 60x40 points (at the screen's scale), not 1200x800 pixels...
				var density = image.ToPlatform().Window.GetDisplayDensity();
				Assert.InRange(uiImage.CGImage.Width, 1, (nint)Math.Ceiling(60 * density) + 1);

				// ...but the image keeps its size in points, so layout does not change.
				Assert.Equal(LargeWidth, uiImage.Size.Width, 1);
				Assert.Equal(LargeHeight, uiImage.Size.Height, 1);
				await AssertColor(image, Colors.Red);
			}, image);
		}

		[Fact]
		public async Task ImagesShownAtTheirOwnSizeAreNotDecodedSmaller()
		{
			SetupBuilder();
			var image = CreateSizedImage(CreateLargeFile(CreateLargePng()), 60, 40, Aspect.Center);

			await RunWithImages(async () =>
			{
				await WaitFor(() => image.LoadingState == ImageLoadingState.Loaded);
				Assert.Equal(LargeWidth, GetUIImage(image).CGImage.Width);
			}, image);
		}

		[Fact]
		public async Task ImageIsDecodedAgainWhenTheViewGrows()
		{
			SetupBuilder();
			var png = CreateLargePng();
			_server.Respond(_ => LargePng(png));
			var image = CreateSizedImage(new UriImageSource { Uri = RedUri }, 30, 20, Aspect.AspectFit);
			var loaded = 0;
			image.ImageLoaded += (_, _) => loaded++;

			await RunWithImages(async () =>
			{
				await WaitFor(() => image.LoadingState == ImageLoadingState.Loaded);
				var smallWidth = GetUIImage(image).CGImage.Width;

				image.WidthRequest = 300;
				image.HeightRequest = 200;

				// Replaced by a sharper image without reloading it.
				await WaitFor(() => GetUIImage(image).CGImage.Width > smallWidth * 4);
				Assert.Equal(LargeWidth, GetUIImage(image).Size.Width, 1);
				Assert.Equal(1, loaded);
				Assert.Equal(ImageLoadingState.Loaded, image.LoadingState);
				Assert.Equal(1, _server.RequestCount);
			}, image);
		}

		[Fact]
		public async Task StreamImagesAreOnlyDecodedSmallerWhenTheirSizeDoesNotDependOnTheImage()
		{
			SetupBuilder();
			var png = CreateLargePng();
			var autoSized = CreateImage(ImageSource.FromStream(() => new MemoryStream(png)));
			autoSized.WidthRequest = -1;
			autoSized.HeightRequest = -1;
			autoSized.Aspect = Aspect.AspectFit;
			var fixedSize = CreateSizedImage(ImageSource.FromStream(() => new MemoryStream(png)), 60, 40, Aspect.AspectFit);

			await RunWithImages(async () =>
			{
				await WaitFor(() => autoSized.LoadingState == ImageLoadingState.Loaded && fixedSize.LoadingState == ImageLoadingState.Loaded);

				// A stream cannot be decoded again when the view turns out larger, so it is only decoded smaller at a fixed size.
				Assert.Equal(LargeWidth, GetUIImage(autoSized).CGImage.Width);
				Assert.True(GetUIImage(fixedSize).CGImage.Width < LargeWidth);
			}, autoSized, fixedSize);
		}

		[Fact]
		public async Task ImagesThatAreNotCachedAreNotDownloadedAgainWhenTheViewGrows()
		{
			SetupBuilder();
			var png = CreateLargePng();
			_server.Respond(_ => LargePng(png));
			var image = CreateSizedImage(new UriImageSource { Uri = RedUri, CachingEnabled = false }, 30, 20, Aspect.AspectFit);

			await RunWithImages(async () =>
			{
				await WaitFor(() => image.LoadingState == ImageLoadingState.Loaded);

				image.WidthRequest = 300;
				image.HeightRequest = 200;
				await WaitFor(() => image.Width == 300);
				await Task.Delay(500);

				// Downloading it again could return another image.
				Assert.Equal(1, _server.RequestCount);
				await AssertColor(image, Colors.Red);
			}, image);
		}

		[Fact]
		public async Task DerivedImageSourceServicesLoadTheImageThemselves()
		{
			var service = default(DerivedUriImageSourceService);
			SetupBuilder(svcs => service = new DerivedUriImageSourceService(ImageSourcesMauiAppBuilderExtensions.GetUriImageLoader(svcs)));
			var png = CreateLargePng();
			_server.Respond(_ => LargePng(png));
			var image = CreateSizedImage(new UriImageSource { Uri = RedUri }, 60, 40, Aspect.AspectFit);

			await RunWithImages(async () =>
			{
				await WaitFor(() => image.LoadingState == ImageLoadingState.Loaded);
				Assert.Equal(1, service.Calls);
			}, image);
		}

		class DerivedUriImageSourceService : UriImageSourceService
		{
			internal DerivedUriImageSourceService(UriImageLoader loader)
				: base(null, loader)
			{
			}

			public int Calls { get; private set; }

			public override Task<IImageSourceServiceResult<UIImage>> GetImageAsync(IImageSource imageSource, float scale = 1, System.Threading.CancellationToken cancellationToken = default)
			{
				Calls++;
				return base.GetImageAsync(imageSource, scale, cancellationToken);
			}
		}

		Image CreateSizedImage(ImageSource source, double width, double height, Aspect aspect)
		{
			var image = CreateImage(source);
			image.WidthRequest = width;
			image.HeightRequest = height;
			image.Aspect = aspect;
			return image;
		}

		static UIImage GetUIImage(Image image) =>
			((UIImageView)image.Handler.PlatformView).Image;

		static HttpResponseMessage LargePng(byte[] png)
		{
			var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(png) };
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
			return response;
		}

		static FileImageSource CreateLargeFile(byte[] png)
		{
			var path = Path.Combine(Path.GetTempPath(), $"large-{Guid.NewGuid():N}.png");
			File.WriteAllBytes(path, png);
			return new FileImageSource { File = path };
		}

		/// <summary>Creates a red PNG of 1200x800 pixels.</summary>
		static byte[] CreateLargePng()
		{
			var format = new UIGraphicsImageRendererFormat { Scale = 1 };
			var renderer = new UIGraphicsImageRenderer(new CGSize(LargeWidth, LargeHeight), format);
			using var image = renderer.CreateImage(context =>
			{
				UIColor.Red.SetFill();
				context.FillRect(new CGRect(0, 0, LargeWidth, LargeHeight));
			});
			return image.AsPNG().ToArray();
		}
	}
}
