using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Image)]
	public partial class ImageLoadingTests : ControlsHandlerTestBase
	{
		static readonly Uri RedUri = new("https://images.test/red.png");

		readonly FakeImageServer _server = new();
		static readonly string CacheRoot = Path.Combine(Path.GetTempPath(), "maui-image-loading-tests");

		readonly string _cacheDirectory = Path.Combine(CacheRoot, Guid.NewGuid().ToString("N"));

		void SetupBuilder(Func<IServiceProvider, UriImageSourceService> createUriImageSourceService = null)
		{
			// Remove the caches of earlier tests.
			try
			{
				Directory.Delete(CacheRoot, recursive: true);
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
			}

			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Image, ImageHandler>();
					handlers.AddHandler<Layout, LayoutHandler>();
				});
				// Apps register this through UseMauiApp; the test host only registers the Core interfaces.
				builder.ConfigureImageSources(services =>
					services.AddService<UriImageSource>(svcs => createUriImageSourceService?.Invoke(svcs) ?? new UriImageSourceService(null, ImageSourcesMauiAppBuilderExtensions.GetUriImageLoader(svcs))));
				builder.ConfigureImageLoading(options =>
				{
					options.HttpMessageHandlerFactory = () => _server;
					options.CacheDirectory = _cacheDirectory;
					options.RetryDelay = TimeSpan.FromMilliseconds(1);
				});
			});
		}

		[Fact]
		public async Task ImageSourceServiceUsesTheConfiguredLoader()
		{
			SetupBuilder();
			_server.Respond(_ => Png(Colors.Red));

			var provider = MauiContext.Services.GetRequiredService<IImageSourceServiceProvider>();
			var service = Assert.IsType<UriImageSourceService>(provider.GetRequiredImageSourceService(new UriImageSource { Uri = RedUri }));
			Assert.Same(MauiContext.Services.GetRequiredService<IImageCache>(), service.Loader);

			var data = await service.Loader.LoadAsync(RedUri, true, TimeSpan.FromDays(1));
			Assert.NotEmpty(data);
			Assert.Equal(1, _server.RequestCount);
		}

		[Fact]
		public async Task RemoteImageIsDownloadedOnceAndShared()
		{
			SetupBuilder();
			_server.Respond(_ => Png(Colors.Red));

			var first = CreateImage(new UriImageSource { Uri = RedUri });
			var second = CreateImage(new UriImageSource { Uri = RedUri });
			var loaded = 0;
			first.ImageLoaded += (_, _) => loaded++;

			await RunWithImages(async () =>
			{
				await WaitFor(() => first.LoadingState == ImageLoadingState.Loaded && second.LoadingState == ImageLoadingState.Loaded);
				await AssertColor(first, Colors.Red);
				await AssertColor(second, Colors.Red);
			}, first, second);

			Assert.Equal(1, loaded);
			Assert.Equal(1, _server.RequestCount);
		}

		[Fact]
		public async Task PlaceholderIsShownWhileLoading()
		{
			SetupBuilder();
			var release = new TaskCompletionSource<bool>();
			_server.Respond(async _ =>
			{
				await release.Task;
				return Png(Colors.Red);
			});

			var image = CreateImage(new UriImageSource { Uri = RedUri });
			image.PlaceholderSource = "blue.png";

			await RunWithImages(async () =>
			{
				await WaitFor(() => _server.RequestCount == 1);
				Assert.Equal(ImageLoadingState.Loading, image.LoadingState);
				Assert.True(image.IsLoading);
				await AssertColor(image, Colors.Blue);

				release.SetResult(true);

				await WaitFor(() => image.LoadingState == ImageLoadingState.Loaded);
				await AssertColor(image, Colors.Red);
			}, image);
		}

		[Fact]
		public async Task ErrorSourceIsShownWhenLoadingFails()
		{
			SetupBuilder();
			_server.Respond(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

			var image = CreateImage(new UriImageSource { Uri = RedUri });
			image.ErrorSource = "green.png";
			ImageFailedEventArgs failure = null;
			image.ImageFailed += (_, e) => failure = e;

			await RunWithImages(async () =>
			{
				await WaitFor(() => image.LoadingState == ImageLoadingState.Failed);
				await AssertColor(image, Colors.Lime);
			}, image);

			var exception = Assert.IsType<HttpRequestException>(failure.Exception);
			Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
			Assert.False(image.IsLoading);
		}

		[Fact]
		public async Task ReloadLoadsTheImageAfterAFailure()
		{
			SetupBuilder();
			_server.Respond(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

			var image = CreateImage(new UriImageSource { Uri = RedUri });

			await RunWithImages(async () =>
			{
				await WaitFor(() => image.LoadingState == ImageLoadingState.Failed);
				var requestsBeforeReload = _server.RequestCount;

				_server.Respond(_ => Png(Colors.Red));
				image.Reload();

				await WaitFor(() => image.LoadingState == ImageLoadingState.Loaded);
				await AssertColor(image, Colors.Red);

				// Failures are not cached: reloading downloads again. (Android may also retry a failed image
				// once on its own when the view is attached, so only the reload itself is checked.)
				Assert.True(_server.RequestCount > requestsBeforeReload);
			}, image);
		}

		[Fact]
		public async Task ExpiredImageIsRevalidatedWithTheServer()
		{
			SetupBuilder();
			_server.Respond(_ => Png(Colors.Red, etag: "\"red\""));
			var first = CreateImage(new UriImageSource { Uri = RedUri, CacheValidity = TimeSpan.Zero });

			await RunWithImages(async () =>
			{
				await WaitFor(() => first.LoadingState == ImageLoadingState.Loaded);
			}, first);

			string ifNoneMatch = null;
			_server.Respond(request =>
			{
				ifNoneMatch = request.Headers.IfNoneMatch.ToString();
				return new HttpResponseMessage(HttpStatusCode.NotModified);
			});
			var second = CreateImage(new UriImageSource { Uri = RedUri, CacheValidity = TimeSpan.Zero });

			await RunWithImages(async () =>
			{
				await WaitFor(() => second.LoadingState == ImageLoadingState.Loaded);
				await AssertColor(second, Colors.Red);
			}, second);

			Assert.Equal("\"red\"", ifNoneMatch);
			Assert.Equal(2, _server.RequestCount);
		}

		[Fact]
		public async Task LateErrorImageDoesNotReplaceANewerImage()
		{
			SetupBuilder();
			var releaseErrorImage = new TaskCompletionSource<bool>();
			var errorImageRequested = false;
			var sourceFails = true;
			_server.Respond(async request =>
			{
				if (request.RequestUri.AbsolutePath.EndsWith("error.png", StringComparison.Ordinal))
				{
					errorImageRequested = true;
					await releaseErrorImage.Task;
					return Png(Colors.Lime);
				}

				return sourceFails ? new HttpResponseMessage(HttpStatusCode.NotFound) : Png(Colors.Red);
			});

			var image = CreateImage(new UriImageSource { Uri = RedUri });
			image.ErrorSource = new UriImageSource { Uri = new Uri("https://images.test/error.png") };

			await RunWithImages(async () =>
			{
				await WaitFor(() => image.LoadingState == ImageLoadingState.Failed && errorImageRequested);

				sourceFails = false;
				image.Reload();
				await WaitFor(() => image.LoadingState == ImageLoadingState.Loaded);
				await AssertColor(image, Colors.Red);

				// The error image of the earlier, failed load arrives after the image was reloaded.
				releaseErrorImage.SetResult(true);
				await Task.Delay(500);
				await image.ToPlatform().AssertContainsColor(Colors.Red, image.Handler.MauiContext);
			}, image);
		}

		[Fact]
		public async Task ReloadWhileLoadingKeepsLoading()
		{
			SetupBuilder();
			var release = new TaskCompletionSource<bool>();
			_server.Respond(async _ =>
			{
				await release.Task;
				return Png(Colors.Red);
			});

			var image = CreateImage(new UriImageSource { Uri = RedUri });

			await RunWithImages(async () =>
			{
				await WaitFor(() => _server.RequestCount == 1);
				image.Reload();
				await Task.Delay(200);
				Assert.True(image.IsLoading);
				Assert.Equal(ImageLoadingState.Loading, image.LoadingState);

				release.SetResult(true);
				await WaitFor(() => image.LoadingState == ImageLoadingState.Loaded && !image.IsLoading);
			}, image);
		}

		[Fact]
		public async Task ClearingTheSourceWhileLoadingStopsLoading()
		{
			SetupBuilder();
			var release = new TaskCompletionSource<bool>();
			_server.Respond(async _ =>
			{
				await release.Task;
				return Png(Colors.Red);
			});

			var image = CreateImage(new UriImageSource { Uri = RedUri });

			await RunWithImages(async () =>
			{
				await WaitFor(() => _server.RequestCount == 1 && image.IsLoading);

				image.Source = null;
				await WaitFor(() => !image.IsLoading && image.LoadingState == ImageLoadingState.Idle);

				release.SetResult(true);
				await Task.Delay(500);
				Assert.False(image.IsLoading);
				Assert.Equal(ImageLoadingState.Idle, image.LoadingState);
			}, image);
		}

		[Fact]
		public async Task EmptyUriImageSourceStaysIdle()
		{
			SetupBuilder();
			var image = CreateImage(new UriImageSource());
			image.ErrorSource = "green.png";

			await RunWithImages(async () =>
			{
				await Task.Delay(500);
				Assert.Equal(ImageLoadingState.Idle, image.LoadingState);
				Assert.Empty(_failures);
			}, image);
		}

		[Fact]
		public async Task KeepPreviousImageWhileLoadingKeepsTheImageInsteadOfThePlaceholder()
		{
			SetupBuilder();
			var release = new TaskCompletionSource<bool>();
			_server.Respond(async request =>
			{
				if (request.RequestUri.AbsolutePath.EndsWith("blue.png", StringComparison.Ordinal))
				{
					await release.Task;
					return Png(Colors.Blue);
				}

				return Png(Colors.Red);
			});

			var image = CreateImage(new UriImageSource { Uri = RedUri });
			image.PlaceholderSource = "green.png";
			image.KeepPreviousImageWhileLoading = true;
			var loaded = 0;
			image.ImageLoaded += (_, _) => loaded++;

			await RunWithImages(async () =>
			{
				await WaitFor(() => image.LoadingState == ImageLoadingState.Loaded);
				await AssertColor(image, Colors.Red);

				image.Source = new UriImageSource { Uri = new Uri("https://images.test/blue.png") };
				await WaitFor(() => _server.RequestCount >= 2);
				Assert.Equal(ImageLoadingState.Loading, image.LoadingState);

				// The current image stays (no placeholder, not cleared) while the new one loads.
				await Task.Delay(300);
				await image.ToPlatform().AssertContainsColor(Colors.Red, image.Handler.MauiContext);
				await image.ToPlatform().AssertDoesNotContainColor(Colors.Lime, image.Handler.MauiContext);

				release.SetResult(true);
				await WaitFor(() => loaded == 2);
				await AssertColor(image, Colors.Blue);
				await image.ToPlatform().AssertDoesNotContainColor(Colors.Red, image.Handler.MauiContext);
			}, image);
		}

		[Fact]
		public async Task KeepPreviousImageWhileLoadingShowsTheErrorImageWhenTheNewImageFails()
		{
			SetupBuilder();
			_server.Respond(request => request.RequestUri.AbsolutePath.EndsWith("missing.png", StringComparison.Ordinal)
				? new HttpResponseMessage(HttpStatusCode.NotFound)
				: Png(Colors.Red));

			var image = CreateImage(new UriImageSource { Uri = RedUri });
			image.ErrorSource = "green.png";
			image.KeepPreviousImageWhileLoading = true;

			await RunWithImages(async () =>
			{
				await WaitFor(() => image.LoadingState == ImageLoadingState.Loaded);

				image.Source = new UriImageSource { Uri = new Uri("https://images.test/missing.png") };
				await WaitFor(() => image.LoadingState == ImageLoadingState.Failed);

				// The previous image is not left in place of the image that failed to load.
				await AssertColor(image, Colors.Lime);
				await image.ToPlatform().AssertDoesNotContainColor(Colors.Red, image.Handler.MauiContext);
			}, image);
		}

		[Fact]
		public async Task KeepPreviousImageWhileLoadingKeepsTheImageWhenTheSourceIsChangedFromImageLoaded()
		{
			SetupBuilder();
			var release = new TaskCompletionSource<bool>();
			_server.Respond(async request =>
			{
				if (request.RequestUri.AbsolutePath.EndsWith("blue.png", StringComparison.Ordinal))
				{
					await release.Task;
					return Png(Colors.Blue);
				}

				return Png(Colors.Red);
			});

			var image = CreateImage(new UriImageSource { Uri = RedUri });
			image.PlaceholderSource = "green.png";
			image.KeepPreviousImageWhileLoading = true;
			var loaded = 0;
			image.ImageLoaded += (_, _) =>
			{
				if (++loaded == 1)
					image.Source = new UriImageSource { Uri = new Uri("https://images.test/blue.png") };
			};

			await RunWithImages(async () =>
			{
				await WaitFor(() => loaded == 1 && _server.RequestCount >= 2);
				await Task.Delay(300);
				await image.ToPlatform().AssertContainsColor(Colors.Red, image.Handler.MauiContext);
				await image.ToPlatform().AssertDoesNotContainColor(Colors.Lime, image.Handler.MauiContext);

				release.SetResult(true);
				await WaitFor(() => loaded == 2);
				await AssertColor(image, Colors.Blue);
			}, image);
		}

		[Fact]
		public async Task KeepPreviousImageWhileLoadingDoesNotKeepTheImageWhenTheErrorImageIsMissing()
		{
			SetupBuilder();
			_server.Respond(request => request.RequestUri.AbsolutePath.EndsWith("missing.png", StringComparison.Ordinal)
				? new HttpResponseMessage(HttpStatusCode.NotFound)
				: Png(Colors.Red));

			var image = CreateImage(new UriImageSource { Uri = RedUri });
			image.ErrorSource = "does_not_exist.png";
			image.KeepPreviousImageWhileLoading = true;

			await RunWithImages(async () =>
			{
				await WaitFor(() => image.LoadingState == ImageLoadingState.Loaded);

				image.Source = new UriImageSource { Uri = new Uri("https://images.test/missing.png") };
				await WaitFor(() => image.LoadingState == ImageLoadingState.Failed);
				await Task.Delay(500);

				await image.ToPlatform().AssertDoesNotContainColor(Colors.Red, image.Handler.MauiContext);
			}, image);
		}

		[Fact]
		public async Task ImageCacheCanBeCleared()
		{
			SetupBuilder();
			_server.Respond(_ => Png(Colors.Red));
			var first = CreateImage(new UriImageSource { Uri = RedUri });

			await RunWithImages(() => WaitFor(() => first.LoadingState == ImageLoadingState.Loaded), first);

			await MauiContext.Services.GetRequiredService<IImageCache>().ClearAsync();

			var second = CreateImage(new UriImageSource { Uri = RedUri });
			await RunWithImages(() => WaitFor(() => second.LoadingState == ImageLoadingState.Loaded), second);

			Assert.Equal(2, _server.RequestCount);
		}

		readonly System.Collections.Generic.List<Image> _images = new();
		readonly System.Collections.Generic.List<string> _failures = new();

		Image CreateImage(ImageSource source)
		{
			var image = new Image
			{
				Source = source,
				WidthRequest = 50,
				HeightRequest = 50,
				Aspect = Aspect.Fill,
			};
			image.ImageFailed += (_, e) => _failures.Add(e.Exception.ToString());
			_images.Add(image);
			return image;
		}

		async Task AssertColor(Image image, Color color)
		{
			// Placeholder and error images are loaded asynchronously after the state changes.
			for (var attempt = 0; ; attempt++)
			{
				try
				{
					// The image is attached to the test window, whose context differs from the test's on Windows.
					await image.ToPlatform().AssertContainsColor(color, image.Handler.MauiContext);
					return;
				}
				catch (Exception) when (attempt < 20)
				{
					await Task.Delay(100);
				}
			}
		}

		async Task WaitFor(Func<bool> condition)
		{
			if (!await Wait(condition, 5000))
				throw new TimeoutException("Condition not met. " + Diagnostics());
		}

		string Diagnostics()
		{
			var states = string.Join(", ", _images.ConvertAll(i => $"{i.LoadingState}/{(i.IsLoading ? "loading" : "idle")}/{(i.Handler is null ? "no handler" : "handler")}"));
			return $"Requests: {_server.RequestCount}; images: {states}; failures: {string.Join(" | ", _failures)}";
		}

		async Task RunWithImages(Func<Task> test, params Image[] images)
		{
			var layout = new VerticalStackLayout();
			foreach (var image in images)
				layout.Add(image);

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, _ => test());
		}

		static HttpResponseMessage Png(Color color, string etag = null)
		{
			var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(CreatePng(color)) };
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
			if (etag is not null)
				response.Headers.ETag = EntityTagHeaderValue.Parse(etag);
			return response;
		}

		/// <summary>Creates a 20x20 PNG of a single color.</summary>
		static byte[] CreatePng(Color color)
		{
			const int size = 20;
			color.ToRgba(out var r, out var g, out var b, out var a);

			var raw = new byte[size * (1 + size * 4)];
			for (var y = 0; y < size; y++)
			{
				var row = y * (1 + size * 4);
				raw[row] = 0; // no filter
				for (var x = 0; x < size; x++)
				{
					var pixel = row + 1 + x * 4;
					raw[pixel] = r;
					raw[pixel + 1] = g;
					raw[pixel + 2] = b;
					raw[pixel + 3] = a;
				}
			}

			using var output = new MemoryStream();
			output.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });

			var header = new byte[13];
			WriteInt(header, 0, size);
			WriteInt(header, 4, size);
			header[8] = 8; // bit depth
			header[9] = 6; // RGBA
			WriteChunk(output, "IHDR", header);

			using (var compressed = new MemoryStream())
			{
				using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
					zlib.Write(raw);
				WriteChunk(output, "IDAT", compressed.ToArray());
			}

			WriteChunk(output, "IEND", Array.Empty<byte>());
			return output.ToArray();

			static void WriteChunk(Stream stream, string type, byte[] data)
			{
				var length = new byte[4];
				WriteInt(length, 0, data.Length);
				stream.Write(length);

				var typeAndData = new byte[4 + data.Length];
				System.Text.Encoding.ASCII.GetBytes(type, 0, 4, typeAndData, 0);
				data.CopyTo(typeAndData, 4);
				stream.Write(typeAndData);

				var crc = new byte[4];
				WriteInt(crc, 0, (int)Crc32(typeAndData));
				stream.Write(crc);
			}

			static void WriteInt(byte[] buffer, int offset, int value)
			{
				buffer[offset] = (byte)(value >> 24);
				buffer[offset + 1] = (byte)(value >> 16);
				buffer[offset + 2] = (byte)(value >> 8);
				buffer[offset + 3] = (byte)value;
			}

			static uint Crc32(byte[] data)
			{
				var crc = 0xFFFFFFFFu;
				foreach (var value in data)
				{
					crc ^= value;
					for (var bit = 0; bit < 8; bit++)
						crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
				}
				return ~crc;
			}
		}

		sealed class FakeImageServer : HttpMessageHandler
		{
			Func<HttpRequestMessage, Task<HttpResponseMessage>> _respond =
				_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
			int _requestCount;

			public int RequestCount => Volatile.Read(ref _requestCount);

			public void Respond(Func<HttpRequestMessage, HttpResponseMessage> respond) =>
				_respond = request => Task.FromResult(respond(request));

			public void Respond(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) =>
				_respond = respond;

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				Interlocked.Increment(ref _requestCount);
				return _respond(request);
			}
		}
	}
}
