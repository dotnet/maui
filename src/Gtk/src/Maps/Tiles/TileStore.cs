using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Maps.Gtk
{
	/// <summary>
	/// Fetches tile bytes through the disk cache and an <see cref="IGtkMapTileFetcher"/>, with a bounded number of concurrent downloads.
	/// </summary>
	/// <remarks>
	/// Thread-safe. The newest request is served first (the tiles being looked at, not the ones panned past). Completion callbacks are
	/// handed to <c>post</c>, which the map view points at the GTK main loop. A failed URL is not retried for <see cref="RetryAfter"/>.
	/// </remarks>
	internal sealed class TileStore
	{
		const int MaxQueued = 96;

		/// <summary>How long a failed tile is left alone before it is requested again.</summary>
		public static readonly TimeSpan RetryAfter = TimeSpan.FromSeconds(30);

		readonly IGtkMapTileFetcher _fetcher;
		readonly TileDiskCache? _disk;
		readonly int _maxConcurrent;
		readonly Action<Action> _post;
		readonly object _gate = new();
		readonly List<string> _queue = new();
		readonly Dictionary<string, Dictionary<object, Action<byte[]?>>> _waiting = new();
		readonly Dictionary<string, DateTime> _failedAt = new();
		int _inFlight;

		public TileStore(IGtkMapTileFetcher fetcher, TileDiskCache? disk, int maxConcurrent, Action<Action> post)
		{
			_fetcher = fetcher ?? throw new ArgumentNullException(nameof(fetcher));
			_disk = disk;
			_maxConcurrent = Math.Max(1, maxConcurrent);
			_post = post ?? throw new ArgumentNullException(nameof(post));
		}

		/// <summary>Number of downloads running now.</summary>
		public int InFlight { get { lock (_gate) return _inFlight; } }

		/// <summary>Number of URLs waiting for a download slot.</summary>
		public int Queued { get { lock (_gate) return _queue.Count; } }

		/// <summary>Whether <paramref name="url"/> failed recently and is being left alone.</summary>
		public bool HasFailedRecently(string url)
		{
			lock (_gate)
				return _failedAt.TryGetValue(url, out var at) && DateTime.UtcNow - at < RetryAfter;
		}

		/// <summary>
		/// Requests <paramref name="url"/>. <paramref name="completed"/> receives the bytes (or <see langword="null"/> on failure) through the post
		/// function. Requests for a URL already queued or in flight share one download; one callback is kept per <paramref name="owner"/>, so a
		/// view that asks again on every repaint does not pile callbacks up.
		/// </summary>
		public void Request(string url, object owner, Action<byte[]?> completed)
		{
			lock (_gate)
			{
				if (_failedAt.TryGetValue(url, out var at))
				{
					if (DateTime.UtcNow - at < RetryAfter)
						return;

					_failedAt.Remove(url);
				}

				if (_waiting.TryGetValue(url, out var callbacks))
				{
					callbacks[owner] = completed;
					// A tile asked for again is one the user is looking at: move it to the front.
					if (_queue.Remove(url))
						_queue.Add(url);
					return;
				}

				_waiting[url] = new Dictionary<object, Action<byte[]?>> { [owner] = completed };
				_queue.Add(url);

				while (_queue.Count > MaxQueued)
				{
					var dropped = _queue[0];
					_queue.RemoveAt(0);
					_waiting.Remove(dropped);
				}
			}

			Pump();
		}

		/// <summary>Marks a URL as failed (its bytes downloaded but could not be decoded).</summary>
		public void MarkFailed(string url)
		{
			lock (_gate)
				_failedAt[url] = DateTime.UtcNow;
		}

		void Pump()
		{
			while (true)
			{
				string url;
				lock (_gate)
				{
					if (_inFlight >= _maxConcurrent || _queue.Count == 0)
						return;

					url = _queue[_queue.Count - 1];
					_queue.RemoveAt(_queue.Count - 1);
					_inFlight++;
				}

				_ = Task.Run(() => RunAsync(url));
			}
		}

		async Task RunAsync(string url)
		{
			byte[]? bytes = null;
			try
			{
				bytes = _disk?.TryRead(url);
				if (bytes is null)
				{
					bytes = await _fetcher.FetchAsync(url, CancellationToken.None).ConfigureAwait(false);
					if (bytes is { Length: > 0 })
						_disk?.TryWrite(url, bytes);
				}
			}
			catch (Exception)
			{
				bytes = null;
			}

			if (bytes is { Length: 0 })
				bytes = null;

			Dictionary<object, Action<byte[]?>>? callbacks;
			lock (_gate)
			{
				_inFlight--;
				_waiting.Remove(url, out callbacks);
				if (bytes is null)
					_failedAt[url] = DateTime.UtcNow;
			}

			if (callbacks is not null)
			{
				var result = bytes;
				_post(() =>
				{
					foreach (var callback in callbacks.Values)
						callback(result);
				});
			}

			Pump();
		}
	}
}
