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
			if (Handler is not null)
			{
				var fullUrl = request?.Url?.ToString();
				var requestUri = HybridWebViewQueryStringHelper.RemovePossibleQueryString(fullUrl);

				if (new Uri(requestUri) is Uri uri && HybridWebViewHandler.AppOriginUri.IsBaseOf(uri))
				{
					var relativePath = HybridWebViewHandler.AppOriginUri.MakeRelativeUri(uri).ToString().Replace('/', '\\');

					// 1. Try special InvokeDotNet path
					if (relativePath == HybridWebViewHandler.InvokeDotNetPath)
					{
						var fullUri = new Uri(fullUrl!);
						var invokeQueryString = HttpUtility.ParseQueryString(fullUri.Query);
						var task = Handler.InvokeDotNetAsync(invokeQueryString);
						return new WebResourceResponse("application/json", "UTF-8", 200, "OK", GetHeaders("application/json"), new DotNetInvokeAsyncStream(task));
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
							// TODO: Log this
							contentType = "text/plain";
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
			}

			return null;
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

		private class DotNetInvokeAsyncStream : Stream
		{
			private const int PauseThreshold = 32 * 1024;
			private const int ResumeThreshold = 16 * 1024;

			private readonly Pipe _pipe;
			private readonly Task<byte[]?> _task;

			private bool _isDisposed;

			public override bool CanRead => !_isDisposed;

			public override bool CanSeek => false;

			public override bool CanWrite => !_isDisposed;

			public override long Length => throw new NotSupportedException();

			public override long Position
			{
				get => throw new NotSupportedException();
				set => throw new NotSupportedException();
			}

			public DotNetInvokeAsyncStream(Task<byte[]?> invokeTask)
			{
				_task = invokeTask;

				_pipe = new Pipe(new PipeOptions(
					pauseWriterThreshold: PauseThreshold,
					resumeWriterThreshold: ResumeThreshold,
					useSynchronizationContext: false));

				StartReading();
			}

			private async void StartReading()
			{
				var data = await _task;
				await WriteAsync(data);
				_pipe.Writer.Complete();
			}

			public override void Flush() =>
				_pipe.Writer.FlushAsync().ConfigureAwait(false).GetAwaiter().GetResult();

			public override int Read(byte[] buffer, int offset, int count)
			{
				ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
				ArgumentOutOfRangeException.ThrowIfNegative(offset, nameof(offset));
				ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
				ArgumentOutOfRangeException.ThrowIfGreaterThan(offset + count, buffer.Length, nameof(count));
				if (_isDisposed)
				{
					throw new ObjectDisposedException(nameof(DotNetInvokeAsyncStream));
				}

				var bytesRead = 0;

				var readResult = _pipe.Reader.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				var slice = readResult.Buffer.Slice(0, Math.Min(count, readResult.Buffer.Length));
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

			public override void Write(byte[] buffer, int offset, int count)
			{
				ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
				ArgumentOutOfRangeException.ThrowIfNegative(offset, nameof(offset));
				ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
				ArgumentOutOfRangeException.ThrowIfGreaterThan(offset + count, buffer.Length, nameof(count));
				if (_isDisposed)
				{
					throw new ObjectDisposedException(nameof(DotNetInvokeAsyncStream));
				}

				var memory = _pipe.Writer.GetMemory(count);

				buffer.AsMemory(offset, count).CopyTo(memory);

				_pipe.Writer.Advance(count);
			}

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
