using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Maps.Gtk
{
	/// <summary>
	/// <see cref="IGeocoding"/> over Nominatim, the OpenStreetMap geocoder.
	/// </summary>
	/// <remarks>
	/// Nominatim's usage policy allows at most one request per second and requires an identifying <c>User-Agent</c>: requests are spaced by
	/// <see cref="MinimumInterval"/> and use <see cref="GtkMapOptions.UserAgent"/>. Point <see cref="GtkMapOptions.NominatimUrl"/> at your own instance for
	/// anything beyond light interactive use. Failures (offline, throttled, unexpected body) return an empty result, not an exception.
	/// </remarks>
	public sealed class NominatimGeocoding : IGeocoding, IDisposable
	{
		readonly HttpClient _client;
		readonly string _root;
		readonly SemaphoreSlim _gate = new(1, 1);
		DateTime _lastRequest = DateTime.MinValue;

		/// <summary>Creates the geocoder from the map options.</summary>
		/// <param name="options">Supplies the service URL and the <c>User-Agent</c>.</param>
		/// <param name="handler">An optional message handler (tests); the client owns it.</param>
		public NominatimGeocoding(GtkMapOptions options, HttpMessageHandler? handler = null)
		{
			if (options is null)
				throw new ArgumentNullException(nameof(options));

			_root = options.NominatimUrl.TrimEnd('/');
			_client = handler is null ? new HttpClient() : new HttpClient(handler);
			_client.Timeout = TimeSpan.FromSeconds(20);
			if (!string.IsNullOrWhiteSpace(options.UserAgent))
				_client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", options.UserAgent);
			_client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", CultureInfo.CurrentUICulture.Name.Length > 0 ? CultureInfo.CurrentUICulture.Name : "en");
		}

		/// <summary>The minimum time between two requests. Default: 1 second; set to zero in tests.</summary>
		public TimeSpan MinimumInterval { get; set; } = TimeSpan.FromSeconds(1);

		/// <inheritdoc/>
		public async Task<IEnumerable<Location>> GetLocationsAsync(string address)
		{
			var result = new List<Location>();
			if (string.IsNullOrWhiteSpace(address))
				return result;

			var url = $"{_root}/search?format=jsonv2&limit=10&q={Uri.EscapeDataString(address)}";
			using var document = await GetAsync(url).ConfigureAwait(false);
			if (document is null || document.RootElement.ValueKind != JsonValueKind.Array)
				return result;

			foreach (var place in document.RootElement.EnumerateArray())
			{
				if (TryParseLocation(place, out var location))
					result.Add(location);
			}

			return result;
		}

		/// <inheritdoc/>
		public async Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude)
		{
			var result = new List<Placemark>();

			var url = string.Format(CultureInfo.InvariantCulture,
				"{0}/reverse?format=jsonv2&addressdetails=1&lat={1}&lon={2}",
				_root, latitude.ToString("R", CultureInfo.InvariantCulture), longitude.ToString("R", CultureInfo.InvariantCulture));

			using var document = await GetAsync(url).ConfigureAwait(false);
			if (document is null || document.RootElement.ValueKind != JsonValueKind.Object || document.RootElement.TryGetProperty("error", out _))
				return result;

			result.Add(ToPlacemark(document.RootElement, latitude, longitude));
			return result;
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			_client.Dispose();
			_gate.Dispose();
		}

		internal static bool TryParseLocation(JsonElement place, out Location location)
		{
			location = null!;
			if (place.ValueKind != JsonValueKind.Object)
				return false;

			if (!TryGetNumber(place, "lat", out var latitude) || !TryGetNumber(place, "lon", out var longitude))
				return false;

			location = new Location(latitude, longitude);
			return true;
		}

		internal static Placemark ToPlacemark(JsonElement place, double latitude, double longitude)
		{
			var placemark = new Placemark { Location = new Location(latitude, longitude) };

			if (place.TryGetProperty("address", out var address) && address.ValueKind == JsonValueKind.Object)
			{
				placemark.CountryName = Text(address, "country");
				placemark.CountryCode = Text(address, "country_code")?.ToUpperInvariant();
				placemark.AdminArea = Text(address, "state") ?? Text(address, "region");
				placemark.SubAdminArea = Text(address, "county") ?? Text(address, "state_district");
				placemark.Locality = Text(address, "city") ?? Text(address, "town") ?? Text(address, "village") ?? Text(address, "municipality");
				placemark.SubLocality = Text(address, "suburb") ?? Text(address, "neighbourhood") ?? Text(address, "city_district");
				placemark.Thoroughfare = Text(address, "road") ?? Text(address, "pedestrian") ?? Text(address, "footway");
				placemark.SubThoroughfare = Text(address, "house_number");
				placemark.PostalCode = Text(address, "postcode");
			}

			placemark.FeatureName = Text(place, "name") is { Length: > 0 } name ? name : placemark.Thoroughfare;
			return placemark;
		}

		static string? Text(JsonElement element, string property) =>
			element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String && value.GetString() is { Length: > 0 } text ? text : null;

		static bool TryGetNumber(JsonElement element, string property, out double value)
		{
			value = 0;
			if (!element.TryGetProperty(property, out var node))
				return false;

			return node.ValueKind switch
			{
				JsonValueKind.String => double.TryParse(node.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value),
				JsonValueKind.Number => node.TryGetDouble(out value),
				_ => false,
			};
		}

		async Task<JsonDocument?> GetAsync(string url)
		{
			await _gate.WaitAsync().ConfigureAwait(false);
			try
			{
				var wait = _lastRequest + MinimumInterval - DateTime.UtcNow;
				if (wait > TimeSpan.Zero)
					await Task.Delay(wait).ConfigureAwait(false);

				_lastRequest = DateTime.UtcNow;

				using var response = await _client.GetAsync(url).ConfigureAwait(false);
				if (!response.IsSuccessStatusCode)
					return null;

				var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
				return string.IsNullOrWhiteSpace(json) ? null : JsonDocument.Parse(json);
			}
			catch (Exception e) when (e is HttpRequestException or JsonException or TaskCanceledException or OperationCanceledException)
			{
				return null;
			}
			finally
			{
				_gate.Release();
			}
		}
	}
}
