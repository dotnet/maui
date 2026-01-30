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
	public partial class MapHandler : IMapHandler
	{
		public static IPropertyMapper<IMap, IMapHandler> Mapper = new PropertyMapper<IMap, IMapHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IMap.MapType)] = MapMapType,
			[nameof(IMap.IsShowingUser)] = MapIsShowingUser,
			[nameof(IMap.IsScrollEnabled)] = MapIsScrollEnabled,
			[nameof(IMap.IsTrafficEnabled)] = MapIsTrafficEnabled,
			[nameof(IMap.IsZoomEnabled)] = MapIsZoomEnabled,
			[nameof(IMap.IsClusteringEnabled)] = MapIsClusteringEnabled,
			[nameof(IMap.Pins)] = MapPins,
			[nameof(IMap.Elements)] = MapElements,
		};


		public static CommandMapper<IMap, IMapHandler> CommandMapper = new(ViewCommandMapper)
		{
			[nameof(IMap.MoveToRegion)] = MapMoveToRegion,
			[nameof(IMapHandler.UpdateMapElement)] = MapUpdateMapElement,
		};

		public MapHandler() : base(Mapper, CommandMapper)
		{

		}

		public MapHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IMap IMapHandler.VirtualView => VirtualView;

		PlatformView IMapHandler.PlatformView => PlatformView;

		public static void MapUpdateMapElement(IMapHandler handler, IMap map, object? arg)
		{
			if (arg is not MapElementHandlerUpdate args)
				return;

			handler.UpdateMapElement(args.MapElement);
		}
	}
}
