using System;
using System.Collections.Generic;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Maps.Gtk
{
	/// <summary>
	/// The platform view of a MAUI <c>Map</c>: a Web-Mercator slippy map in a custom <see cref="global::Gtk.Widget"/>.
	/// Tiles are decoded to <see cref="Gdk.Texture"/>s and appended to the snapshot; pins, polylines, polygons, circles, the info window and
	/// the attribution are drawn with Cairo on top (<c>Snapshot.AppendCairo</c>).
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	/// <item>Input: drag pans, wheel zooms about the pointer, pinch (<c>GestureZoom</c>) zooms about the fingers, double click zooms in, a click selects a pin or reports a map click.</item>
	/// <item>Measure: minimum 0, natural 256x256 (the map normally fills the space MAUI arranges for it).</item>
	/// <item>Tiles are drawn at 1 logical pixel per tile pixel; at a scale factor above 1 the tiles of the next zoom level(s) are used so they stay sharp (capped at the tile source's maximum zoom).</item>
	/// <item>All members must be used on the GTK main thread.</item>
	/// </list>
	/// </remarks>
	public sealed partial class MauiGtkMapView : global::Gtk.Widget
	{
		const int TileSize = MercatorProjection.TileSize;
		const int MaxTextureEntries = 200;
		const double DragThreshold = 4;

		readonly GtkMapOptions _options;
		readonly TileStore _store;
		readonly LruCache<TileKey, Gdk.Texture> _textures = new(MaxTextureEntries);

		double _centerLatitude;
		double _centerLongitude;
		int _zoom = 2;
		GtkMapTileSource _tileSource;
		MapSpan? _pendingRegion;

		bool _scrollEnabled = true;
		bool _zoomEnabled = true;
		bool _showUser;
		Location? _userLocation;

		bool _viewportUpdateQueued;
		bool _alive = true;

		global::Gtk.GestureDrag? _drag;
		global::Gtk.GestureClick? _click;
		global::Gtk.EventControllerScroll? _scroll;
		global::Gtk.EventControllerMotion? _motion;
		global::Gtk.GestureZoom? _pinch;
		int _pinchStartZoom;
		double _pinchX;
		double _pinchY;
		double _dragStartX;
		double _dragStartY;
		double _dragLastX;
		double _dragLastY;
		bool _dragMoved;
		double _pointerX;
		double _pointerY;

		/// <summary>Creates a map view that reads its tile servers and cache from <paramref name="options"/>.</summary>
		/// <param name="options">The options.</param>
		public MauiGtkMapView(GtkMapOptions options)
		{
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_store = options.GetTileStore();
			_tileSource = options.StreetTiles;
			Focusable = true;
			Overflow = global::Gtk.Overflow.Hidden;
			ConnectInput();
		}

		/// <summary>Creates a map view with default <see cref="GtkMapOptions"/>.</summary>
		public MauiGtkMapView() : this(new GtkMapOptions())
		{
		}

		/// <summary>Raised for a click that hit neither a pin nor an info window and was not a drag.</summary>
		public event EventHandler<MapPositionEventArgs>? MapClicked;

		/// <summary>Raised (coalesced, on the idle loop) whenever the visible region changed.</summary>
		public event EventHandler? ViewportChanged;

		/// <summary>The latitude of the centre.</summary>
		public double CenterLatitude => _centerLatitude;

		/// <summary>The longitude of the centre.</summary>
		public double CenterLongitude => _centerLongitude;

		/// <summary>The zoom level (0 = whole world in one tile).</summary>
		public int Zoom => _zoom;

		/// <summary>Whether dragging pans the map.</summary>
		public bool IsScrollEnabled
		{
			get => _scrollEnabled;
			set => _scrollEnabled = value;
		}

		/// <summary>Whether the wheel and double click zoom the map.</summary>
		public bool IsZoomEnabled
		{
			get => _zoomEnabled;
			set => _zoomEnabled = value;
		}

		/// <summary>The tile source; the zoom is clamped to its maximum.</summary>
		public GtkMapTileSource TileSource
		{
			get => _tileSource;
			set
			{
				value ??= _options.StreetTiles;
				if (ReferenceEquals(_tileSource, value))
					return;

				_tileSource = value;
				_zoom = Math.Min(_zoom, value.MaxZoom);
				QueueDraw();
			}
		}

		/// <summary>Whether a user marker is drawn at <see cref="UserLocation"/>.</summary>
		public bool IsShowingUser
		{
			get => _showUser;
			set
			{
				_showUser = value;
				QueueDraw();
			}
		}

		/// <summary>The position of the user marker, or <see langword="null"/>.</summary>
		public Location? UserLocation
		{
			get => _userLocation;
			set
			{
				_userLocation = value;
				QueueDraw();
			}
		}

		/// <summary>The viewport at the current allocation.</summary>
		internal MapViewport Viewport => new(_centerLatitude, _centerLongitude, _zoom, Width, Height);

		/// <summary>The region currently shown.</summary>
		public MapSpan VisibleRegion => Viewport.VisibleRegion;

		/// <summary>Centres on <paramref name="center"/> at <paramref name="zoom"/> (clamped to the tile source).</summary>
		/// <param name="center">The new centre.</param>
		/// <param name="zoom">The new zoom level.</param>
		public void MoveTo(Location center, int zoom)
		{
			_pendingRegion = null;
			_centerLatitude = MercatorProjection.ClampLatitude(center.Latitude);
			_centerLongitude = MercatorProjection.NormalizeLongitude(center.Longitude);
			_zoom = ClampZoom(zoom);
			ScheduleViewportUpdate();
		}

		/// <summary>
		/// Centres on <paramref name="span"/> and picks the largest zoom at which it still fits. Deferred until the first allocation when the
		/// widget has no size yet (the zoom depends on the viewport).
		/// </summary>
		/// <param name="span">The region to show.</param>
		public void MoveToRegion(MapSpan span)
		{
			if (span is null)
				throw new ArgumentNullException(nameof(span));

			if (Width <= 1 || Height <= 1)
			{
				_pendingRegion = span;
				return;
			}

			ApplyRegion(span);
		}

		void ApplyRegion(MapSpan span)
		{
			_pendingRegion = null;
			_centerLatitude = MercatorProjection.ClampLatitude(span.Center.Latitude);
			_centerLongitude = MercatorProjection.NormalizeLongitude(span.Center.Longitude);
			_zoom = ClampZoom(MercatorProjection.ZoomForSpan(_centerLatitude, span.LatitudeDegrees, span.LongitudeDegrees, Width, Height, _tileSource.MaxZoom));
			ScheduleViewportUpdate();
		}

		int ClampZoom(int zoom) => Math.Max(MercatorProjection.MinZoomLevel, Math.Min(zoom, _tileSource.MaxZoom));

		/// <summary>Moves the centre by a pixel offset (positive = the map content moves up/left).</summary>
		/// <param name="deltaX">Pixels to the east.</param>
		/// <param name="deltaY">Pixels to the south.</param>
		public void PanByPixels(double deltaX, double deltaY)
		{
			double world = MercatorProjection.WorldSize(_zoom);
			double centerX = MercatorProjection.LongitudeToX(_centerLongitude, _zoom) + deltaX;
			double centerY = MercatorProjection.LatitudeToY(_centerLatitude, _zoom) + deltaY;

			centerX = ((centerX % world) + world) % world;
			centerY = Math.Max(0, Math.Min(world, centerY));

			_centerLongitude = MercatorProjection.NormalizeLongitude(MercatorProjection.XToLongitude(centerX, _zoom));
			_centerLatitude = MercatorProjection.ClampLatitude(MercatorProjection.YToLatitude(centerY, _zoom));
			ScheduleViewportUpdate();
		}

		/// <summary>Zooms by <paramref name="steps"/> levels keeping the position under (<paramref name="px"/>, <paramref name="py"/>) fixed.</summary>
		/// <param name="steps">Levels to zoom in (negative zooms out).</param>
		/// <param name="px">Pointer x in widget pixels.</param>
		/// <param name="py">Pointer y in widget pixels.</param>
		public void ZoomAt(int steps, double px, double py)
		{
			int target = ClampZoom(_zoom + steps);
			if (target == _zoom)
				return;

			var anchor = Viewport.FromPixel(px, py);
			int width = Math.Max(1, Width);
			int height = Math.Max(1, Height);

			_zoom = target;

			double centerX = MercatorProjection.LongitudeToX(anchor.Longitude, _zoom) - px + width / 2.0;
			double centerY = MercatorProjection.LatitudeToY(anchor.Latitude, _zoom) - py + height / 2.0;

			_centerLongitude = MercatorProjection.NormalizeLongitude(MercatorProjection.XToLongitude(centerX, _zoom));
			_centerLatitude = MercatorProjection.ClampLatitude(MercatorProjection.YToLatitude(centerY, _zoom));
			ScheduleViewportUpdate();
		}

		/// <summary>Coalesces "the viewport moved" into one idle callback: it must not run inside a size-allocate or a snapshot.</summary>
		void ScheduleViewportUpdate()
		{
			QueueDraw();

			if (_viewportUpdateQueued)
				return;

			_viewportUpdateQueued = true;
			GLib.Idle.Add(() =>
			{
				_viewportUpdateQueued = false;
				if (_alive)
					ViewportChanged?.Invoke(this, EventArgs.Empty);
				return false;
			});
		}

		/// <summary>Stops tile callbacks and input; called when the handler disconnects.</summary>
		internal void Detach()
		{
			_alive = false;
			DisconnectInput();
			_textures.Clear();
		}

		/// <inheritdoc/>
		protected override void OnMeasure(global::Gtk.Orientation orientation, int forSize,
			out int minimum, out int natural, out int minimumBaseline, out int naturalBaseline)
		{
			minimum = 0;
			natural = TileSize;
			minimumBaseline = naturalBaseline = -1;
		}

		/// <inheritdoc/>
		protected override void OnSizeAllocate(int width, int height, int baseline)
		{
			if (_pendingRegion is { } span && width > 1 && height > 1)
			{
				// The zoom depends on the size; applied here (not deferred) because the new size is the one being allocated.
				_pendingRegion = null;
				_centerLatitude = MercatorProjection.ClampLatitude(span.Center.Latitude);
				_centerLongitude = MercatorProjection.NormalizeLongitude(span.Center.Longitude);
				_zoom = ClampZoom(MercatorProjection.ZoomForSpan(_centerLatitude, span.LatitudeDegrees, span.LongitudeDegrees, width, height, _tileSource.MaxZoom));
			}

			ScheduleViewportUpdate();
		}

		#region Input

		void ConnectInput()
		{
			_drag = new global::Gtk.GestureDrag { Button = 1 };
			_drag.DragStarted += OnDragStarted;
			_drag.DragUpdate += OnDragUpdate;
			AddController(_drag);

			_click = new global::Gtk.GestureClick { Button = 1 };
			_click.Pressed += OnPressed;
			_click.Released += OnReleased;
			AddController(_click);

			_scroll = new global::Gtk.EventControllerScroll(global::Gtk.EventControllerScrollFlags.Vertical);
			_scroll.Scroll += OnScroll;
			AddController(_scroll);

			_motion = new global::Gtk.EventControllerMotion();
			_motion.Motion += OnMotion;
			AddController(_motion);

			_pinch = new global::Gtk.GestureZoom();
			_pinch.Begin += OnPinchBegin;
			_pinch.ScaleChanged += OnPinchScaleChanged;
			AddController(_pinch);
		}

		void DisconnectInput()
		{
			if (_drag is not null)
			{
				_drag.DragStarted -= OnDragStarted;
				_drag.DragUpdate -= OnDragUpdate;
				RemoveController(_drag);
				_drag = null;
			}

			if (_click is not null)
			{
				_click.Pressed -= OnPressed;
				_click.Released -= OnReleased;
				RemoveController(_click);
				_click = null;
			}

			if (_scroll is not null)
			{
				_scroll.Scroll -= OnScroll;
				RemoveController(_scroll);
				_scroll = null;
			}

			if (_motion is not null)
			{
				_motion.Motion -= OnMotion;
				RemoveController(_motion);
				_motion = null;
			}

			if (_pinch is not null)
			{
				_pinch.Begin -= OnPinchBegin;
				_pinch.ScaleChanged -= OnPinchScaleChanged;
				RemoveController(_pinch);
				_pinch = null;
			}
		}

		void OnDragStarted(object o, global::Gtk.DragStartedArgs args) => BeginDrag(args.StartX, args.StartY);

		void OnDragUpdate(object o, global::Gtk.DragUpdateArgs args) => UpdateDrag(args.OffsetX, args.OffsetY);

		void OnPressed(object o, global::Gtk.PressedArgs args)
		{
			if (args.NPress == 2)
				HandleDoubleClick(args.X, args.Y);
		}

		void OnReleased(object o, global::Gtk.ReleasedArgs args)
		{
			if (args.NPress == 1)
				HandleClick(args.X, args.Y);
		}

		void OnScroll(object o, global::Gtk.ScrollArgs args)
		{
			// dy < 0 is "scroll up" (zoom in), like the other platforms.
			if (args.Dy < -0.01)
				HandleWheel(1);
			else if (args.Dy > 0.01)
				HandleWheel(-1);

			args.RetVal = true;
		}

		void OnPinchBegin(object o, global::Gtk.BeginArgs args)
		{
			double x = _pointerX, y = _pointerY;
			if (_pinch is not null && _pinch.GetBoundingBoxCenter(out double cx, out double cy))
			{
				x = cx;
				y = cy;
			}

			BeginPinch(x, y);
		}

		void OnPinchScaleChanged(object o, global::Gtk.ScaleChangedArgs args)
		{
			if (_pinch is not null && _pinch.GetBoundingBoxCenter(out double cx, out double cy))
			{
				_pinchX = cx;
				_pinchY = cy;
			}

			HandlePinch(args.Scale);
		}

		void OnMotion(object o, global::Gtk.MotionArgs args)
		{
			_pointerX = args.X;
			_pointerY = args.Y;
		}

		internal void BeginDrag(double x, double y)
		{
			_dragStartX = _dragLastX = x;
			_dragStartY = _dragLastY = y;
			_dragMoved = false;
			GrabFocus();
		}

		internal void UpdateDrag(double offsetX, double offsetY)
		{
			if (!_dragMoved && Math.Abs(offsetX) <= DragThreshold && Math.Abs(offsetY) <= DragThreshold)
				return;

			_dragMoved = true;

			double x = _dragStartX + offsetX;
			double y = _dragStartY + offsetY;
			double deltaX = x - _dragLastX;
			double deltaY = y - _dragLastY;
			_dragLastX = x;
			_dragLastY = y;

			if (_scrollEnabled)
				PanByPixels(-deltaX, -deltaY);
		}

		/// <summary>A pinch started: remembers the zoom and the focal point.</summary>
		internal void BeginPinch(double x, double y)
		{
			_pinchStartZoom = _zoom;
			_pinchX = x;
			_pinchY = y;
			GrabFocus();
		}

		/// <summary>
		/// A pinch moved: <paramref name="scale"/> is the factor since the gesture began (GTK's convention). The zoom is an integer tile level, so
		/// it follows <c>start + round(log2(scale))</c> while the focal point stays fixed.
		/// </summary>
		internal void HandlePinch(double scale)
		{
			if (!_zoomEnabled || !(scale > 0) || double.IsInfinity(scale))
				return;

			int target = ClampZoom(_pinchStartZoom + (int)Math.Round(Math.Log(scale, 2)));
			if (target != _zoom)
				ZoomAt(target - _zoom, _pinchX, _pinchY);
		}

		internal void HandleWheel(int steps)
		{
			if (_zoomEnabled)
				ZoomAt(steps, _pointerX, _pointerY);
		}

		internal void HandleDoubleClick(double x, double y)
		{
			if (_zoomEnabled)
				ZoomAt(1, x, y);
		}

		/// <summary>A completed click (not a drag): info window, pin, or map.</summary>
		internal void HandleClick(double x, double y)
		{
			if (_dragMoved)
			{
				_dragMoved = false;
				return;
			}

			if (_selectedPin is { } selected && InfoWindowContains(x, y))
			{
				selected.SendInfoWindowClick();
				return;
			}

			if (PinAt(x, y) is { } pin)
			{
				// SendMarkerClick returns PinClickedEventArgs.HideInfoWindow: a handler that sets it keeps the info window closed.
				bool hideInfoWindow = pin.SendMarkerClick();
				SelectPin(hideInfoWindow ? null : pin);
				return;
			}

			SelectPin(null);
			var position = Viewport.FromPixel(x, y);
			MapClicked?.Invoke(this, new MapPositionEventArgs(new Location(position.Latitude, position.Longitude)));
		}

		#endregion

		readonly struct TileKey : IEquatable<TileKey>
		{
			readonly string _template;
			readonly int _zoom;
			readonly int _x;
			readonly int _y;

			public TileKey(string template, int zoom, int x, int y)
			{
				_template = template;
				_zoom = zoom;
				_x = x;
				_y = y;
			}

			public bool Equals(TileKey other) => _zoom == other._zoom && _x == other._x && _y == other._y && string.Equals(_template, other._template, StringComparison.Ordinal);

			public override bool Equals(object? obj) => obj is TileKey other && Equals(other);

			public override int GetHashCode() => HashCode.Combine(_template, _zoom, _x, _y);
		}
	}

	/// <summary>The position of a map click.</summary>
	public sealed class MapPositionEventArgs : EventArgs
	{
		/// <summary>Creates the arguments.</summary>
		/// <param name="position">The clicked position.</param>
		public MapPositionEventArgs(Location position) => Position = position;

		/// <summary>The clicked position.</summary>
		public Location Position { get; }
	}
}
