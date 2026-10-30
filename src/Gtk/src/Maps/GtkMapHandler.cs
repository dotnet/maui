using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Maps.Handlers;

namespace Microsoft.Maui.Maps.Gtk
{
	/// <summary>
	/// <see cref="MapHandler"/> over a <see cref="MauiGtkMapView"/>: an OpenStreetMap-compatible slippy-tile map drawn with GTK 4 and Cairo.
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	/// <item>Supported: <c>MapType</c> (street/satellite/hybrid tile sources of <see cref="GtkMapOptions"/>), <c>IsScrollEnabled</c>, <c>IsZoomEnabled</c>,
	/// <c>Pins</c> (with info windows and <c>MarkerClicked</c>/<c>InfoWindowClicked</c>), <c>Elements</c> (polyline, polygon, circle), <c>MoveToRegion</c>,
	/// <c>VisibleRegion</c>, <c>Map.MapClicked</c>.</item>
	/// <item><c>IsShowingUser</c> draws a marker at <see cref="GtkMapOptions.UserLocationProvider"/>'s position (GTK has no location service).</item>
	/// <item><c>IsTrafficEnabled</c> is ignored: there is no free traffic tile layer.</item>
	/// </list>
	/// </remarks>
	public class GtkMapHandler : MapHandler
	{
		/// <summary>Creates the handler with the GTK mappers.</summary>
		public GtkMapHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the GTK one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the GTK one.</param>
		public GtkMapHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		GtkMapOptions Options => MauiContext?.Services.GetService<GtkMapOptions>() ?? s_defaultOptions;

		static readonly GtkMapOptions s_defaultOptions = new();

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new MauiGtkMapView(Options);

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is MauiGtkMapView map)
			{
				map.MapClicked += OnMapClicked;
				map.ViewportChanged += OnViewportChanged;
			}
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is MauiGtkMapView map)
			{
				map.MapClicked -= OnMapClicked;
				map.ViewportChanged -= OnViewportChanged;
				map.Detach();
			}

			base.DisconnectHandler(platformView);
		}

		// The virtual view is null once the handler disconnected; the generic VirtualView property would throw then.
		IMap? View => ((IElementHandler)this).VirtualView as IMap;

		void OnMapClicked(object? sender, MapPositionEventArgs e) => View?.Clicked(e.Position);

		void OnViewportChanged(object? sender, EventArgs e)
		{
			if (View is { } view && PlatformView is MauiGtkMapView map && map.Width > 1 && map.Height > 1)
				view.VisibleRegion = map.VisibleRegion;
		}

		internal static readonly IPropertyMapper<IMap, IMapHandler> GtkMapper = CreateMapper();

		internal static readonly CommandMapper<IMap, IMapHandler> GtkCommandMapper = CreateCommandMapper();

		static IPropertyMapper<IMap, IMapHandler> CreateMapper()
		{
			var mapper = new GtkMapperBuilder<IMap, IMapHandler>(MapHandler.Mapper).AddViewMappings();
			var commands = new GtkCommandMapperBuilder<IMap, IMapHandler>(MapHandler.CommandMapper).AddViewCommands();
			Configure(mapper, commands);
			return mapper.Build();
		}

		static CommandMapper<IMap, IMapHandler> CreateCommandMapper()
		{
			var mapper = new GtkMapperBuilder<IMap, IMapHandler>(MapHandler.Mapper).AddViewMappings();
			var commands = new GtkCommandMapperBuilder<IMap, IMapHandler>(MapHandler.CommandMapper).AddViewCommands();
			Configure(mapper, commands);
			return commands.Build();
		}

		// The stock Standard mappings of the map throw NotImplementedException, so none of these call them.
		static void Configure(GtkMapperBuilder<IMap, IMapHandler> mapper, GtkCommandMapperBuilder<IMap, IMapHandler> commands)
		{
			mapper.MapGtk<MauiGtkMapView>(nameof(IMap.MapType), (h, v, w) => w.TileSource = OptionsOf(h).GetTileSource(v.MapType), callStock: false);
			mapper.MapGtk<MauiGtkMapView>(nameof(IMap.IsShowingUser), (h, v, w) =>
			{
				w.IsShowingUser = v.IsShowingUser;
				w.UserLocation = v.IsShowingUser ? OptionsOf(h).UserLocationProvider?.Invoke() : null;
			}, callStock: false);
			mapper.MapGtk<MauiGtkMapView>(nameof(IMap.IsScrollEnabled), (h, v, w) => w.IsScrollEnabled = v.IsScrollEnabled, callStock: false);
			mapper.MapGtk<MauiGtkMapView>(nameof(IMap.IsZoomEnabled), (h, v, w) => w.IsZoomEnabled = v.IsZoomEnabled, callStock: false);
			mapper.MapGtk<MauiGtkMapView>(nameof(IMap.IsTrafficEnabled), (h, v, w) => { }, callStock: false);
			mapper.MapGtk<MauiGtkMapView>(nameof(IMap.Pins), (h, v, w) => w.SetPins(v.Pins), callStock: false);
			mapper.MapGtk<MauiGtkMapView>(nameof(IMap.Elements), (h, v, w) => w.SetElements(v.Elements), callStock: false);

			commands.MapGtk<MauiGtkMapView>(nameof(IMap.MoveToRegion), (h, v, args, w) =>
			{
				if (args is MapSpan span)
					w.MoveToRegion(span);
			}, callStock: false);
			commands.MapGtk<MauiGtkMapView>(nameof(IMapHandler.UpdateMapElement), (h, v, args, w) => w.Invalidate(), callStock: false);
		}

		static GtkMapOptions OptionsOf(IMapHandler handler) =>
			handler is GtkMapHandler gtk ? gtk.Options : s_defaultOptions;
	}
}
