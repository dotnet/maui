using System;
using System.Globalization;

namespace Microsoft.Maui.Maps.Gtk
{
	/// <summary>
	/// An XYZ ("slippy map") raster tile layer: a URL template plus the attribution the tile provider's terms require.
	/// </summary>
	/// <remarks>
	/// Templates understand <c>{z}</c>, <c>{x}</c> and <c>{y}</c>. The built-in sources point at public community servers whose
	/// usage policies forbid heavy or commercial traffic: set your own tile server on <see cref="GtkMapOptions"/> before shipping.
	/// </remarks>
	public sealed class GtkMapTileSource
	{
		/// <summary>Creates a tile source.</summary>
		/// <param name="urlTemplate">URL with <c>{z}</c>, <c>{x}</c>, <c>{y}</c> placeholders.</param>
		/// <param name="attribution">Text shown in the lower-right corner of the map (may be empty).</param>
		/// <param name="maxZoom">The highest zoom level the server provides (0 to 19).</param>
		/// <param name="overlayUrlTemplate">An optional transparent layer painted over the base layer.</param>
		public GtkMapTileSource(string urlTemplate, string? attribution, int maxZoom = 19, string? overlayUrlTemplate = null)
		{
			if (string.IsNullOrEmpty(urlTemplate))
				throw new ArgumentException("A URL template is required.", nameof(urlTemplate));

			UrlTemplate = urlTemplate;
			OverlayUrlTemplate = string.IsNullOrEmpty(overlayUrlTemplate) ? null : overlayUrlTemplate;
			Attribution = attribution ?? string.Empty;
			MaxZoom = Math.Max(MercatorProjection.MinZoomLevel, Math.Min(maxZoom, MercatorProjection.MaxZoomLevel));
		}

		/// <summary>The base layer URL template.</summary>
		public string UrlTemplate { get; }

		/// <summary>The optional overlay layer URL template.</summary>
		public string? OverlayUrlTemplate { get; }

		/// <summary>The attribution text.</summary>
		public string Attribution { get; }

		/// <summary>The highest zoom level of the server.</summary>
		public int MaxZoom { get; }

		/// <summary>OpenStreetMap standard tiles.</summary>
		public static GtkMapTileSource OpenStreetMap => new("https://tile.openstreetmap.org/{z}/{x}/{y}.png", "© OpenStreetMap contributors", 19);

		/// <summary>Esri World Imagery.</summary>
		public static GtkMapTileSource EsriSatellite => new(
			"https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}", "Imagery © Esri", 19);

		/// <summary>Esri World Imagery with the Esri boundaries-and-places overlay.</summary>
		public static GtkMapTileSource EsriHybrid => new(
			"https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}", "Imagery © Esri", 19,
			"https://server.arcgisonline.com/ArcGIS/rest/services/Reference/World_Boundaries_and_Places/MapServer/tile/{z}/{y}/{x}");

		/// <summary>Substitutes the tile coordinates into a URL template.</summary>
		/// <param name="template">The template.</param>
		/// <param name="zoom">The zoom level.</param>
		/// <param name="x">The tile column.</param>
		/// <param name="y">The tile row.</param>
		public static string FormatUrl(string template, int zoom, int x, int y)
		{
			if (template is null)
				throw new ArgumentNullException(nameof(template));

			return template
				.Replace("{z}", zoom.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
				.Replace("{x}", x.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
				.Replace("{y}", y.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
		}
	}
}
