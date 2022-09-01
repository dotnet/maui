#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Maps.Platform.MauiMKMapView;
#elif MONOANDROID
using Android.Gms.Maps;
using PlatformView = Android.Gms.Maps.MapView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
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
		GoogleMap? Map { get; }
#endif
		void UpdateMapElement(IMapElement element);
	}
}
