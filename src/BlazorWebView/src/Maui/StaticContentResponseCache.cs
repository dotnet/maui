using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	internal sealed class StaticContentResponseCache
	{
		internal const int MaxEntrySize = 8 * 1024 * 1024;
		private const int MaxEntryCount = 256;
		private const long MaxTotalSize = 32L * 1024 * 1024;

		private readonly object _lock = new();
		private readonly Dictionary<string, LinkedListNode<StaticContentResponse>> _entries = new(StringComparer.Ordinal);
		private readonly LinkedList<StaticContentResponse> _leastRecentlyUsed = new();
		private long _totalSize;

		public bool TryGet(string requestUri, out StaticContentResponse cachedResponse)
		{
			lock (_lock)
			{
				if (!_entries.TryGetValue(requestUri, out var node))
				{
					cachedResponse = null!;
					return false;
				}

				if (node.Value.ExpiresAt <= DateTimeOffset.UtcNow)
				{
					Remove(node);
					cachedResponse = null!;
					return false;
				}

				_leastRecentlyUsed.Remove(node);
				_leastRecentlyUsed.AddLast(node);
				cachedResponse = node.Value;
				return true;
			}
		}

		public void Set(StaticContentResponse cachedResponse)
		{
			if (cachedResponse.Content.Length > MaxEntrySize)
			{
				return;
			}

			lock (_lock)
			{
				if (_entries.TryGetValue(cachedResponse.RequestUri, out var existingNode))
				{
					Remove(existingNode);
				}

				while (_leastRecentlyUsed.Count >= MaxEntryCount ||
					(_leastRecentlyUsed.Count > 0 && _totalSize + cachedResponse.Content.Length > MaxTotalSize))
				{
					Remove(_leastRecentlyUsed.First!);
				}

				var node = _leastRecentlyUsed.AddLast(cachedResponse);
				_entries.Add(cachedResponse.RequestUri, node);
				_totalSize += cachedResponse.Content.Length;
			}
		}

		public void Remove(string requestUri)
		{
			lock (_lock)
			{
				if (_entries.TryGetValue(requestUri, out var node))
				{
					Remove(node);
				}
			}
		}

		public void Clear()
		{
			lock (_lock)
			{
				_entries.Clear();
				_leastRecentlyUsed.Clear();
				_totalSize = 0;
			}
		}

		private void Remove(LinkedListNode<StaticContentResponse> node)
		{
			_entries.Remove(node.Value.RequestUri);
			_leastRecentlyUsed.Remove(node);
			_totalSize -= node.Value.Content.Length;
		}
	}

	internal sealed class StaticContentResponse
	{
		public StaticContentResponse(
			string requestUri,
			string contentType,
			int statusCode,
			string statusMessage,
			IDictionary<string, string> headers,
			byte[] content,
			DateTimeOffset expiresAt)
		{
			RequestUri = requestUri;
			ContentType = contentType;
			StatusCode = statusCode;
			StatusMessage = statusMessage;
			Headers = new Dictionary<string, string>(headers, StringComparer.OrdinalIgnoreCase);
			Content = content;
			ExpiresAt = expiresAt;
		}

		public string RequestUri { get; }
		public string ContentType { get; }
		public int StatusCode { get; }
		public string StatusMessage { get; }
		public Dictionary<string, string> Headers { get; }
		public byte[] Content { get; }
		public DateTimeOffset ExpiresAt { get; }
	}

	internal static class StaticContentResponseCachePolicy
	{
		public static StaticContentCacheRequestBehavior GetRequestBehavior(
			string? method,
			IEnumerable<KeyValuePair<string, string>>? headers)
		{
			if (!string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase))
			{
				return StaticContentCacheRequestBehavior.Disabled;
			}

			var behavior = StaticContentCacheRequestBehavior.Default;
			if (headers is not null)
			{
				foreach (var header in headers)
				{
					if (string.Equals(header.Key, "Range", StringComparison.OrdinalIgnoreCase) ||
						string.Equals(header.Key, "Authorization", StringComparison.OrdinalIgnoreCase))
					{
						return StaticContentCacheRequestBehavior.Disabled;
					}

					if (string.Equals(header.Key, "Cache-Control", StringComparison.OrdinalIgnoreCase) &&
						CacheControlHeaderValue.TryParse(header.Value, out var cacheControl))
					{
						if (cacheControl.NoStore)
						{
							return StaticContentCacheRequestBehavior.Disabled;
						}

						if (cacheControl.NoCache ||
							(cacheControl.MaxAge is TimeSpan maxAge && maxAge <= TimeSpan.Zero))
						{
							behavior = StaticContentCacheRequestBehavior.Refresh;
						}
					}

					if (string.Equals(header.Key, "Pragma", StringComparison.OrdinalIgnoreCase) &&
						ContainsDirective(header.Value, "no-cache"))
					{
						behavior = StaticContentCacheRequestBehavior.Refresh;
					}
				}
			}

			return behavior;
		}

		public static bool TryGetCacheLifetime(string cacheControl, out TimeSpan cacheLifetime)
		{
			cacheLifetime = default;

			if (!CacheControlHeaderValue.TryParse(cacheControl, out var parsedCacheControl) ||
				parsedCacheControl.NoStore ||
				parsedCacheControl.NoCache ||
				parsedCacheControl.MaxAge is not TimeSpan maxAge ||
				maxAge <= TimeSpan.Zero)
			{
				return false;
			}

			cacheLifetime = maxAge;
			return true;
		}

		public static DateTimeOffset GetExpiration(TimeSpan cacheLifetime)
		{
			var now = DateTimeOffset.UtcNow;
			var maximumLifetime = DateTimeOffset.MaxValue - now;
			return cacheLifetime >= maximumLifetime
				? DateTimeOffset.MaxValue
				: now + cacheLifetime;
		}

		private static bool ContainsDirective(string value, string directive)
		{
			foreach (var item in value.Split(','))
			{
				if (string.Equals(item.Trim(), directive, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}
	}

	internal enum StaticContentCacheRequestBehavior
	{
		Disabled,
		Default,
		Refresh,
	}

	internal static class StaticContentResponseBuffer
	{
		public static bool TryBuffer(
			Stream content,
			string requestUri,
			ILogger? logger,
			out byte[] cachedContent,
			out Stream responseContent)
		{
			var buffer = new MemoryStream();
			var copyBuffer = new byte[81920];

			try
			{
				if (content.CanSeek && content.Length - content.Position > StaticContentResponseCache.MaxEntrySize)
				{
					cachedContent = Array.Empty<byte>();
					responseContent = content;
					return false;
				}

				int bytesRead;
				while ((bytesRead = content.Read(copyBuffer, 0, copyBuffer.Length)) != 0)
				{
					buffer.Write(copyBuffer, 0, bytesRead);
					if (buffer.Length > StaticContentResponseCache.MaxEntrySize)
					{
						cachedContent = Array.Empty<byte>();
						responseContent = new ConcatenatedReadStream(
							new MemoryStream(buffer.ToArray(), writable: false),
							content);
						return false;
					}
				}

				content.Dispose();
				cachedContent = buffer.ToArray();
				responseContent = new MemoryStream(cachedContent, writable: false);
				return true;
			}
			catch (IOException ex)
			{
				logger?.LogWarning(ex, "Unable to buffer static content response for {Url}; serving it without caching.", requestUri);
				cachedContent = Array.Empty<byte>();
				responseContent = new ConcatenatedReadStream(
					new MemoryStream(buffer.ToArray(), writable: false),
					content);
				return false;
			}
			catch
			{
				content.Dispose();
				throw;
			}
			finally
			{
				buffer.Dispose();
			}
		}

		public static async Task<(bool IsBuffered, byte[] CachedContent, Stream ResponseContent)> TryBufferAsync(
			Stream content,
			string requestUri,
			ILogger? logger)
		{
			var buffer = new MemoryStream();
			var copyBuffer = new byte[81920];

			try
			{
				if (content.CanSeek && content.Length - content.Position > StaticContentResponseCache.MaxEntrySize)
				{
					return (false, Array.Empty<byte>(), content);
				}

				int bytesRead;
				while ((bytesRead = await content.ReadAsync(copyBuffer.AsMemory())) != 0)
				{
					buffer.Write(copyBuffer, 0, bytesRead);
					if (buffer.Length > StaticContentResponseCache.MaxEntrySize)
					{
						return (
							false,
							Array.Empty<byte>(),
							new ConcatenatedReadStream(
								new MemoryStream(buffer.ToArray(), writable: false),
								content));
					}
				}

				content.Dispose();
				var cachedContent = buffer.ToArray();
				return (true, cachedContent, new MemoryStream(cachedContent, writable: false));
			}
			catch (IOException ex)
			{
				logger?.LogWarning(ex, "Unable to buffer static content response for {Url}; serving it without caching.", requestUri);
				return (
					false,
					Array.Empty<byte>(),
					new ConcatenatedReadStream(
						new MemoryStream(buffer.ToArray(), writable: false),
						content));
			}
			catch
			{
				content.Dispose();
				throw;
			}
			finally
			{
				buffer.Dispose();
			}
		}

		private sealed class ConcatenatedReadStream : Stream
		{
			private readonly Stream _prefix;
			private readonly Stream _remainder;

			public ConcatenatedReadStream(Stream prefix, Stream remainder)
			{
				_prefix = prefix;
				_remainder = remainder;
			}

			public override bool CanRead => true;
			public override bool CanSeek => false;
			public override bool CanWrite => false;
			public override long Length => throw new NotSupportedException();
			public override long Position
			{
				get => throw new NotSupportedException();
				set => throw new NotSupportedException();
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				var bytesRead = _prefix.Read(buffer, offset, count);
				return bytesRead != 0 ? bytesRead : _remainder.Read(buffer, offset, count);
			}

			public override void Flush()
			{
			}

			public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
			public override void SetLength(long value) => throw new NotSupportedException();
			public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_prefix.Dispose();
					_remainder.Dispose();
				}

				base.Dispose(disposing);
			}
		}
	}
}
