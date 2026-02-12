#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Maps.Platform.MauiMKMapView;
#elif MONOANDROID
using PlatformView = Android.Gms.Maps.MapView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0 && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Maps.Handlers
{
	/// <summary>
	/// Handler for the <see cref="IMap"/> control that manages the platform-specific map implementation.
	/// </summary>
	public partial class MapHandler : IMapHandler
	{
		/// <summary>
		/// The property mapper that maps cross-platform properties to platform-specific methods.
		/// </summary>
		public static IPropertyMapper<IMap, IMapHandler> Mapper = new PropertyMapper<IMap, IMapHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IMap.MapType)] = MapMapType,
			[nameof(IMap.IsShowingUser)] = MapIsShowingUser,
			[nameof(IMap.IsScrollEnabled)] = MapIsScrollEnabled,
			[nameof(IMap.IsTrafficEnabled)] = MapIsTrafficEnabled,
			[nameof(IMap.IsZoomEnabled)] = MapIsZoomEnabled,
			[nameof(IMap.Pins)] = MapPins,
			[nameof(IMap.Elements)] = MapElements,
		};

		/// <summary>
		/// The command mapper that maps cross-platform commands to platform-specific methods.
		/// </summary>
		public static CommandMapper<IMap, IMapHandler> CommandMapper = new(ViewCommandMapper)
		{
			[nameof(IMap.MoveToRegion)] = MapMoveToRegion,
			[nameof(IMap.ShowInfoWindow)] = MapShowInfoWindow,
			[nameof(IMap.HideInfoWindow)] = MapHideInfoWindow,
			[nameof(IMapHandler.UpdateMapElement)] = MapUpdateMapElement,
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="MapHandler"/> class with default mappers.
		/// </summary>
		public MapHandler() : base(Mapper, CommandMapper)
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MapHandler"/> class with optional custom mappers.
		/// </summary>
		/// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
		/// <param name="commandMapper">The command mapper to use, or <see langword="null"/> to use the default.</param>
		public MapHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IMap IMapHandler.VirtualView => VirtualView;

		PlatformView IMapHandler.PlatformView => PlatformView;

		/// <summary>
		/// Maps the <see cref="IMapHandler.UpdateMapElement"/> command to the platform-specific implementation.
		/// </summary>
		/// <param name="handler">The map handler.</param>
		/// <param name="map">The map control.</param>
		/// <param name="arg">The <see cref="MapElementHandlerUpdate"/> argument.</param>
		public static void MapUpdateMapElement(IMapHandler handler, IMap map, object? arg)
		{
			if (arg is not MapElementHandlerUpdate args)
				return;

			handler.UpdateMapElement(args.MapElement);
		}

		/// <summary>
		/// Maps the <see cref="IMap.ShowInfoWindow"/> command to the platform-specific implementation.
		/// </summary>
		public static void MapShowInfoWindow(IMapHandler handler, IMap map, object? arg)
		{
			if (arg is IMapPin pin && handler is MapHandler mapHandler)
				mapHandler.ShowInfoWindow(pin);
		}

		/// <summary>
		/// Maps the <see cref="IMap.HideInfoWindow"/> command to the platform-specific implementation.
		/// </summary>
		public static void MapHideInfoWindow(IMapHandler handler, IMap map, object? arg)
		{
			if (arg is IMapPin pin && handler is MapHandler mapHandler)
				mapHandler.HideInfoWindow(pin);
		}
	}
}
