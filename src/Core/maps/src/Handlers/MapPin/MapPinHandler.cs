#if __IOS__ || MACCATALYST
using PlatformView = MapKit.IMKAnnotation;
#elif MONOANDROID
using PlatformView = Android.Gms.Maps.Model.MarkerOptions;
#elif WINDOWS
using PlatformView = System.Object;
#elif TIZEN
using PlatformView = System.Object;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0 && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Maps.Handlers
{
	public partial class MapPinHandler : IMapPinHandler
	{
		public static IPropertyMapper<IMapPin, IMapPinHandler> Mapper = new PropertyMapper<IMapPin, IMapPinHandler>(ElementMapper)
		{
			[nameof(IMapPin.Location)] = MapLocation,
			[nameof(IMapPin.Label)] = MapLabel,
			[nameof(IMapPin.Address)] = MapAddress,
		};

		public MapPinHandler() : base(Mapper)
		{

		}

		public MapPinHandler(IPropertyMapper? mapper = null)
		: base(mapper ?? Mapper)
		{
		}

		IMapPin IMapPinHandler.VirtualView => VirtualView;

		PlatformView IMapPinHandler.PlatformView => PlatformView;
	}
}
