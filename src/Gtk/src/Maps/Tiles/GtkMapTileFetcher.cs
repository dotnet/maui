using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Maps.Gtk
{
	/// <summary>Downloads the bytes of a map tile. Replace it on <see cref="GtkMapOptions.TileFetcher"/> to serve tiles from anywhere.</summary>
	public interface IGtkMapTileFetcher
	{
		/// <summary>Fetches a tile.</summary>
		/// <param name="url">The tile URL.</param>
		/// <param name="cancellationToken">Cancels the download.</param>
		/// <returns>The encoded image (PNG/JPEG), or <see langword="null"/> when the tile is unavailable.</returns>
		Task<byte[]?> FetchAsync(string url, CancellationToken cancellationToken);
	}

	/// <summary>The default fetcher: HTTP GET with an identifying <c>User-Agent</c> (required by the OpenStreetMap tile usage policy).</summary>
	public sealed class HttpGtkMapTileFetcher : IGtkMapTileFetcher, IDisposable
	{
		readonly HttpClient _client;

		/// <summary>Creates the fetcher.</summary>
		/// <param name="userAgent">The <c>User-Agent</c> header value.</param>
		/// <param name="handler">An optional message handler (tests); the client owns it.</param>
		public HttpGtkMapTileFetcher(string userAgent, HttpMessageHandler? handler = null)
		{
			_client = handler is null ? new HttpClient() : new HttpClient(handler);
			_client.Timeout = TimeSpan.FromSeconds(20);
			if (!string.IsNullOrWhiteSpace(userAgent))
				_client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);
		}

		/// <inheritdoc/>
		public async Task<byte[]?> FetchAsync(string url, CancellationToken cancellationToken)
		{
			try
			{
				using var response = await _client.GetAsync(url, cancellationToken).ConfigureAwait(false);
				if (!response.IsSuccessStatusCode)
					return null;

				return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (Exception) when (!cancellationToken.IsCancellationRequested)
			{
				// Offline, DNS failure, throttling: the caller draws a placeholder tile.
				return null;
			}
		}

		/// <inheritdoc/>
		public void Dispose() => _client.Dispose();
	}
}
