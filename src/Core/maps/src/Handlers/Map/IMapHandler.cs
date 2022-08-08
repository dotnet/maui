#if __IOS__ || MACCATALYST
using PlatformView = MapKit.MKMapView;
#elif MONOANDROID
using Android.Gms.Maps;
using PlatformView = Android.Gms.Maps.MapView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.WebView2;
#elif TIZEN
using PlatformView = System.Object;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0 && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Maps.Handlers
{
	public interface IMapHandler : IViewHandler
	{
		new IMap VirtualView { get; }
		new PlatformView PlatformView { get; }
#if MONOANDROID
		GoogleMap? Map { get; set; }
#endif

	}
}
