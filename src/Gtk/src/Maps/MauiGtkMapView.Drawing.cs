using System;
using System.Collections.Generic;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Graphics;
using MauiColor = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.Maps.Gtk
{
	public sealed partial class MauiGtkMapView
	{
		const double PinHeadRadius = 8.0;
		const double PinHeadOffset = 20.0;
		const double PinHitPadding = 4.0;
		const double DefaultStrokeWidth = 3.0;
		const string TitleFont = "Sans Bold 9";
		const string BodyFont = "Sans 8";
		const string AttributionFont = "Sans 7";

		static readonly Gdk.RGBA s_water = new() { Red = 0.667f, Green = 0.788f, Blue = 0.859f, Alpha = 1f };
		static readonly Gdk.RGBA s_land = new() { Red = 0.918f, Green = 0.906f, Blue = 0.878f, Alpha = 1f };
		static readonly Gdk.RGBA s_grid = new() { Red = 0.847f, Green = 0.831f, Blue = 0.792f, Alpha = 1f };

		IMapPin[] _pins = Array.Empty<IMapPin>();
		IMapElement[] _elements = Array.Empty<IMapElement>();
		IMapPin? _selectedPin;
		(double X, double Y, double Width, double Height)? _infoWindow;

		/// <summary>The pins shown (a snapshot of <c>IMap.Pins</c>).</summary>
		public IReadOnlyList<IMapPin> Pins => _pins;

		/// <summary>Number of tiles decoded and ready to paint.</summary>
		internal int LoadedTileCount => _textures.Count;

		/// <summary>The info window rectangle of the last paint, or <see langword="null"/>.</summary>
		internal (double X, double Y, double Width, double Height)? InfoWindowBounds => _infoWindow;

		/// <summary>The pin whose info window is open, or <see langword="null"/>.</summary>
		public IMapPin? SelectedPin => _selectedPin;

		/// <summary>Replaces the pins.</summary>
		/// <param name="pins">The pins; the order is the z-order (the last is on top).</param>
		public void SetPins(IEnumerable<IMapPin> pins)
		{
			_pins = pins is null ? Array.Empty<IMapPin>() : new List<IMapPin>(pins).ToArray();
			if (_selectedPin is not null && Array.IndexOf(_pins, _selectedPin) < 0)
				SelectPin(null);
			QueueDraw();
		}

		/// <summary>Replaces the map elements (polylines, polygons, circles). They are read when drawing, so property changes need only <see cref="Invalidate"/>.</summary>
		/// <param name="elements">The elements; the order is the z-order.</param>
		public void SetElements(IEnumerable<IMapElement> elements)
		{
			_elements = elements is null ? Array.Empty<IMapElement>() : new List<IMapElement>(elements).ToArray();
			QueueDraw();
		}

		/// <summary>Repaints the map (for example after a map element changed).</summary>
		public void Invalidate() => QueueDraw();

		void SelectPin(IMapPin? pin)
		{
			if (ReferenceEquals(_selectedPin, pin))
				return;

			_selectedPin = pin;
			_infoWindow = null;
			QueueDraw();
		}

		/// <summary>The topmost pin whose marker contains the widget pixel, or <see langword="null"/>.</summary>
		public IMapPin? PinAt(double px, double py)
		{
			var viewport = Viewport;

			for (int i = _pins.Length - 1; i >= 0; i--)
			{
				var location = _pins[i].Location;
				if (location is null)
					continue;

				var (x, y) = viewport.ToPixel(location.Latitude, location.Longitude);
				if (px >= x - PinHeadRadius - PinHitPadding && px <= x + PinHeadRadius + PinHitPadding &&
					py >= y - PinHeadOffset - PinHeadRadius - PinHitPadding && py <= y + PinHitPadding)
					return _pins[i];
			}

			return null;
		}

		bool InfoWindowContains(double px, double py) =>
			_infoWindow is { } r && px >= r.X && px <= r.X + r.Width && py >= r.Y && py <= r.Y + r.Height;

		/// <inheritdoc/>
		protected override void OnSnapshot(global::Gtk.Snapshot snapshot)
		{
			int width = Width;
			int height = Height;
			if (width <= 0 || height <= 0)
				return;

			var bounds = Platform.Rect(0, 0, width, height);
			snapshot.PushClip(bounds);
			snapshot.AppendColor(s_water, bounds);

			var viewport = Viewport;
			DrawTiles(snapshot, viewport, width, height);

			using (var cr = snapshot.AppendCairo(bounds))
				DrawOverlay(cr, viewport, width, height);

			snapshot.Pop();
		}

		void DrawTiles(global::Gtk.Snapshot snapshot, MapViewport viewport, int width, int height)
		{
			int zoom = viewport.Zoom;
			int tileCount = MercatorProjection.TileCount(zoom);
			double originX = viewport.OriginX;
			double originY = viewport.OriginY;

			int firstX = (int)Math.Floor(originX / TileSize);
			int lastX = (int)Math.Floor((originX + width - 1) / TileSize);
			int firstY = Math.Max(0, (int)Math.Floor(originY / TileSize));
			int lastY = Math.Min(tileCount - 1, (int)Math.Floor((originY + height - 1) / TileSize));

			for (int tileY = firstY; tileY <= lastY; tileY++)
			{
				for (int tileX = firstX; tileX <= lastX; tileX++)
				{
					int wrappedX = ((tileX % tileCount) + tileCount) % tileCount;
					var rect = Platform.Rect(tileX * TileSize - originX, tileY * TileSize - originY, TileSize, TileSize);

					if (!DrawTile(snapshot, _tileSource.UrlTemplate, zoom, wrappedX, tileY, rect))
						DrawPlaceholder(snapshot, rect);

					if (_tileSource.OverlayUrlTemplate is { } overlay)
						DrawTile(snapshot, overlay, zoom, wrappedX, tileY, rect);
				}
			}
		}

		bool DrawTile(global::Gtk.Snapshot snapshot, string template, int zoom, int x, int y, Graphene.Rect rect)
		{
			var key = new TileKey(template, zoom, x, y);
			if (_textures.TryGet(key, out var texture))
			{
				snapshot.AppendTexture(texture, rect);
				return true;
			}

			RequestTile(key, MapTileUrl(template, zoom, x, y));
			return false;
		}

		static string MapTileUrl(string template, int zoom, int x, int y) => GtkMapTileSource.FormatUrl(template, zoom, x, y);

		void RequestTile(TileKey key, string url)
		{
			_store.Request(url, this, bytes =>
			{
				if (!_alive)
					return;

				if (bytes is null)
				{
					QueueDraw();
					return;
				}

				try
				{
					_textures.Set(key, new Gdk.Texture(new GLib.Bytes(bytes)));
				}
				catch (Exception)
				{
					// Not an image (an HTML error page, a truncated body): treat it as a miss.
					_store.MarkFailed(url);
				}

				QueueDraw();
			});
		}

		static void DrawPlaceholder(global::Gtk.Snapshot snapshot, Graphene.Rect rect)
		{
			// A land-coloured tile with a hairline border: the map stays legible, and pins and shapes stay meaningful, while tiles load or offline.
			snapshot.AppendColor(s_land, rect);
			var line = Platform.Rect(rect.X, rect.Y, rect.Width, 1);
			snapshot.AppendColor(s_grid, line);
			snapshot.AppendColor(s_grid, Platform.Rect(rect.X, rect.Y, 1, rect.Height));
		}

		void DrawOverlay(Cairo.Context cr, MapViewport viewport, int width, int height)
		{
			foreach (var element in _elements)
			{
				switch (element)
				{
					case ICircleMapElement circle:
						DrawCircle(cr, viewport, circle);
						break;
					case IGeoPathMapElement path:
						DrawPath(cr, viewport, path);
						break;
				}
			}

			if (_showUser && _userLocation is { } user)
			{
				var (ux, uy) = viewport.ToPixel(user.Latitude, user.Longitude);
				DrawUserLocation(cr, ux, uy);
			}

			foreach (var pin in _pins)
			{
				if (pin.Location is not { } location)
					continue;

				var (x, y) = viewport.ToPixel(location.Latitude, location.Longitude);
				if (x < -TileSize || y < -TileSize || x > width + TileSize || y > height + TileSize)
					continue;

				DrawPin(cr, pin, x, y);
			}

			_infoWindow = null;
			if (_selectedPin is { Location: { } selected } selectedPin)
			{
				var (x, y) = viewport.ToPixel(selected.Latitude, selected.Longitude);
				DrawInfoWindow(cr, selectedPin, x, y - PinHeadOffset - PinHeadRadius - 6, width);
			}

			DrawAttribution(cr, width, height);
		}

		static MauiColor? SolidColor(Paint? paint) => (paint as SolidPaint)?.Color;

		static void SetSource(Cairo.Context cr, MauiColor color) => cr.SetSourceRGBA(color.Red, color.Green, color.Blue, color.Alpha);

		static readonly MauiColor s_defaultStroke = MauiColor.FromRgb(51, 103, 214);

		static void Stroke(Cairo.Context cr, IMapElement element)
		{
			// IStroke.StrokeLineCap/Join/Dash are not implemented by the Controls map elements: only colour and thickness are read.
			SetSource(cr, SolidColor(element.Stroke) ?? s_defaultStroke);
			double thickness = element.StrokeThickness;
			cr.LineWidth = thickness > 0 ? thickness : DefaultStrokeWidth;
			cr.LineJoin = Cairo.LineJoin.Round;
			cr.LineCap = Cairo.LineCap.Round;
			cr.Stroke();
		}

		static void DrawPath(Cairo.Context cr, MapViewport viewport, IGeoPathMapElement path)
		{
			int count = path.Count;
			if (count < 2)
				return;

			cr.NewPath();
			for (int i = 0; i < count; i++)
			{
				var point = path[i];
				var (x, y) = viewport.ToPixel(point.Latitude, point.Longitude);
				if (i == 0)
					cr.MoveTo(x, y);
				else
					cr.LineTo(x, y);
			}

			// Polygons are the geo-path elements with a fill.
			if (path is IFilledMapElement filled)
			{
				cr.ClosePath();
				if (SolidColor(filled.Fill) is { } fill)
				{
					SetSource(cr, fill);
					cr.FillPreserve();
				}
			}

			Stroke(cr, path);
		}

		static void DrawCircle(Cairo.Context cr, MapViewport viewport, ICircleMapElement circle)
		{
			if (circle.Center is not { } center)
				return;

			double metersPerPixel = MercatorProjection.MetersPerPixel(center.Latitude, viewport.Zoom);
			double radius = metersPerPixel > 0 ? circle.Radius.Meters / metersPerPixel : 0;
			if (radius <= 0)
				return;

			var (x, y) = viewport.ToPixel(center.Latitude, center.Longitude);
			cr.NewPath();
			cr.Arc(x, y, radius, 0, 2 * Math.PI);

			if (SolidColor(circle.Fill) is { } fill)
			{
				SetSource(cr, fill);
				cr.FillPreserve();
			}

			Stroke(cr, circle);
		}

		static void DrawUserLocation(Cairo.Context cr, double x, double y)
		{
			cr.NewPath();
			cr.Arc(x, y, 12, 0, 2 * Math.PI);
			cr.SetSourceRGBA(0.15, 0.45, 0.90, 0.25);
			cr.Fill();

			cr.NewPath();
			cr.Arc(x, y, 6, 0, 2 * Math.PI);
			cr.SetSourceRGB(0.10, 0.40, 0.85);
			cr.FillPreserve();
			cr.SetSourceRGB(1, 1, 1);
			cr.LineWidth = 2;
			cr.Stroke();
		}

		static (double R, double G, double B) PinColor(IMapPin pin) =>
			pin is Controls.Maps.Pin controlsPin
				? controlsPin.Type switch
				{
					Controls.Maps.PinType.Place => (0.13, 0.42, 0.80),
					Controls.Maps.PinType.SavedPin => (0.13, 0.60, 0.29),
					Controls.Maps.PinType.SearchResult => (0.95, 0.55, 0.09),
					_ => (0.83, 0.18, 0.18),
				}
				: (0.83, 0.18, 0.18);

		void DrawPin(Cairo.Context cr, IMapPin pin, double x, double y)
		{
			var (r, g, b) = PinColor(pin);

			cr.NewPath();
			cr.Arc(x, y - PinHeadOffset, PinHeadRadius, 0, 2 * Math.PI);
			cr.ClosePath();
			cr.MoveTo(x - PinHeadRadius * 0.72, y - PinHeadOffset + PinHeadRadius * 0.70);
			cr.LineTo(x, y);
			cr.LineTo(x + PinHeadRadius * 0.72, y - PinHeadOffset + PinHeadRadius * 0.70);
			cr.ClosePath();

			cr.SetSourceRGB(r, g, b);
			cr.FillPreserve();
			cr.SetSourceRGB(1, 1, 1);
			cr.LineWidth = 1.5;
			cr.Stroke();

			cr.NewPath();
			cr.Arc(x, y - PinHeadOffset, PinHeadRadius * 0.38, 0, 2 * Math.PI);
			cr.SetSourceRGB(1, 1, 1);
			cr.Fill();
		}

		void DrawInfoWindow(Cairo.Context cr, IMapPin pin, double anchorX, double bottom, int viewWidth)
		{
			string title = pin.Label ?? string.Empty;
			string body = pin.Address ?? string.Empty;
			if (title.Length == 0 && body.Length == 0)
				return;

			using var titleLayout = CreatePangoLayout(title);
			titleLayout.FontDescription = Pango.FontDescription.FromString(TitleFont);
			titleLayout.GetPixelSize(out int titleWidth, out int titleHeight);

			using var bodyLayout = CreatePangoLayout(body);
			bodyLayout.FontDescription = Pango.FontDescription.FromString(BodyFont);
			bodyLayout.GetPixelSize(out int bodyWidth, out int bodyHeight);

			const double PaddingX = 8;
			const double PaddingY = 5;
			double contentWidth = Math.Max(titleWidth, bodyWidth);
			double boxWidth = contentWidth + 2 * PaddingX;
			double boxHeight = (title.Length > 0 ? titleHeight : 0) + (body.Length > 0 ? bodyHeight : 0) + 2 * PaddingY;
			double boxX = Math.Max(2, Math.Min(viewWidth - boxWidth - 2, anchorX - boxWidth / 2));
			double boxY = bottom - boxHeight;

			_infoWindow = (boxX, boxY, boxWidth, boxHeight);

			RoundedRectangle(cr, boxX, boxY, boxWidth, boxHeight, 4);
			cr.SetSourceRGBA(1, 1, 1, 0.97);
			cr.FillPreserve();
			cr.SetSourceRGBA(0.25, 0.25, 0.25, 0.6);
			cr.LineWidth = 1;
			cr.Stroke();

			double textY = boxY + PaddingY;
			cr.SetSourceRGB(0.1, 0.1, 0.1);
			if (title.Length > 0)
			{
				cr.MoveTo(boxX + PaddingX, textY);
				Pango.CairoHelper.ShowLayout(cr, titleLayout);
				textY += titleHeight;
			}

			if (body.Length > 0)
			{
				cr.SetSourceRGB(0.35, 0.35, 0.35);
				cr.MoveTo(boxX + PaddingX, textY);
				Pango.CairoHelper.ShowLayout(cr, bodyLayout);
			}
		}

		void DrawAttribution(Cairo.Context cr, int width, int height)
		{
			string attribution = _tileSource.Attribution;
			if (string.IsNullOrEmpty(attribution))
				return;

			using var layout = CreatePangoLayout(attribution);
			layout.FontDescription = Pango.FontDescription.FromString(AttributionFont);
			layout.GetPixelSize(out int textWidth, out int textHeight);

			const double PaddingX = 4;
			const double PaddingY = 1;
			double boxWidth = textWidth + 2 * PaddingX;
			double boxHeight = textHeight + 2 * PaddingY;
			double boxX = width - boxWidth;
			double boxY = height - boxHeight;

			cr.SetSourceRGBA(1, 1, 1, 0.75);
			cr.Rectangle(boxX, boxY, boxWidth, boxHeight);
			cr.Fill();

			cr.SetSourceRGB(0.2, 0.2, 0.2);
			cr.MoveTo(boxX + PaddingX, boxY + PaddingY);
			Pango.CairoHelper.ShowLayout(cr, layout);
		}

		static void RoundedRectangle(Cairo.Context cr, double x, double y, double width, double height, double radius)
		{
			cr.NewPath();
			cr.Arc(x + radius, y + radius, radius, Math.PI, 1.5 * Math.PI);
			cr.Arc(x + width - radius, y + radius, radius, 1.5 * Math.PI, 2 * Math.PI);
			cr.Arc(x + width - radius, y + height - radius, radius, 0, 0.5 * Math.PI);
			cr.Arc(x + radius, y + height - radius, radius, 0.5 * Math.PI, Math.PI);
			cr.ClosePath();
		}

		static class Platform
		{
			public static Graphene.Rect Rect(double x, double y, double width, double height) =>
				Microsoft.Maui.Gtk.Platform.GtkInterop.ToGrapheneRect(x, y, width, height);
		}
	}
}
