using System;
using System.Net.Http;
using System.Threading.Tasks;
using Android.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageLoadingTests
	{
		static readonly Uri BlueUri = new("https://images.test/blue.png");

		[Fact]
		public async Task KeptImageIsRemovedFromTheViewWhenTheHandlerIsDisconnected()
		{
			SetupBuilder();
			_server.Respond(request => Png(request.RequestUri == BlueUri ? Colors.Blue : Colors.Red));
			var image = CreateImage(new UriImageSource { Uri = RedUri });
			image.KeepPreviousImageWhileLoading = true;
			var loaded = 0;
			image.ImageLoaded += (_, _) => loaded++;

			await RunWithImages(async () =>
			{
				await WaitFor(() => loaded == 1);

				// Loaded while keeping the previous image: the loader, not Glide, set it on the view.
				image.Source = new UriImageSource { Uri = BlueUri };
				await WaitFor(() => loaded == 2);
				var imageView = (ImageView)image.Handler.PlatformView;
				Assert.NotNull(imageView.Drawable);

				// Its bitmap is released when the handler is disconnected, so the view must not keep drawing it.
				image.Handler.DisconnectHandler();
				Assert.Null(imageView.Drawable);
			}, image);
		}

		[Fact]
		public async Task KeptImageIsClearedWhenKeepingIsTurnedOff()
		{
			SetupBuilder();
			var release = new TaskCompletionSource<bool>();
			_server.Respond(async request =>
			{
				if (request.RequestUri.AbsolutePath.EndsWith("slow.png", StringComparison.Ordinal))
					await release.Task;
				return Png(request.RequestUri == BlueUri ? Colors.Blue : Colors.Red);
			});
			var image = CreateImage(new UriImageSource { Uri = RedUri });
			image.KeepPreviousImageWhileLoading = true;
			var loaded = 0;
			image.ImageLoaded += (_, _) => loaded++;

			await RunWithImages(async () =>
			{
				await WaitFor(() => loaded == 1);
				image.Source = new UriImageSource { Uri = BlueUri };
				await WaitFor(() => loaded == 2);

				// Like any image on Android, it is cleared while the next image loads when it is not kept.
				image.KeepPreviousImageWhileLoading = false;
				image.Source = new UriImageSource { Uri = new Uri("https://images.test/slow.png") };
				await WaitFor(() => image.LoadingState == ImageLoadingState.Loading);
				Assert.Null(((ImageView)image.Handler.PlatformView).Drawable);

				release.SetResult(true);
				await WaitFor(() => loaded == 3);
			}, image);
		}

		[Fact]
		public async Task ErrorImageIsClearedWhenTheNextImageStartsLoading()
		{
			SetupBuilder();
			var release = new TaskCompletionSource<bool>();
			_server.Respond(async request =>
			{
				if (request.RequestUri.AbsolutePath.EndsWith("slow.png", StringComparison.Ordinal))
				{
					await release.Task;
					return Png(Colors.Blue);
				}
				return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
			});
			var image = CreateImage(new UriImageSource { Uri = RedUri });
			image.ErrorSource = "green.png";

			await RunWithImages(async () =>
			{
				await WaitFor(() => image.LoadingState == ImageLoadingState.Failed);
				await AssertColor(image, Colors.Lime);

				// Like any image on Android, the error image is cleared while the next image loads.
				image.Source = new UriImageSource { Uri = new Uri("https://images.test/slow.png") };
				await WaitFor(() => image.LoadingState == ImageLoadingState.Loading);
				Assert.Null(((ImageView)image.Handler.PlatformView).Drawable);

				release.SetResult(true);
				await WaitFor(() => image.LoadingState == ImageLoadingState.Loaded);
				await AssertColor(image, Colors.Blue);
			}, image);
		}
	}
}
