using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Util;

namespace Microsoft.Maui.Platform
{
	internal class ImageGetter : Java.Lang.Object, Html.IImageGetter
	{
		const string Tag = "MauiImageGetter";
		const int MaxCacheEntries = 50;
		const long MaxImageBytes = 10 * 1024 * 1024;

		static readonly ConcurrentDictionary<string, BitmapDrawable> s_cache = new();
		static readonly ConcurrentDictionary<string, Task> s_inFlight = new();
		static readonly HttpClient s_httpClient = new() { Timeout = TimeSpan.FromSeconds(30) };

		readonly Resources _resources;
		readonly Action? _onImageLoaded;
		readonly SynchronizationContext? _syncContext;

		public ImageGetter(Resources resources, Action? onImageLoaded = null)
		{
			_resources = resources ?? throw new ArgumentNullException(nameof(resources));
			_onImageLoaded = onImageLoaded;
			_syncContext = SynchronizationContext.Current;
		}

		public Drawable? GetDrawable(string? source)
		{
			if (string.IsNullOrWhiteSpace(source))
				return null;

			if (s_cache.TryGetValue(source, out var cached))
				return cached;

			// Dedupe concurrent fetches for the same URL
			if (!s_inFlight.ContainsKey(source))
			{
				var task = LoadImageAsync(source);
				s_inFlight.TryAdd(source, task);
			}

			return new ColorDrawable(Color.Transparent);
		}

		async Task LoadImageAsync(string source)
		{
			try
			{
				if (!Uri.TryCreate(source, UriKind.Absolute, out var uri) ||
					(uri.Scheme != "https" && uri.Scheme != "http"))
				{
					Log.Warn(Tag, $"Skipping image with unsupported URI scheme: {source}");
					return;
				}

				using var response = await s_httpClient
					.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)
					.ConfigureAwait(false);

				response.EnsureSuccessStatusCode();

				var contentLength = response.Content.Headers.ContentLength;
				if (contentLength > MaxImageBytes)
				{
					Log.Warn(Tag, $"Image exceeds {MaxImageBytes} byte limit ({contentLength} bytes): {source}");
					return;
				}

				using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
				var bitmap = await BitmapFactory.DecodeStreamAsync(stream).ConfigureAwait(false);

				if (bitmap is null)
					return;

				var drawable = new BitmapDrawable(_resources, bitmap);
				drawable.SetBounds(0, 0, bitmap.Width, bitmap.Height);

				if (s_cache.Count >= MaxCacheEntries)
					EvictOldestEntries();

				s_cache[source] = drawable;

				if (_onImageLoaded is not null)
				{
					if (_syncContext is not null)
						_syncContext.Post(_ => _onImageLoaded.Invoke(), null);
					else
						_onImageLoaded.Invoke();
				}
			}
			catch (HttpRequestException ex)
			{
				Log.Warn(Tag, $"Failed to download image '{source}': {ex.Message}");
			}
			catch (System.IO.IOException ex)
			{
				Log.Warn(Tag, $"IO error loading image '{source}': {ex.Message}");
			}
			catch (OperationCanceledException)
			{
				Log.Debug(Tag, $"Image download timed out or was cancelled: {source}");
			}
			finally
			{
				s_inFlight.TryRemove(source, out _);
			}
		}

		static void EvictOldestEntries()
		{
			int toRemove = s_cache.Count / 2;
			foreach (var key in s_cache.Keys)
			{
				if (toRemove-- <= 0)
					break;

				if (s_cache.TryRemove(key, out var evicted))
					evicted.Bitmap?.Recycle();
			}
		}
	}
}

