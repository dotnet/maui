// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Android.Webkit;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
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
			var response = GetResponseStream(view, request);

			if (response is not null)
			{
				return response;
			}

			return base.ShouldInterceptRequest(view, request);
		}

		private WebResourceResponse? GetResponseStream(AWebView? view, IWebResourceRequest? request)
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
				var responseStream = new DotNetInvokeAsyncStream(contentBytesTask, Handler);
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
			var notFoundContent = "Resource not found (404)";

			var notFoundByteArray = Encoding.UTF8.GetBytes(notFoundContent);
			var notFoundContentStream = new MemoryStream(notFoundByteArray);

			return new WebResourceResponse("text/plain", "UTF-8", 404, "Not Found", GetHeaders("text/plain"), notFoundContentStream);
		}

		private Stream? PlatformOpenAppPackageFile(string filename)
		{
			if (Handler is null)
			{
				return null;
			}

			filename = FileSystemUtils.NormalizePath(filename);

			try
			{
				return Handler.Context.Assets?.Open(filename);
			}
			catch (Java.IO.FileNotFoundException)
			{
				return null;
			}
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

		private class DotNetInvokeAsyncStream : Stream
		{
			private const int PauseThreshold = 32 * 1024;
			private const int ResumeThreshold = 16 * 1024;

			private readonly Task<byte[]?> _task;
			private readonly WeakReference<HybridWebViewHandler> _handler;
			private readonly Pipe _pipe;

			private bool _isDisposed;

			private HybridWebViewHandler? Handler => _handler?.GetTargetOrDefault();

			public override bool CanRead => !_isDisposed;

			public override bool CanSeek => false;

			public override bool CanWrite => false;

			public override long Length => throw new NotSupportedException();

			public override long Position
			{
				get => throw new NotSupportedException();
				set => throw new NotSupportedException();
			}

			public DotNetInvokeAsyncStream(Task<byte[]?> invokeTask, HybridWebViewHandler handler)
			{
				_task = invokeTask;
				_handler = new(handler);

				_pipe = new Pipe(new PipeOptions(
					pauseWriterThreshold: PauseThreshold,
					resumeWriterThreshold: ResumeThreshold,
					useSynchronizationContext: false));

				InvokeMethodAndWriteBytes();
			}

			private async void InvokeMethodAndWriteBytes()
			{
				try
				{
					var data = await _task;

					// the stream or handler may be disposed after the method completes
					ObjectDisposedException.ThrowIf(_isDisposed, nameof(DotNetInvokeAsyncStream));
					ArgumentNullException.ThrowIfNull(Handler, nameof(Handler));

					// copy the data into the pipe
					if (data is not null && data.Length > 0)
					{
						var memory = _pipe.Writer.GetMemory(data.Length);
						data.CopyTo(memory);
						_pipe.Writer.Advance(data.Length);
					}

					_pipe.Writer.Complete();
				}
				catch (Exception ex)
				{
					Handler?.MauiContext?.CreateLogger<HybridWebViewHandler>()?.LogError(ex, "Error invoking .NET method from JavaScript: {ErrorMessage}", ex.Message);

					_pipe.Writer.Complete(ex);
				}
			}

			public override void Flush() =>
				throw new NotSupportedException();

			public override int Read(byte[] buffer, int offset, int count)
			{
				ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
				ArgumentOutOfRangeException.ThrowIfNegative(offset, nameof(offset));
				ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
				ArgumentOutOfRangeException.ThrowIfGreaterThan(offset + count, buffer.Length, nameof(count));
				ObjectDisposedException.ThrowIf(_isDisposed, nameof(DotNetInvokeAsyncStream));

				// this is a blocking read, so we need to wait for data to be available
				var readResult = _pipe.Reader.ReadAsync().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
				var slice = readResult.Buffer.Slice(0, Math.Min(count, readResult.Buffer.Length));

				var bytesRead = 0;
				foreach (var span in slice)
				{
					var bytesToCopy = Math.Min(count, span.Length);
					span.CopyTo(new Memory<byte>(buffer, offset, bytesToCopy));
					offset += bytesToCopy;
					count -= bytesToCopy;
					bytesRead += bytesToCopy;
				}

				_pipe.Reader.AdvanceTo(slice.End);

				return bytesRead;
			}

			public override long Seek(long offset, SeekOrigin origin) =>
				throw new NotSupportedException();

			public override void SetLength(long value) =>
				throw new NotSupportedException();

			public override void Write(byte[] buffer, int offset, int count) =>
				throw new NotSupportedException();

			protected override void Dispose(bool disposing)
			{
				_isDisposed = true;

				_pipe.Writer.Complete();
				_pipe.Reader.Complete();

				base.Dispose(disposing);
			}
		}
	}
}
