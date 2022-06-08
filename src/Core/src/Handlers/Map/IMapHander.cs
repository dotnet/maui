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
	public interface IMapHander : IViewHandler
	{
		new IMap VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}
