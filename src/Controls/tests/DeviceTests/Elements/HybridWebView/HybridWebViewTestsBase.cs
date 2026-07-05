#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.HybridWebView)]
public partial class HybridWebViewTestsBase : ControlsHandlerTestBase
{
	void SetupBuilder()
	{
		EnsureHandlerCreated(builder =>
		{
			builder.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<HybridWebView, HybridWebViewHandler>();
			});

			builder.Services.AddHybridWebViewDeveloperTools();
			builder.Services.AddScoped<IHybridWebViewTaskManager, HybridWebViewTaskManager>();
		});
	}

	protected Task RunTest(Func<HybridWebView, Task> test) =>
		RunTest(null, test);

	protected async Task RunTest(string? defaultFile, Func<HybridWebView, Task> test)
	{
		var hybridWebView = new HybridWebView
		{
			WidthRequest = 100,
			HeightRequest = 100,

			HybridRoot = "HybridTestRoot",
			DefaultFile = defaultFile ?? "index.html",
		};

		await RunTest(hybridWebView, (handler, view) => test(view));
	}

	protected async Task RunTest(HybridWebView hybridWebView, Func<HybridWebViewHandler, HybridWebView, Task> test)
	{
		// NOTE: skip this test on older Android devices because it is not currently supported on these versions
		if (OperatingSystem.IsAndroid() && !OperatingSystem.IsAndroidVersionAtLeast(24))
		{
			return;
		}

		SetupBuilder();

		// Set up the view to be displayed/parented and run our tests on it
		await AttachAndRun(hybridWebView, async handler =>
		{
			await WebViewHelpers.WaitForHybridWebViewLoaded(hybridWebView);

			// Use a cancellation token with a timeout
			using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

			var testWrapper = test((HybridWebViewHandler)handler, hybridWebView);

			await testWrapper.WaitAsync(cts.Token);
		});
	}

	protected static partial class WebViewHelpers
	{
		const int MaxWaitTimes = 100;
		const int WaitTimeInMS = 250;

		private static async Task Retry(Func<Task<bool>> tryAction, Func<int, Task<Exception>> createExceptionWithTimeoutMS)
		{
			for (var i = 0; i < MaxWaitTimes; i++)
			{
				if (await tryAction())
				{
					await Task.Delay(WaitTimeInMS);
					return;
				}

				await Task.Delay(WaitTimeInMS);
			}

			throw await createExceptionWithTimeoutMS(MaxWaitTimes * WaitTimeInMS);
		}

		public static async Task WaitForHybridWebViewLoaded(HybridWebView hybridWebView)
		{
			await Retry(async () =>
			{
				var loaded = await hybridWebView.EvaluateJavaScriptAsync("('HybridWebView' in window && Object.prototype.hasOwnProperty.call(window, 'HybridWebView')) && (document.getElementById('htmlLoaded') !== null)");
				return loaded == "true";
			}, createExceptionWithTimeoutMS: (int timeoutInMS) => Task.FromResult(new Exception($"Waited {timeoutInMS}ms but couldn't get the HybridWebView test page to be ready.")));
		}

		public static async Task WaitForHtmlStatusSet(HybridWebView hybridWebView)
		{
			await Retry(async () =>
			{
				var controlValue = await hybridWebView.EvaluateJavaScriptAsync("document.getElementById('status').innerText");
				return !string.IsNullOrEmpty(controlValue);
			}, createExceptionWithTimeoutMS: (int timeoutInMS) => Task.FromResult(new Exception($"Waited {timeoutInMS}ms but couldn't get status element to have a non-empty value.")));
		}
	}

	protected class AsyncStream : Stream
	{
		readonly Task<Stream> _streamTask;
		Stream? _stream;
		bool _isDisposed;

		public AsyncStream(Task<Stream> streamTask)
		{
			_streamTask = streamTask ?? throw new ArgumentNullException(nameof(streamTask));
		}

		async Task<Stream> GetStreamAsync(CancellationToken cancellationToken = default)
		{
			ObjectDisposedException.ThrowIf(_isDisposed, nameof(AsyncStream));

			if (_stream != null)
				return _stream;

			_stream = await _streamTask.ConfigureAwait(false);
			return _stream;
		}

		public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
		{
			var stream = await GetStreamAsync(cancellationToken).ConfigureAwait(false);
			return await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			var stream = GetStreamAsync().GetAwaiter().GetResult();
			return stream.Read(buffer, offset, count);
		}

		public override void Flush() => throw new NotSupportedException();

		public override Task FlushAsync(CancellationToken cancellationToken) => throw new NotSupportedException();

		public override bool CanRead => !_isDisposed;

		public override bool CanSeek => false;

		public override bool CanWrite => false;

		public override long Length => throw new NotSupportedException();

		public override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

		public override void SetLength(long value) => throw new NotSupportedException();

		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
				_stream?.Dispose();

			_isDisposed = true;
			base.Dispose(disposing);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_isDisposed)
				return;

			if (_stream != null)
				await _stream.DisposeAsync().ConfigureAwait(false);

			_isDisposed = true;
			await base.DisposeAsync().ConfigureAwait(false);
		}
	}
}
