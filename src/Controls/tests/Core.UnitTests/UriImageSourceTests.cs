using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[Collection(nameof(FileSystemCollection))]
	public class UriImageSourceTests : BaseTestFixture
	{
		[Theory]
		[InlineData(true, 1)]
		[InlineData(false, 2)]
		public async Task CachingEnabledControlsHttpRequests(bool cachingEnabled, int expectedRequests)
		{
			using var cache = new TemporaryDirectory();
			using var server = new LoopbackHttpServer([1, 2, 3]);
			FileSystem.SetCurrent(new TestFileSystem(cache.Path));

			try
			{
				IStreamImageSource firstSource = new UriImageSource
				{
					Uri = server.Uri,
					CachingEnabled = cachingEnabled,
				};
				IStreamImageSource secondSource = new UriImageSource
				{
					Uri = server.Uri,
					CachingEnabled = cachingEnabled,
				};

				using var first = await firstSource.GetStreamAsync();
				using var second = await secondSource.GetStreamAsync();

				Assert.Equal([1, 2, 3], await ReadAllBytesAsync(first));
				Assert.Equal([1, 2, 3], await ReadAllBytesAsync(second));
				Assert.Equal(expectedRequests, server.RequestCount);
				Assert.Equal(cachingEnabled, File.Exists(UriImageSourceCache.GetCachePath(server.Uri, cache.Path)));
			}
			finally
			{
				FileSystem.SetCurrent(null);
			}
		}

		[Fact]
		public async Task ExpiredCacheTriggersHttpRequest()
		{
			using var cache = new TemporaryDirectory();
			using var server = new LoopbackHttpServer([1, 2, 3]);
			FileSystem.SetCurrent(new TestFileSystem(cache.Path));

			try
			{
				IStreamImageSource firstSource = new UriImageSource
				{
					Uri = server.Uri,
					CacheValidity = TimeSpan.FromMinutes(1),
				};

				using var first = await firstSource.GetStreamAsync();
				File.SetLastWriteTimeUtc(
					UriImageSourceCache.GetCachePath(server.Uri, cache.Path),
					DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(2)));

				IStreamImageSource secondSource = new UriImageSource
				{
					Uri = server.Uri,
					CacheValidity = TimeSpan.FromMinutes(1),
				};
				using var second = await secondSource.GetStreamAsync();

				Assert.Equal([1, 2, 3], await ReadAllBytesAsync(first));
				Assert.Equal([1, 2, 3], await ReadAllBytesAsync(second));
				Assert.Equal(2, server.RequestCount);
			}
			finally
			{
				FileSystem.SetCurrent(null);
			}
		}

		[Fact]
		public async Task HttpCancellationIsPropagatedAndNotCached()
		{
			using var cache = new TemporaryDirectory();
			using var server = new LoopbackHttpServer([1, 2, 3], TimeSpan.FromSeconds(30));
			FileSystem.SetCurrent(new TestFileSystem(cache.Path));

			try
			{
				IStreamImageSource source = new UriImageSource
				{
					Uri = server.Uri,
				};
				using var cancellationTokenSource = new CancellationTokenSource();

				var loadTask = source.GetStreamAsync(cancellationTokenSource.Token);
				await server.RequestReceived.WaitAsync(TimeSpan.FromSeconds(5));
				cancellationTokenSource.Cancel();

				await Assert.ThrowsAnyAsync<OperationCanceledException>(() => loadTask);
				Assert.False(File.Exists(UriImageSourceCache.GetCachePath(server.Uri, cache.Path)));
			}
			finally
			{
				FileSystem.SetCurrent(null);
			}
		}

		[Fact]
		public async Task SecondCallLoadsFromCache()
		{
			using var cache = new TemporaryDirectory();
			var uri = new Uri("https://example.com/image.png");
			var networkCalls = 0;

			Task<Stream> Download(CancellationToken cancellationToken)
			{
				networkCalls++;
				return Task.FromResult<Stream>(new MemoryStream([1, 2, 3]));
			}

			using var first = await UriImageSourceCache.GetStreamAsync(uri, TimeSpan.FromDays(1), Download, cache.Path, default);
			using var second = await UriImageSourceCache.GetStreamAsync(uri, TimeSpan.FromDays(1), Download, cache.Path, default);

			Assert.Equal([1, 2, 3], await ReadAllBytesAsync(first));
			Assert.Equal([1, 2, 3], await ReadAllBytesAsync(second));
			Assert.Equal(1, networkCalls);
			Assert.True(File.Exists(UriImageSourceCache.GetCachePath(uri, cache.Path)));
		}

		[Fact]
		public async Task ExpiredEntryIsDownloadedAgain()
		{
			using var cache = new TemporaryDirectory();
			var uri = new Uri("https://example.com/image.png");
			var networkCalls = 0;

			Task<Stream> Download(CancellationToken cancellationToken)
			{
				networkCalls++;
				return Task.FromResult<Stream>(new MemoryStream([(byte)networkCalls]));
			}

			using var first = await UriImageSourceCache.GetStreamAsync(uri, TimeSpan.FromMinutes(1), Download, cache.Path, default);
			File.SetLastWriteTimeUtc(UriImageSourceCache.GetCachePath(uri, cache.Path), DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(2)));
			using var second = await UriImageSourceCache.GetStreamAsync(uri, TimeSpan.FromMinutes(1), Download, cache.Path, default);

			Assert.Equal([1], await ReadAllBytesAsync(first));
			Assert.Equal([2], await ReadAllBytesAsync(second));
			Assert.Equal(2, networkCalls);
		}

		[Fact]
		public async Task ZeroValidityAlwaysDownloads()
		{
			using var cache = new TemporaryDirectory();
			var uri = new Uri("https://example.com/image.png");
			var networkCalls = 0;

			Task<Stream> Download(CancellationToken cancellationToken)
			{
				networkCalls++;
				return Task.FromResult<Stream>(new MemoryStream([1, 2, 3]));
			}

			using var first = await UriImageSourceCache.GetStreamAsync(uri, TimeSpan.Zero, Download, cache.Path, default);
			using var second = await UriImageSourceCache.GetStreamAsync(uri, TimeSpan.Zero, Download, cache.Path, default);

			Assert.Equal(2, networkCalls);
		}

		[Fact]
		public async Task FailedRetrieveIsNotCached()
		{
			using var cache = new TemporaryDirectory();
			var uri = new Uri("https://example.com/missing.png");
			var networkCalls = 0;

			Task<Stream> Download(CancellationToken cancellationToken)
			{
				networkCalls++;
				return Task.FromResult<Stream>(null);
			}

			Assert.Null(await UriImageSourceCache.GetStreamAsync(uri, TimeSpan.FromDays(1), Download, cache.Path, default));
			Assert.Null(await UriImageSourceCache.GetStreamAsync(uri, TimeSpan.FromDays(1), Download, cache.Path, default));
			Assert.Equal(2, networkCalls);
			Assert.False(File.Exists(UriImageSourceCache.GetCachePath(uri, cache.Path)));
		}

		[Fact]
		public async Task ConcurrentCallsToSameUriAreCoalesced()
		{
			using var cache = new TemporaryDirectory();
			var uri = new Uri("https://example.com/image.png");
			var releaseDownload = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
			var downloadStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
			var networkCalls = 0;

			async Task<Stream> Download(CancellationToken cancellationToken)
			{
				Interlocked.Increment(ref networkCalls);
				downloadStarted.SetResult();
				await releaseDownload.Task.WaitAsync(cancellationToken);
				return new MemoryStream([1, 2, 3]);
			}

			var firstTask = UriImageSourceCache.GetStreamAsync(uri, TimeSpan.FromDays(1), Download, cache.Path, default);
			await downloadStarted.Task;
			var secondTask = UriImageSourceCache.GetStreamAsync(uri, TimeSpan.FromDays(1), Download, cache.Path, default);
			releaseDownload.SetResult();

			using var first = await firstTask;
			using var second = await secondTask;

			Assert.Equal([1, 2, 3], await ReadAllBytesAsync(first));
			Assert.Equal([1, 2, 3], await ReadAllBytesAsync(second));
			Assert.Equal(1, networkCalls);
		}

		[Fact]
		public async Task CancellationWhileWaitingForCacheIsObserved()
		{
			using var cache = new TemporaryDirectory();
			var uri = new Uri("https://example.com/image.png");
			var releaseDownload = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
			var downloadStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

			async Task<Stream> Download(CancellationToken cancellationToken)
			{
				downloadStarted.SetResult();
				await releaseDownload.Task.WaitAsync(cancellationToken);
				return new MemoryStream([1, 2, 3]);
			}

			var firstTask = UriImageSourceCache.GetStreamAsync(uri, TimeSpan.FromDays(1), Download, cache.Path, default);
			await downloadStarted.Task;

			using var cancellationTokenSource = new CancellationTokenSource();
			var secondTask = UriImageSourceCache.GetStreamAsync(uri, TimeSpan.FromDays(1), Download, cache.Path, cancellationTokenSource.Token);
			cancellationTokenSource.Cancel();

			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => secondTask);

			releaseDownload.SetResult();
			using var first = await firstTask;
		}

		[Fact]
		public async Task CancellationDoesNotCreateCacheEntry()
		{
			using var cache = new TemporaryDirectory();
			var uri = new Uri("https://example.com/image.png");
			using var cancellationTokenSource = new CancellationTokenSource();

			Task<Stream> Download(CancellationToken cancellationToken)
			{
				cancellationTokenSource.Cancel();
				return Task.FromResult<Stream>(new MemoryStream([1, 2, 3]));
			}

			await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
				UriImageSourceCache.GetStreamAsync(uri, TimeSpan.FromDays(1), Download, cache.Path, cancellationTokenSource.Token));

			Assert.False(File.Exists(UriImageSourceCache.GetCachePath(uri, cache.Path)));
			Assert.Empty(Directory.GetFiles(cache.Path, "*.tmp", SearchOption.AllDirectories));
		}

		[Fact]
		public async Task CacheWriteFailureStillReturnsDownloadedImage()
		{
			using var cache = new TemporaryDirectory();
			var invalidCacheDirectory = System.IO.Path.Combine(cache.Path, "file");
			File.WriteAllText(invalidCacheDirectory, "not a directory");
			var errors = 0;

			using var stream = await UriImageSourceCache.GetStreamAsync(
				new Uri("https://example.com/image.png"),
				TimeSpan.FromDays(1),
				cancellationToken => Task.FromResult<Stream>(new MemoryStream([1, 2, 3])),
				invalidCacheDirectory,
				default,
				ex => errors++);

			Assert.Equal([1, 2, 3], await ReadAllBytesAsync(stream));
			Assert.Equal(1, errors);
		}

		[Fact]
		public void NullUriDoesNotCrash()
		{
			var loader = new UriImageSource();
			loader.Uri = null;
		}

		[Fact]
		public void SameUriHasSameCachePath()
		{
			var uri = new Uri("https://example.com/image.png?version=1");

			Assert.Equal(
				UriImageSourceCache.GetCachePath(uri, "cache"),
				UriImageSourceCache.GetCachePath(uri, "cache"));
		}

		[Fact]
		public void DifferentUrisHaveDifferentCachePaths()
		{
			Assert.NotEqual(
				UriImageSourceCache.GetCachePath(new Uri("https://example.com/image.png?version=1"), "cache"),
				UriImageSourceCache.GetCachePath(new Uri("https://example.com/image.png?version=2"), "cache"));
		}

		static async Task<byte[]> ReadAllBytesAsync(Stream stream)
		{
			using var destination = new MemoryStream();
			await stream.CopyToAsync(destination);
			return destination.ToArray();
		}

		sealed class TemporaryDirectory : IDisposable
		{
			public TemporaryDirectory()
			{
				Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"maui-uri-image-cache-{Guid.NewGuid():N}");
				Directory.CreateDirectory(Path);
			}

			public string Path { get; }

			public void Dispose() => Directory.Delete(Path, true);
		}
	}

	[CollectionDefinition(nameof(FileSystemCollection), DisableParallelization = true)]
	public sealed class FileSystemCollection
	{
	}

	sealed class TestFileSystem : IFileSystem
	{
		public TestFileSystem(string cacheDirectory)
		{
			CacheDirectory = cacheDirectory;
		}

		public string CacheDirectory { get; }

		public string AppDataDirectory => throw new NotSupportedException();

		public Task<Stream> OpenAppPackageFileAsync(string filename) => throw new NotSupportedException();

		public Task<bool> AppPackageFileExistsAsync(string filename) => throw new NotSupportedException();
	}

	sealed class LoopbackHttpServer : IDisposable
	{
		readonly byte[] _content;
		readonly TimeSpan _responseDelay;
		readonly CancellationTokenSource _cancellationTokenSource = new();
		readonly TcpListener _listener = new(IPAddress.Loopback, 0);
		readonly TaskCompletionSource _requestReceived = new(TaskCreationOptions.RunContinuationsAsynchronously);
		readonly Task _serverTask;
		int _requestCount;

		public LoopbackHttpServer(byte[] content, TimeSpan responseDelay = default)
		{
			_content = content;
			_responseDelay = responseDelay;
			_listener.Start();
			Uri = new Uri($"http://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}/image");
			_serverTask = RunAsync(_cancellationTokenSource.Token);
		}

		public Uri Uri { get; }

		public int RequestCount => Volatile.Read(ref _requestCount);

		public Task RequestReceived => _requestReceived.Task;

		async Task RunAsync(CancellationToken cancellationToken)
		{
			try
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					using var client = await _listener.AcceptTcpClientAsync(cancellationToken);
					await HandleRequestAsync(client, cancellationToken);
				}
			}
			catch (OperationCanceledException)
			{
			}
			catch (ObjectDisposedException)
			{
			}
		}

		async Task HandleRequestAsync(TcpClient client, CancellationToken cancellationToken)
		{
			var stream = client.GetStream();
			using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);

			while (!string.IsNullOrEmpty(await reader.ReadLineAsync(cancellationToken)))
			{
			}

			Interlocked.Increment(ref _requestCount);
			_requestReceived.TrySetResult();
			await Task.Delay(_responseDelay, cancellationToken);

			var headers = Encoding.ASCII.GetBytes(
				$"HTTP/1.1 200 OK\r\nContent-Length: {_content.Length}\r\nConnection: close\r\n\r\n");
			await stream.WriteAsync(headers, cancellationToken);
			await stream.WriteAsync(_content, cancellationToken);
		}

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
			_listener.Stop();

			try
			{
				_serverTask.GetAwaiter().GetResult();
			}
			catch (SocketException)
			{
			}

			_cancellationTokenSource.Dispose();
		}
	}
}
