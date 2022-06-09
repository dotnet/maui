#if __IOS__ || MACCATALYST
using PlatformView = MapKit.MKMapView;
#elif MONOANDROID
using PlatformView = Android.Gms.Maps.MapView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.WebView2;
#elif TIZEN
using PlatformView = System.Object;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0 && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class MapHandler : IMapHander
	{
		public static IPropertyMapper<IMap, IMapHander> Mapper = new PropertyMapper<IMap, IMapHander>(ViewHandler.ViewMapper)
		{
			[nameof(IMap.MapType)] = MapMapType,
		};

		public static CommandMapper<IMap, IMapHander> CommandMapper = new(ViewCommandMapper);

		public MapHandler() : base(Mapper, CommandMapper)
		{

		}

		public MapHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IMap IMapHander.VirtualView => VirtualView;

		PlatformView IMapHander.PlatformView => PlatformView;
	}
}
