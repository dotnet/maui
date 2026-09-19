#nullable disable
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Controls
{
	internal class StreamWrapper : Stream, IImageSourceCacheStream
	{
		readonly Stream _wrapped;
		readonly bool _canCache;
		readonly long? _expectedLength;
		readonly CancellationToken _cancellationToken;
		IDisposable _additionalDisposable;

		public StreamWrapper(Stream wrapped)
			: this(wrapped, null)
		{
		}

		public StreamWrapper(Stream wrapped, IDisposable additionalDisposable)
			: this(wrapped, additionalDisposable, canCache: true, expectedLength: null, default)
		{
		}

		StreamWrapper(
			Stream wrapped,
			IDisposable additionalDisposable,
			bool canCache,
			long? expectedLength,
			CancellationToken cancellationToken)
		{
			if (wrapped == null)
				throw new ArgumentNullException(nameof(wrapped));

			_wrapped = wrapped;
			_additionalDisposable = additionalDisposable;
			_canCache = canCache;
			_expectedLength = expectedLength;
			_cancellationToken = cancellationToken;
		}

		public bool CanCache => _canCache;

		public long? ExpectedLength => _expectedLength;

		public override bool CanRead
		{
			get { return _wrapped.CanRead; }
		}

		public override bool CanSeek
		{
			get { return _wrapped.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return _wrapped.CanWrite; }
		}

		public override long Length
		{
			get { return _wrapped.Length; }
		}

		public override long Position
		{
			get { return _wrapped.Position; }
			set { _wrapped.Position = value; }
		}

		public event EventHandler Disposed;

		public override void Flush()
		{
			_wrapped.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			_cancellationToken.ThrowIfCancellationRequested();
			try
			{
				return _wrapped.Read(buffer, offset, count);
			}
			catch (Exception ex) when (
				(ex is IOException || ex is ObjectDisposedException) &&
				_cancellationToken.IsCancellationRequested)
			{
				throw new OperationCanceledException(_cancellationToken);
			}
		}

		public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			_cancellationToken.ThrowIfCancellationRequested();
			try
			{
				return await _wrapped.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex) when (
				(ex is IOException || ex is ObjectDisposedException) &&
				_cancellationToken.IsCancellationRequested)
			{
				throw new OperationCanceledException(_cancellationToken);
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _wrapped.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			_wrapped.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_wrapped.Write(buffer, offset, count);
		}

		protected override void Dispose(bool disposing)
		{
			_wrapped.Dispose();
			Disposed?.Invoke(this, EventArgs.Empty);
			_additionalDisposable?.Dispose();
			_additionalDisposable = null;

			base.Dispose(disposing);
		}

		public static async Task<Stream> GetStreamAsync(
			Uri uri,
			CancellationToken cancellationToken,
			HttpClient client,
			CancellationToken responseCancellationToken = default)
		{
			HttpResponseMessage response = null;
			try
			{
				response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
				if (!response.IsSuccessStatusCode)
				{
					Application.Current?.FindMauiContext()?.CreateLogger<StreamWrapper>()?
							.LogWarning("Could not retrieve {Uri}, status code {StatusCode}", uri, response.StatusCode);

					response.Dispose();
					response = null;
					client.Dispose();
					return null;
				}

				var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
				// The response and client own the live network stream and must outlive the caller's reads.
				var owner = new HttpResponseOwner(response, client, responseCancellationToken);
				response = null;
				return new StreamWrapper(
					stream,
					owner,
					canCache: owner.Response.Headers.CacheControl?.NoStore != true,
					expectedLength: owner.Response.Content.Headers.ContentLength,
					responseCancellationToken);
			}
			catch
			{
				response?.Dispose();
				client.Dispose();
				throw;
			}
		}

		sealed class HttpResponseOwner : IDisposable
		{
			readonly HttpClient _client;
			readonly CancellationTokenRegistration _cancellationRegistration;
			int _disposed;

			public HttpResponseOwner(HttpResponseMessage response, HttpClient client, CancellationToken cancellationToken)
			{
				Response = response;
				_client = client;
				_cancellationRegistration = cancellationToken.Register(
					static state => ((HttpResponseOwner)state).DisposeResources(),
					this);
			}

			public HttpResponseMessage Response { get; }

			public void Dispose()
			{
				_cancellationRegistration.Dispose();
				DisposeResources();
			}

			void DisposeResources()
			{
				if (Interlocked.Exchange(ref _disposed, 1) != 0)
					return;

				Response.Dispose();
				_client.Dispose();
			}
		}
	}
}