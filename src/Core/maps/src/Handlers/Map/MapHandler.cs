#if __IOS__ || MACCATALYST
using PlatformView = MapKit.MKMapView;
#elif MONOANDROID
using PlatformView = Android.Gms.Maps.MapView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.WebView2;
#elif TIZEN
using PlatformView = ElmSharp.EvasObject;
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
			[nameof(IMap.HasScrollEnabled)] = MapHasScrollEnabled,
			[nameof(IMap.HasTrafficEnabled)] = MapHasTrafficEnabled,
			[nameof(IMap.HasZoomEnabled)] = MapHasZoomEnabled,
			[nameof(IMap.Pins)] = MapPins,
			[nameof(IMap.Elements)] = MapElements,
		};


		public static CommandMapper<IMap, IMapHandler> CommandMapper = new(ViewCommandMapper)
		{
			[nameof(IMap.MoveToRegion)] = MapMoveToRegion,
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
	}
}
