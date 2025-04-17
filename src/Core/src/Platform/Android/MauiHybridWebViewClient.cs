// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Android.Webkit;
using Microsoft.Extensions.Logging;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.Platform
{
	[RequiresUnreferencedCode(HybridWebViewHandler.DynamicFeatures)]
#if !NETSTANDARD
	[RequiresDynamicCode(HybridWebViewHandler.DynamicFeatures)]
#endif
	public class MauiHybridWebViewClient : WebViewClient
	{
		private readonly WeakReference<HybridWebViewHandler?> _handler;

		public MauiHybridWebViewClient(HybridWebViewHandler handler)
		{
			_handler = new(handler);
		}

		private HybridWebViewHandler? Handler => _handler is not null && _handler.TryGetTarget(out var h) ? h : null;

		public override WebResourceResponse? ShouldInterceptRequest(AWebView? view, IWebResourceRequest? request)
		{
			if (view is not null && request is not null)
			{
				// 1. Check if the app wants to modify or override the request
				var response = TryInterceptResponseStream(view, request);
				if (response is not null)
				{
					return response;
				}

				// 2. Check if the request is for a local resource
				response = GetResponseStream(view, request);
				if (response is not null)
				{
					return response;
				}
			}

			// 3. Otherwise, we let the request go through as is
			return base.ShouldInterceptRequest(view, request);
		}

		private WebResourceResponse? TryInterceptResponseStream(AWebView view, IWebResourceRequest request)
		{
			if (Handler is null || Handler is IViewHandler ivh && ivh.VirtualView is null)
			{
				return null;
			}

			// 1. First, create the event args
			var platformArgs = new WebResourceRequestedEventArgs(view, request);

			// 2. Trigger the event for the app
			var handled = Handler.VirtualView.WebResourceRequested(platformArgs);

			// 3. If the app reported that it completed the request, then we do nothing more
			if (handled)
			{
				return platformArgs.Response;
			}

			return null;
		}

		private WebResourceResponse? GetResponseStream(AWebView view, IWebResourceRequest request)
		{
			if (Handler is null || Handler is IViewHandler ivh && ivh.VirtualView is null)
			{
				return null;
			}

			var fullUrl = request?.Url?.ToString();
			var requestUri = HybridWebViewQueryStringHelper.RemovePossibleQueryString(fullUrl);
			if (new Uri(requestUri) is not Uri uri || !HybridWebViewHandler.AppOriginUri.IsBaseOf(uri))
			{
				return null;
			}

			var relativePath = HybridWebViewHandler.AppOriginUri.MakeRelativeUri(uri).ToString().Replace('/', '\\');

			// 1. Try special InvokeDotNet path
			if (relativePath == HybridWebViewHandler.InvokeDotNetPath)
			{
				var fullUri = new Uri(fullUrl!);
				var invokeQueryString = HttpUtility.ParseQueryString(fullUri.Query);
				var contentBytesTask = Handler.InvokeDotNetAsync(invokeQueryString);
				var responseStream = new AsyncStream(contentBytesTask, Handler);
				return new WebResourceResponse("application/json", "UTF-8", 200, "OK", GetHeaders("application/json"), responseStream);
			}

			// 2. If nothing found yet, try to get static content from the asset path
			string? contentType;
			if (string.IsNullOrEmpty(relativePath))
			{
				relativePath = Handler.VirtualView.DefaultFile;
				contentType = "text/html";
			}
			else
			{
				if (!HybridWebViewHandler.ContentTypeProvider.TryGetContentType(relativePath, out contentType!))
				{
					contentType = "text/plain";
					Handler.MauiContext?.CreateLogger<HybridWebViewHandler>()?.LogWarning("Could not determine content type for '{relativePath}'", relativePath);
				}
			}

			var assetPath = Path.Combine(Handler.VirtualView.HybridRoot!, relativePath!);
			var contentStream = PlatformOpenAppPackageFile(assetPath);

			if (contentStream is not null)
			{
				// 3.a. If something was found, return the content

				// TODO: We don't know the content length because Android doesn't tell us. Seems to work without it!

				return new WebResourceResponse(contentType, "UTF-8", 200, "OK", GetHeaders(contentType ?? "text/plain"), contentStream);
			}

			// 3.b. Otherwise, return a 404
			return new WebResourceResponse(null, "UTF-8", 404, "Not Found", null, null);
		}

		private Stream? PlatformOpenAppPackageFile(string filename)
		{
			if (Handler is null)
			{
				return null;
			}

			filename = PathUtils.NormalizePath(filename);

			try
			{
				return Handler.Context.Assets?.Open(filename);
			}
			catch (Java.IO.FileNotFoundException)
			{
				return null;
			}
		}

		internal static class PathUtils
		{
			public static string NormalizePath(string filename) =>
				filename
					.Replace('\\', Path.DirectorySeparatorChar)
					.Replace('/', Path.DirectorySeparatorChar);
		}

		private protected static IDictionary<string, string> GetHeaders(string contentType) =>
			new Dictionary<string, string> {
				{ "Content-Type", contentType },
			};

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Disconnect();
			}

			base.Dispose(disposing);
		}

		internal void Disconnect()
		{
			_handler.SetTarget(null);
		}

		class AsyncStream : Stream
		{
			readonly Task<Stream> _streamTask;
			readonly WeakReference<HybridWebViewHandler> _handler;
			Stream? _stream;
			bool _isDisposed;

			public AsyncStream(Task<byte[]?> byteArrayTask, HybridWebViewHandler handler)
				: this(AsStreamTask(byteArrayTask), handler)
			{
			}

			public AsyncStream(Task<Stream> streamTask, HybridWebViewHandler handler)
			{
				_streamTask = streamTask ?? throw new ArgumentNullException(nameof(streamTask));
				_handler = new WeakReference<HybridWebViewHandler>(handler ?? throw new ArgumentNullException(nameof(handler)));
			}

			HybridWebViewHandler? Handler => _handler?.GetTargetOrDefault();

			static async Task<Stream> AsStreamTask(Task<byte[]?> task)
			{
				var bytes = await task;
				if (bytes is null)
					return Stream.Null;
				return new MemoryStream(bytes);
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
				try
				{
					var stream = await GetStreamAsync(cancellationToken).ConfigureAwait(false);
					return await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					Handler?.MauiContext?.CreateLogger<HybridWebViewHandler>()?.LogError(ex, "Error invoking .NET method from JavaScript: {ErrorMessage}", ex.Message);
					throw;
				}
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				try
				{
					var stream = GetStreamAsync().GetAwaiter().GetResult();
					return stream.Read(buffer, offset, count);
				}
				catch (Exception ex)
				{
					Handler?.MauiContext?.CreateLogger<HybridWebViewHandler>()?.LogError(ex, "Error invoking .NET method from JavaScript: {ErrorMessage}", ex.Message);
					throw;
				}
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
}
